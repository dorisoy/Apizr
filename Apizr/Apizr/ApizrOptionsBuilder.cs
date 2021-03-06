using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Apizr.Authenticating;
using Apizr.Caching;
using Apizr.Connecting;
using Apizr.Logging;
using Apizr.Mapping;
using HttpTracer;
using Polly.Registry;
using Refit;

namespace Apizr
{
    public class ApizrOptionsBuilder : IApizrOptionsBuilder
    {
        protected readonly ApizrOptions Options;

        internal ApizrOptionsBuilder(ApizrOptions apizrOptions)
        {
            Options = apizrOptions;
        }

        public IApizrOptions ApizrOptions => Options;

        public IApizrOptionsBuilder WithBaseAddress(string baseAddress)
        {
            if (Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out var baseUri))
                Options.BaseAddressFactory = () => baseUri;

            return this;
        }

        public IApizrOptionsBuilder WithBaseAddress(Uri baseAddress)
        {
            Options.BaseAddressFactory = () => baseAddress;

            return this;
        }

        public IApizrOptionsBuilder WithBaseAddress(Func<string> baseAddressFactory)
        {
            Options.BaseAddressFactory = () =>
                Uri.TryCreate(baseAddressFactory.Invoke(), UriKind.RelativeOrAbsolute, out var baseUri)
                    ? baseUri
                    : null;

            return this;
        }

        public IApizrOptionsBuilder WithBaseAddress(Func<Uri> baseAddressFactory)
        {
            Options.BaseAddressFactory = baseAddressFactory;

            return this;
        }

        public IApizrOptionsBuilder WithLoggingVerbosity(HttpMessageParts trafficVerbosity,
            ApizrLogLevel apizrVerbosity)
            => WithLoggingVerbosity(() => trafficVerbosity, () => apizrVerbosity);

        public IApizrOptionsBuilder WithLoggingVerbosity(Func<HttpMessageParts> trafficVerbosityFactory, Func<ApizrLogLevel> apizrVerbosityFactory)
        {
            Options.HttpTracerVerbosityFactory = trafficVerbosityFactory;
            Options.ApizrVerbosityFactory = apizrVerbosityFactory;

            return this;
        }

        public IApizrOptionsBuilder WithHttpClientHandler(HttpClientHandler httpClientHandler)
            => WithHttpClientHandler(() => httpClientHandler);

        public IApizrOptionsBuilder WithHttpClientHandler(Func<HttpClientHandler> httpClientHandlerFactory)
        {
            Options.HttpClientHandlerFactory = httpClientHandlerFactory;

            return this;
        }

        public IApizrOptionsBuilder WithAuthenticationHandler<TAuthenticationHandler>(Func<ILogHandler, IApizrOptionsBase, TAuthenticationHandler> authenticationHandler) where TAuthenticationHandler : AuthenticationHandlerBase
        {
            Options.DelegatingHandlersFactories.Add(authenticationHandler);

            return this;
        }

        public IApizrOptionsBuilder WithAuthenticationHandler(Func<HttpRequestMessage, Task<string>> refreshToken)
        {
            var authenticationHandler = new Func<ILogHandler, IApizrOptionsBase, DelegatingHandler>((logHandler, options) =>
                new AuthenticationHandler(logHandler, options, refreshToken));
            Options.DelegatingHandlersFactories.Add(authenticationHandler);

            return this;
        }

        public IApizrOptionsBuilder WithAuthenticationHandler<TSettingsService>(TSettingsService settingsService,
            Expression<Func<TSettingsService, string>> tokenProperty,
            Func<HttpRequestMessage, Task<string>> refreshToken)
            => WithAuthenticationHandler(() => settingsService, tokenProperty, refreshToken);

        public IApizrOptionsBuilder WithAuthenticationHandler<TSettingsService>(
            Func<TSettingsService> settingsServiceFactory, Expression<Func<TSettingsService, string>> tokenProperty,
            Func<HttpRequestMessage, Task<string>> refreshToken)
        {
            var authenticationHandler = new Func<ILogHandler, IApizrOptionsBase, DelegatingHandler>((logHandler, options) =>
                new AuthenticationHandler<TSettingsService>(logHandler, options, settingsServiceFactory, tokenProperty, refreshToken));
            Options.DelegatingHandlersFactories.Add(authenticationHandler);

            return this;
        }

