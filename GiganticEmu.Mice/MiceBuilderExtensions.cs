using System;
using Microsoft.Extensions.DependencyInjection;

namespace GiganticEmu.Mice
{
    public static class MiceBuilderExtensions
    {
        public static IServiceCollection AddMice(this IServiceCollection services, Action<MiceOptions>? setupAction = null)
        {
            var options = new MiceOptions();
            setupAction?.Invoke(options);

            services.AddSingleton<MiceOptions>(options);
            services.AddSingleton<MiceCommandHandler>();

            services.AddHostedService<MiceServer>();
            services.AddScoped<MiceClient>();

            return services;
        }
    }
}