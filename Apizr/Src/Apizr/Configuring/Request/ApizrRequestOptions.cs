﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Apizr.Configuring.Shared;
using Apizr.Configuring.Shared.Context;
using Apizr.Extending;
using Apizr.Logging;
using Microsoft.Extensions.Logging;
using Polly;

namespace Apizr.Configuring.Request;

/// <inheritdoc cref="IApizrRequestOptions" />
public class ApizrRequestOptions : ApizrRequestOptionsBase, IApizrRequestOptions
{
    public ApizrRequestOptions(IApizrGlobalSharedRegistrationOptionsBase sharedOptions,
        IDictionary<string, object> handlersParameters,
        HttpTracerMode? httpTracerMode,
        HttpMessageParts? trafficVerbosity,
        TimeSpan? operationTimeout,
        TimeSpan? requestTimeout,
        params LogLevel[] logLevels) : 
        base(sharedOptions, httpTracerMode, trafficVerbosity, operationTimeout, requestTimeout, logLevels)
    {
        foreach (var handlersParameter in handlersParameters)
            HandlersParameters[handlersParameter.Key] = handlersParameter.Value;

        Headers = sharedOptions?.Headers?.TryGetValue(ApizrRegistrationMode.Set, out var headers) == true ? headers : [];
        _headersStore = sharedOptions?.Headers?.TryGetValue(ApizrRegistrationMode.Store, out var headersStore) == true ? headersStore : [];
    }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; internal set; }

    /// <inheritdoc />
    public bool ClearCache { get; internal set; }

    /// <inheritdoc />
    public IApizrResilienceContextOptions ResilienceContextOptions { get; internal set; }

    /// <inheritdoc />
    public IList<string> Headers { get; internal set; }

    private readonly IList<string> _headersStore;
    /// <inheritdoc />
    IList<string> IApizrRequestOptions.HeadersStore => _headersStore;

    /// <inheritdoc />
    Expression IApizrRequestOptions.OriginalExpression { get; set; }
}