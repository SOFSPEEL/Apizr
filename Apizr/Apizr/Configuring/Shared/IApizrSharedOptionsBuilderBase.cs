﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Apizr.Logging;
using Microsoft.Extensions.Logging;
using Refit;

namespace Apizr.Configuring.Shared
{
    public interface IApizrSharedOptionsBuilderBase
    {
    }

    public interface IApizrSharedOptionsBuilderBase<out TApizrSharedOptions, out TApizrSharedOptionsBuilder> : IApizrSharedOptionsBuilderBase
        where TApizrSharedOptions : IApizrSharedOptionsBase
        where TApizrSharedOptionsBuilder : IApizrSharedOptionsBuilderBase<TApizrSharedOptions, TApizrSharedOptionsBuilder>
    {
        /// <summary>
        /// Apizr common options
        /// </summary>
        TApizrSharedOptions ApizrOptions { get; }

        /// <summary>
        /// Provide a method to refresh the authorization token when needed
        /// </summary>
        /// <param name="refreshTokenFactory">Refresh token method called when expired or empty</param>
        /// <returns></returns>
        TApizrSharedOptionsBuilder WithAuthenticationHandler(Func<HttpRequestMessage, Task<string>> refreshTokenFactory);

        /// <summary>
        /// Add a custom delegating handler
        /// </summary>
        /// <param name="delegatingHandler">A delegating handler</param>
        /// <returns></returns>
        TApizrSharedOptionsBuilder AddDelegatingHandler(DelegatingHandler delegatingHandler);

        /// <summary>
        /// Configure logging level for the api
        /// </summary>
        /// <param name="httpTracerMode"></param>
        /// <param name="trafficVerbosity">Http traffic tracing verbosity (default: All)</param>
        /// <param name="logLevel">Log level to apply while writing (default: Information)</param>
        /// <returns></returns>
        TApizrSharedOptionsBuilder WithLogging(HttpTracerMode httpTracerMode = HttpTracerMode.Everything, HttpMessageParts trafficVerbosity = HttpMessageParts.All, LogLevel logLevel = LogLevel.Information);
    }
}
