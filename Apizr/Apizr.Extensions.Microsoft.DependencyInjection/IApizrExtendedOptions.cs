using System;
using System.Collections.Generic;
using System.Net.Http;
using Apizr.Logging;
using Apizr.Mapping;
using Apizr.Requesting;
using HttpTracer;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Apizr
{
    public interface IApizrExtendedOptions : IApizrOptionsBase
    {
        /// <summary>
        /// Type of the manager
        /// </summary>
        Type ApizrManagerType { get; }

        /// <summary>
        /// Type of the connectivity handler
        /// </summary>
        Type ConnectivityHandlerType { get; }

        /// <summary>
        /// Type of the cache handler
        /// </summary>
        Type CacheHandlerType { get; }

        /// <summary>
        /// Type of the logging handler
        /// </summary>
        Type LogHandlerType { get; }

        /// <summary>
        /// Type of the mapping handler
        /// </summary>
        Type MappingHandlerType { get; }

        /// <summary>
        /// Base address factory
        /// </summary>
        Func<IServiceProvider, Uri> BaseAddressFactory { get; }

        /// <summary>
        /// Request tracing verbosity factory
        /// </summary>
        Func<IServiceProvider, HttpMessageParts> HttpTracerVerbosityFactory { get; }

        /// <summary>
        /// Apizr executions tracing verbosity factory
        /// </summary>
        Func<IServiceProvider, ApizrLogLevel> ApizrVerbosityFactory { get; }

        /// <summary>
        /// HttpClientHandler factory
        /// </summary>
        Func<IServiceProvider, HttpClientHandler> HttpClientHandlerFactory { get; }

        /// <summary>
        /// Refit settings factory
        /// </summary>
        Func<IServiceProvider, RefitSettings> RefitSettingsFactory { get; }

        /// <summary>
        /// HttpClient builder
        /// </summary>
        Action<IHttpClientBuilder> HttpClientBuilder { get; }

        /// <summary>
        /// Delegating handlers factories
        /// </summary>
        IList<Func<IServiceProvider, IApizrOptionsBase, DelegatingHandler>> DelegatingHandlersExtendedFactories { get; }

        /// <summary>
        /// Entities auto registered with <see cref="IApizrManager{ICrudApi}"/>
        /// </summary>
        IDictionary<Type, CrudEntityAttribute> CrudEntities { get; }

        /// <summary>
        /// Web apis auto registered with <see cref="IApizrManager{TWebApi}"/>
        /// </summary>
        IDictionary<Type, WebApiAttribute> WebApis { get; }

        /// <summary>
        /// Mappings between api request object and model object used for classic auto registration
        /// </summary>
        IDictionary<Type, MappedWithAttribute> ObjectMappings { get; }

        /// <summary>
        /// Post registration actions
        /// </summary>
        IList<Action<IServiceCollection>> PostRegistrationActions { get; }
    }
}
