using BlazorPro.BlazorSize;
using Microsoft.Extensions.DependencyInjection;

namespace Blazm.Components
{
    public static class GridServiceCollectionExtensions
    {
        public static IServiceCollection AddGrid(this IServiceCollection services)
        {
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
