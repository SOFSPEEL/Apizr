﻿using Apizr.Configuring.Common;
using Apizr.Configuring.Proper;
using Apizr.Transferring.Requesting;
using System;

namespace Apizr.Configuring.Registry
{
    public interface IApizrDownloadRegistryBuilderBase<out TApizrDownloadRegistryBuilder, out TApizrRegistryBuilder, out TApizrProperOptionsBuilder, out TApizrCommonOptionsBuilder>
        where TApizrDownloadRegistryBuilder : IApizrDownloadRegistryBuilderBase<TApizrDownloadRegistryBuilder, TApizrRegistryBuilder, TApizrProperOptionsBuilder, TApizrCommonOptionsBuilder>
        where TApizrRegistryBuilder : IApizrRegistryBuilderBase
        where TApizrProperOptionsBuilder : IApizrGlobalProperOptionsBuilderBase
        where TApizrCommonOptionsBuilder : IApizrGlobalCommonOptionsBuilderBase
    {
        TApizrDownloadRegistryBuilder AddFor<TDownloadApi>(Action<TApizrProperOptionsBuilder> optionsBuilder = null)
            where TDownloadApi : IDownloadApi;

        TApizrDownloadRegistryBuilder AddFor<TDownloadApi, TDownloadParams>(Action<TApizrProperOptionsBuilder> optionsBuilder = null)
            where TDownloadApi : IDownloadApi<TDownloadParams>;
    }
}