        public IApizrOptionsBuilder WithAuthenticationHandler<TSettingsService, TTokenService>(
            TSettingsService settingsService,
            Expression<Func<TSettingsService, string>> tokenProperty, TTokenService tokenService,
            Expression<Func<TTokenService, HttpRequestMessage, Task<string>>> refreshTokenMethod)
            => WithAuthenticationHandler(() => settingsService, tokenProperty, () => tokenService, refreshTokenMethod);

        public IApizrOptionsBuilder WithAuthenticationHandler<TSettingsService, TTokenService>(
            Func<TSettingsService> settingsServiceFactory, Expression<Func<TSettingsService, string>> tokenProperty,
            Func<TTokenService> tokenServiceFactory,
            Expression<Func<TTokenService, HttpRequestMessage, Task<string>>> refreshTokenMethod)
        {
            var authenticationHandler = new Func<ILogHandler, IApizrOptionsBase, DelegatingHandler>((logHandler, options) =>
                new AuthenticationHandler<TSettingsService, TTokenService>(logHandler,
                    options,
                    settingsServiceFactory, tokenProperty,
                    tokenServiceFactory, refreshTokenMethod));
            Options.DelegatingHandlersFactories.Add(authenticationHandler);

            return this;
        }

        public IApizrOptionsBuilder AddDelegatingHandler(DelegatingHandler delegatingHandler)
            => AddDelegatingHandler(_ => delegatingHandler);

        public IApizrOptionsBuilder AddDelegatingHandler(Func<ILogHandler, DelegatingHandler> delegatingHandlerFactory)
            => AddDelegatingHandler((logHandler, _) => delegatingHandlerFactory(logHandler));

        public IApizrOptionsBuilder AddDelegatingHandler(Func<ILogHandler, IApizrOptionsBase, DelegatingHandler> delegatingHandlerFactory)
        {
            Options.DelegatingHandlersFactories.Add(delegatingHandlerFactory);

            return this;
        }

        public IApizrOptionsBuilder WithRefitSettings(RefitSettings refitSettings)
            => WithRefitSettings(() => refitSettings);

        public IApizrOptionsBuilder WithRefitSettings(Func<RefitSettings> refitSettingsFactory)
        {
            Options.RefitSettingsFactory = refitSettingsFactory;

            return this;
        }

        public IApizrOptionsBuilder WithPolicyRegistry(IReadOnlyPolicyRegistry<string> policyRegistry)
            => WithPolicyRegistry(() => policyRegistry);

        public IApizrOptionsBuilder WithPolicyRegistry(Func<IReadOnlyPolicyRegistry<string>> policyRegistryFactory)
        {
            Options.PolicyRegistryFactory = policyRegistryFactory;

            return this;
        }

        public IApizrOptionsBuilder WithConnectivityHandler(IConnectivityHandler connectivityHandler)
            => WithConnectivityHandler(() => connectivityHandler);

        public IApizrOptionsBuilder WithConnectivityHandler(Func<IConnectivityHandler> connectivityHandlerFactory)
        {
            Options.ConnectivityHandlerFactory = connectivityHandlerFactory;

            return this;
        }

        public IApizrOptionsBuilder WithCacheHandler(ICacheHandler cacheHandler)
            => WithCacheHandler(() => cacheHandler);

        public IApizrOptionsBuilder WithCacheHandler(Func<ICacheHandler> cacheHandlerFactory)
        {
            Options.CacheHandlerFactory = cacheHandlerFactory;

            return this;
        }

        public IApizrOptionsBuilder WithLogHandler(ILogHandler logHandler)
            => WithLogHandler(() => logHandler);

        public IApizrOptionsBuilder WithLogHandler(Func<ILogHandler> logHandlerFactory)
        {
            Options.LogHandlerFactory = logHandlerFactory;

            return this;
        }

        public IApizrOptionsBuilder WithMappingHandler(IMappingHandler mappingHandler)
            => WithMappingHandler(() => mappingHandler);

        public IApizrOptionsBuilder WithMappingHandler(Func<IMappingHandler> mappingHandlerFactory)
        {
            Options.MappingHandlerFactory = mappingHandlerFactory;

            return this;
        }
    }
}
