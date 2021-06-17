using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GiganticEmu.Shared.Backend
{
    public static class ApplicationDatabaseBuilderExtensions
    {
        public static IServiceCollection AddApplicationDatabase(this IServiceCollection services)
        {
            services.AddDbContextFactory<ApplicationDatabase, ApplicationDatabaseFactory>();
            services.AddScoped<ApplicationDatabase>(p => p.GetRequiredService<IDbContextFactory<ApplicationDatabase>>().CreateDbContext());

            return services;
        }
    }
}