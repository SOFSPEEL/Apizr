﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Apizr.Configuring.Request;
using Apizr.Extending;
using Apizr.Optional.Requesting.Handling.Base;
using MediatR;
using Optional;
using Optional.Async.Extensions;
using Polly;

namespace Apizr.Optional.Requesting.Handling
{
    /// <summary>
    /// The mediation execute optional unit request handler
    /// </summary>
    /// <typeparam name="TWebApi">The web api type</typeparam>
    /// <typeparam name="TModelData">The model data type</typeparam>
    /// <typeparam name="TApiData">The api data type</typeparam>
    public class ExecuteOptionalUnitRequestHandler<TWebApi, TModelData, TApiData> : ExecuteOptionalUnitRequestHandlerBase<TWebApi, TModelData, TApiData, ExecuteOptionalUnitRequest<TWebApi, TModelData, TApiData>, IApizrUnitRequestOptions, IApizrUnitRequestOptionsBuilder>
    {
        public ExecuteOptionalUnitRequestHandler(IApizrManager<TWebApi> webApiManager) : base(webApiManager)
        {
        }

        /// <inheritdoc />
        public override async Task<Option<Unit, ApizrException>> Handle(ExecuteOptionalUnitRequest<TWebApi, TModelData, TApiData> request, CancellationToken cancellationToken)
        {
            try
            {
                switch (request.ExecuteApiMethod)
                {
                    case Expression<Func<TWebApi, TApiData, Task>> executeApiMethod:
                        return await request
                            .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                            .MapAsync(async _ =>
                            {
                                await WebApiManager.ExecuteAsync<TModelData, TApiData>(executeApiMethod, request.ModelRequestData, (Action<IApizrCatchUnitRequestOptionsBuilder>)request.OptionsBuilder);

                                return Unit.Value;
                            }).ConfigureAwait(false);

                    case Expression<Func<IApizrUnitRequestOptions, TWebApi, TApiData, Task>> executeApiMethod:
                        return await request
                            .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                            .MapAsync(async _ =>
                            {
                                await WebApiManager.ExecuteAsync<TModelData, TApiData>((options, api, apiData) => executeApiMethod.Compile()(options, api, apiData), request.ModelRequestData, (Action<IApizrCatchUnitRequestOptionsBuilder>)request.OptionsBuilder);

                                return Unit.Value;
                            }).ConfigureAwait(false);

                    default:
                        throw new ApizrException(new NotImplementedException());
                }
            }
            catch (ApizrException e)
            {
                return Option.None<Unit, ApizrException>(e);
            }
        }
    }

    /// <summary>
    /// The mediation execute optional unit request handler
    /// </summary>
    /// <typeparam name="TWebApi">The web api type</typeparam>
    public class ExecuteOptionalUnitRequestHandler<TWebApi> : ExecuteOptionalUnitRequestHandlerBase<TWebApi, ExecuteOptionalUnitRequest<TWebApi>, IApizrUnitRequestOptions, IApizrUnitRequestOptionsBuilder>
    {
        public ExecuteOptionalUnitRequestHandler(IApizrManager<TWebApi> webApiManager) : base(webApiManager)
        {
        }

        /// <inheritdoc />
        public override async Task<Option<Unit, ApizrException>> Handle(ExecuteOptionalUnitRequest<TWebApi> request,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (request.ExecuteApiMethod)
                {
                    case Expression<Func<TWebApi, Task>> executeApiMethod:
                        return await request
                            .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                            .MapAsync(async _ =>
                            {
                                await WebApiManager.ExecuteAsync(executeApiMethod, (Action<IApizrCatchUnitRequestOptionsBuilder>) request.OptionsBuilder);

                                return Unit.Value;
                            }).ConfigureAwait(false);

                    case Expression<Func<IApizrUnitRequestOptions, TWebApi, Task>> executeApiMethod:
                        return await request
                            .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                            .MapAsync(async _ =>
                            {
                                await WebApiManager.ExecuteAsync((options, api) => executeApiMethod.Compile()(options, api), (Action<IApizrCatchUnitRequestOptionsBuilder>) request.OptionsBuilder);

                                return Unit.Value;
                            }).ConfigureAwait(false);

                    default:
                        throw new ApizrException(new NotImplementedException());
                }
            }
            catch (ApizrException e)
            {
                return Option.None<Unit, ApizrException>(e);
            }
        }
    }
}
