using Microsoft.Extensions.DependencyInjection;

namespace GiganticEmu.Mice
{
    public static class MiceBuilderExtensions
    {
        public static IServiceCollection AddMice(this IServiceCollection services)
        {
            services.AddSingleton<MiceCommandHandler>();
            services.AddSingleton<MiceServer>();
            services.AddSingleton<MatchmakingService>();

            services.AddHostedService<MiceServer>(p => p.GetRequiredService<MiceServer>());
            services.AddHostedService<MatchmakingService>(p => p.GetRequiredService<MatchmakingService>());
            services.AddScoped<MiceClient>();

            return services;
        }
    }
}