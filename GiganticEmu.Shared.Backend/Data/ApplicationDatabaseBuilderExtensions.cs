using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GiganticEmu.Shared.Backend
{
    public static class ApplicationDatabaseBuilderExtensions
    {

        public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, Action<ApplicationDatabaseOptions>? setupAction = null)
        {
            var options = new ApplicationDatabaseOptions();
            setupAction?.Invoke(options);

            services.AddDbContext<ApplicationDatabase>(c =>
            {
                if (options.ConnectionString is string connectionString)
                {
                    c.UseNpgsql(connectionString);
                }
            });

            return services;
        }
    }
}