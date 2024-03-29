﻿using BlazorPro.BlazorSize;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Blazm.Components
{
    public static class BlazmServiceCollectionExtensions
    {
        [Obsolete("Use AddBlazm instead")]
        public static IServiceCollection AddGrid(this IServiceCollection services)
        {
            return services.AddBlazm();
        }

        public static IServiceCollection AddBlazm(this IServiceCollection services)
        {
            services.AddScoped<ResizeListener>();
            services.AddResizeListener(options =>
            {
                options.ReportRate = 300;
                options.EnableLogging = true;
                options.SuppressInitEvent = false;
            });

            return services;
        }
    }
}
