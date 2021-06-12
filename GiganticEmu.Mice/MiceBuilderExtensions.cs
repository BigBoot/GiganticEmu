using Microsoft.Extensions.DependencyInjection;

namespace GiganticEmu.Mice
{
    public static class MiceBuilderExtensions
    {
        public static IServiceCollection AddMice(this IServiceCollection services)
        {
            services.AddSingleton<MiceCommandHandler>();

            services.AddHostedService<MiceServer>();
            services.AddScoped<MiceClient>();

            return services;
        }
    }
}