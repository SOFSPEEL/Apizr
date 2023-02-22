﻿using Apizr.Extending.Configuring.Common;
using Apizr.Extending.Configuring.Manager;
using System.IO;
using System.Linq;
using Apizr.Extending;
using Apizr.Transferring.Requesting;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Apizr.Optional.Requesting;
using Apizr.Optional.Requesting.Handling;
using Optional;

namespace Apizr
{
    public static class OptionalFileTransferOptionsBuilderExtensions
    {
        /// <summary>
        /// Let Apizr handle file transfer requests management with some mediation and optional result
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static IApizrExtendedCommonOptionsBuilder WithFileTransferOptionalMediation(this IApizrExtendedCommonOptionsBuilder optionsBuilder)
        {
            WithFileTransferOptionalMediation(optionsBuilder.ApizrOptions);

            return optionsBuilder;
        }
        /// <summary>
        /// Let Apizr handle file transfer requests management with some mediation and optional result
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static IApizrExtendedManagerOptionsBuilder WithFileTransferOptionalMediation(this IApizrExtendedManagerOptionsBuilder optionsBuilder)
        {
            WithFileTransferOptionalMediation(optionsBuilder.ApizrOptions);

            return optionsBuilder;
        }

        private static void WithFileTransferOptionalMediation(IApizrExtendedCommonOptions apizrOptions)
        {
            apizrOptions.PostRegistrationActions.Add((webApiType, services) =>
            {
                if (typeof(ITransferApiBase).IsAssignableFrom(webApiType))
                {
                    // Upload
                    if (typeof(IUploadApi).IsAssignableFrom(webApiType))
                    {
                        var requestType = typeof(UploadOptionalCommand<>).MakeGenericType(webApiType);
                        var requestHandlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(Option<Unit, ApizrException>));
                        var requestHandlerImplementationType = typeof(UploadOptionalCommandHandler<>).MakeGenericType(webApiType);

                        services.TryAddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                        // Short
                        if (typeof(IUploadApi) == webApiType || typeof(ITransferApi) == webApiType)
                        {
                            var shortRequestType = typeof(UploadOptionalCommand);
                            var shortRequestHandlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(shortRequestType, typeof(Option<Unit, ApizrException>));
                            var shortRequestHandlerImplementationType = typeof(UploadOptionalCommandHandler<>).MakeGenericType(webApiType);

                            services.TryAddSingleton(shortRequestHandlerServiceType, shortRequestHandlerImplementationType);
                        }
                    }

                    // Download
                    if (typeof(IDownloadApi).IsAssignableFrom(webApiType))
                    {
                        var requestType = typeof(DownloadOptionalQuery<>).MakeGenericType(webApiType);
                        var requestHandlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(Option<FileInfo, ApizrException>));
                        var requestHandlerImplementationType = typeof(DownloadOptionalQueryHandler<>).MakeGenericType(webApiType);

                        services.TryAddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                        // Short
                        if (typeof(IDownloadApi) == webApiType || typeof(ITransferApi) == webApiType)
                        {
                            var shortRequestType = typeof(DownloadOptionalQuery);
                            var shortRequestHandlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(shortRequestType, typeof(Option<FileInfo, ApizrException>));
                            var shortRequestHandlerImplementationType = typeof(DownloadOptionalQueryHandler<>).MakeGenericType(webApiType);

                            services.TryAddSingleton(shortRequestHandlerServiceType, shortRequestHandlerImplementationType);
                        }
                    }
                    else if (typeof(IDownloadApi<>).IsAssignableFromGenericType(webApiType))
                    {
                        var downloadParamsType = webApiType.GetInterfaces().FirstOrDefault(type => type.IsGenericType)?.GetGenericArguments().First();
                        var requestType = typeof(DownloadOptionalQuery<,>).MakeGenericType(webApiType, downloadParamsType);
                        var requestHandlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(Option<FileInfo, ApizrException>));
                        var requestHandlerImplementationType = typeof(DownloadOptionalQueryHandler<,>).MakeGenericType(webApiType, downloadParamsType);

                        services.TryAddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                    }
                }
            });
        }
    }
}
