﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Apizr.Caching;
using Apizr.Connecting;
using Apizr.Logging;
using Apizr.Mapping;
using Apizr.Requesting;
using HttpTracer;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Apizr
{
    public class ApizrExtendedOptions : ApizrOptionsBase, IApizrExtendedOptions
    {
        public ApizrExtendedOptions(Type webApiType, Type apizrManagerType, Uri baseAddress,
            HttpMessageParts? httpTracerVerbosity, bool? isPriorityManagementEnabled,
            string[] assemblyPolicyRegistryKeys,
            string[] webApiPolicyRegistryKeys) : base(webApiType, baseAddress,
            httpTracerVerbosity, isPriorityManagementEnabled, assemblyPolicyRegistryKeys, webApiPolicyRegistryKeys)
        {
            ApizrManagerType = apizrManagerType;
            HttpClientHandlerFactory = _ => new HttpClientHandler();
            RefitSettingsFactory = _ => new RefitSettings();
            ConnectivityHandlerType = typeof(VoidConnectivityHandler);
            CacheHandlerType = typeof(VoidCacheHandler);
            LogHandlerType = typeof(DefaultLogHandler);
            MappingHandlerType = typeof(VoidMappingHandler);
            DelegatingHandlersExtendedFactories = new List<Func<IServiceProvider, DelegatingHandler>>();
            CrudEntities = new Dictionary<Type, CrudEntityAttribute>();
            WebApis = new Dictionary<Type, WebApiAttribute>();
            ObjectMappings = new Dictionary<Type, MappedWithAttribute>();
            PostRegistrationActions = new List<Action<IServiceCollection>>();
        }

        public Type ApizrManagerType { get; }
        public Type ConnectivityHandlerType { get; set; }
        public Type CacheHandlerType { get; set; }
        public Type LogHandlerType { get; set; }
        public Type MappingHandlerType { get; set; }
        public Func<IServiceProvider, HttpClientHandler> HttpClientHandlerFactory { get; set; }
        public Func<IServiceProvider, RefitSettings> RefitSettingsFactory { get; set; }
        public Action<IHttpClientBuilder> HttpClientBuilder { get; set; }
        public IList<Func<IServiceProvider, DelegatingHandler>> DelegatingHandlersExtendedFactories { get; }
        public IDictionary<Type, CrudEntityAttribute> CrudEntities { get; }
        public IDictionary<Type, WebApiAttribute> WebApis { get; }
        public IDictionary<Type, MappedWithAttribute> ObjectMappings { get; }
        public IList<Action<IServiceCollection>> PostRegistrationActions { get; }
    }
}
