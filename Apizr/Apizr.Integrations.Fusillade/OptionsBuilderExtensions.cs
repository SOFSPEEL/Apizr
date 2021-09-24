﻿namespace Apizr
{
    public static partial class OptionsBuilderExtensions
    {
        public static T WithPriorityManagement<T>(this T builder) where T : IApizrOptionsBuilderBase
        {
            builder.SetPrimaryHttpMessageHandler((innerHandler, logger) => new PriorityHttpMessageHandler(innerHandler, logger));

            return builder;
        }
    }
}
