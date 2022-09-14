﻿using System;

namespace Apizr.Configuring.Registry
{
    internal interface IApizrConcurrentRegistry : IApizrRegistry
    {
        void AddOrUpdateManagerFor<TWebApi>(Func<IApizrManager<TWebApi>> managerFactory);
        void AddOrUpdateManagerFor(Type webApiType, Func<IApizrManager> managerFactory);
    }
}
