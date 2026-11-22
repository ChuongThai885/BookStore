using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Common.Routing
{
    public sealed class LowercaseParameterTransformer: IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value) => value?.ToString()?.ToLowerInvariant();
    }
    public static class RoutingExtensions
    {
        public static IServiceCollection AddLowercaseRouting(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
            });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });

            return services;
        }
    }
}
