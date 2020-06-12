﻿using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Apizr.Caching;
using Apizr.Connecting;
using Apizr.Logging;
using Apizr.Policing;
using Apizr.Prioritizing;
using Fusillade;
using HttpTracer;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Refit;
using HttpRequestMessageExtensions = Apizr.Policing.HttpRequestMessageExtensions;

namespace Apizr
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApizr<TWebApi>(this IServiceCollection services,
            Action<IApizrExtendedOptionsBuilder> optionsBuilder = null) =>
            AddApizr(services, typeof(TWebApi), typeof(ApizrManager<TWebApi>),
                optionsBuilder);
        public static IServiceCollection AddApizr(this IServiceCollection services, Type webApiType,
            Action<IApizrExtendedOptionsBuilder> optionsBuilder = null) =>
            AddApizr(services, webApiType, typeof(ApizrManager<>).MakeGenericType(webApiType),
                optionsBuilder);

        public static IServiceCollection AddApizr(
            this IServiceCollection services, Type webApiType, Type apizrManagerType, Action<IApizrExtendedOptionsBuilder> optionsBuilder = null)
        {
            if (!typeof(IApizrManager<>).MakeGenericType(webApiType).IsAssignableFrom(apizrManagerType))
                throw new ArgumentException(
                    $"Your Apizr manager class must inherit from IApizrManager generic interface or derived");

            var apizrOptions = CreateApizrExtendedOptions(webApiType, apizrManagerType, optionsBuilder);
            foreach (var priority in ((Priority[])Enum.GetValues(typeof(Priority))).Where(x => x != Priority.Explicit))
            {
                var builder = services.AddHttpClient(ForType(apizrOptions.WebApiType, priority))
                    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                    {
                        var logHandler = serviceProvider.GetRequiredService<ILogHandler>();
                        var handlerBuilder = new HttpHandlerBuilder(new HttpClientHandler
                        {
                            AutomaticDecompression = apizrOptions.DecompressionMethods
                        }, new HttpTracerLogWrapper(logHandler));
                        handlerBuilder.HttpTracerHandler.Verbosity = apizrOptions.HttpTracerVerbosity;

                        if (apizrOptions.PolicyRegistryKeys != null && apizrOptions.PolicyRegistryKeys.Any())
                        {
                            IPolicyRegistry<string> policyRegistry = null;
                            try
                            {
                                policyRegistry = serviceProvider.GetRequiredService<IPolicyRegistry<string>>();
                            }
                            catch (Exception)
                            {
                                logHandler.Write(
                                    $"Apizr - Global policies: You get some global policies but didn't register a {nameof(PolicyRegistry)} instance. Global policies will be ignored for  for {webApiType.Name} {priority} instance");
                            }

                            if (policyRegistry != null)
                            {
                                foreach (var policyRegistryKey in apizrOptions.PolicyRegistryKeys)
                                {
                                    if (policyRegistry.TryGet<IsPolicy>(policyRegistryKey, out var registeredPolicy))
                                    {
                                        logHandler.Write($"Apizr - Global policies: Found a policy with key {policyRegistryKey} for {webApiType.Name} {priority} instance");
                                        if (registeredPolicy is IAsyncPolicy<HttpResponseMessage> registeredPolicyForHttpResponseMessage)
                                        {
                                            var policySelector =
                                                new Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>(
                                                    request =>
                                                    {
                                                        var pollyContext = new Context().WithLogHandler(logHandler);
                                                        HttpRequestMessageExtensions.SetPolicyExecutionContext(request, pollyContext);
                                                        return registeredPolicyForHttpResponseMessage;
                                                    });
                                            handlerBuilder.AddHandler(new PolicyHttpMessageHandler(policySelector));

                                            logHandler.Write($"Apizr - Global policies: Policy with key {policyRegistryKey} will be applied to {webApiType.Name} {priority} instance");
                                        }
                                        else
                                        {
                                            logHandler.Write($"Apizr - Global policies: Policy with key {policyRegistryKey} is not of {typeof(IAsyncPolicy<HttpResponseMessage>)} type and will be ignored for {webApiType.Name} {priority} instance");
                                        }
                                    }
                                    else
                                    {
                                        logHandler.Write($"Apizr - Global policies: No policy found for key {policyRegistryKey} and will be ignored for  for {webApiType.Name} {priority} instance");
                                    }
                                } 
                            }
                        }

                        foreach (var handlerExtendedFactory in apizrOptions.DelegatingHandlersExtendedFactories)
                            handlerBuilder.AddHandler(handlerExtendedFactory.Invoke(serviceProvider));

                        var innerHandler = handlerBuilder.Build();
                        var primaryMessageHandler = new RateLimitedHttpMessageHandler(innerHandler, priority);

                        return primaryMessageHandler;
                    })
                    .AddTypedClient(typeof(ILazyPrioritizedWebApi<>).MakeGenericType(apizrOptions.WebApiType),
                        (client, serviceProvider) =>
                            Prioritize.TypeFor(apizrOptions.WebApiType, priority)
                                .GetConstructor(new[] {typeof(Func<object>)})
                                ?.Invoke(new object[]
                                {
                                    new Func<object>(() => RestService.For(apizrOptions.WebApiType, client,
                                        apizrOptions.RefitSettingsFactory()))
                                }));

                if (apizrOptions.BaseAddress != null)
                    builder.ConfigureHttpClient(x => x.BaseAddress = apizrOptions.BaseAddress);

                apizrOptions.HttpClientBuilder?.Invoke(builder);

                builder.ConfigureHttpClient(x =>
                {
                    if (x.BaseAddress == null)
                        throw new ArgumentNullException(nameof(x.BaseAddress), $"You must provide a valid web api uri with the {nameof(WebApiAttribute)} or the options builder");
                });
            }

            services.AddSingleton(typeof(IConnectivityHandler), apizrOptions.ConnectivityHandlerType);

            services.AddSingleton(typeof(ICacheProvider), apizrOptions.CacheProviderType);

            services.AddSingleton(typeof(ILogHandler), apizrOptions.LogHandlerType);

            services.AddSingleton(typeof(IApizrManager<>).MakeGenericType(apizrOptions.WebApiType), typeof(ApizrManager<>).MakeGenericType(apizrOptions.WebApiType));

            return services;
        }

        private static IApizrExtendedOptions CreateApizrExtendedOptions(Type webApiType, Type apizrManagerType, Action<IApizrExtendedOptionsBuilder> optionsBuilder = null)
        {
            var webApiAttribute = webApiType.GetTypeInfo().GetCustomAttribute<WebApiAttribute>(true);
            Uri.TryCreate(webApiAttribute?.BaseUri, UriKind.RelativeOrAbsolute, out var baseAddress);

            var traceAttribute = webApiType.GetTypeInfo().GetCustomAttribute<TraceAttribute>(true);

            var assemblyPolicyAttribute = webApiType.Assembly.GetCustomAttribute<PolicyAttribute>();

            var webApiPolicyAttribute = webApiType.GetTypeInfo().GetCustomAttribute<PolicyAttribute>(true);

            var builder = new ApizrExtendedOptionsBuilder(new ApizrExtendedOptions(webApiType, apizrManagerType, baseAddress,
                webApiAttribute?.DecompressionMethods, traceAttribute?.Verbosity, assemblyPolicyAttribute?.RegistryKeys,
                webApiPolicyAttribute?.RegistryKeys));

            optionsBuilder?.Invoke(builder);

            return builder.ApizrOptions;
        }

        private static string ForType(Type refitInterfaceType, Priority priority)
        {
            string typeName;

            if (refitInterfaceType.IsNested)
            {
                var className = "AutoGenerated" + priority + refitInterfaceType.DeclaringType.Name + refitInterfaceType.Name;
                typeName = refitInterfaceType.AssemblyQualifiedName.Replace(refitInterfaceType.DeclaringType.FullName + "+" + refitInterfaceType.Name, refitInterfaceType.Namespace + "." + className);
            }
            else
            {
                var className = "AutoGenerated" + priority + refitInterfaceType.Name;

                if (refitInterfaceType.Namespace == null)
                {
                    className = $"{className}.{className}";
                }

                typeName = refitInterfaceType.AssemblyQualifiedName.Replace(refitInterfaceType.Name, className);
            }

            return typeName;
        }
    }
}