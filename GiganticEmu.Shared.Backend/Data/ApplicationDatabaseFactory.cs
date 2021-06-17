using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GiganticEmu.Shared.Backend
{
    public class ApplicationDatabaseFactory : IDesignTimeDbContextFactory<ApplicationDatabase>, IDbContextFactory<ApplicationDatabase>
    {
        public ApplicationDatabase CreateDbContext(string[] args)
        {
            return CreateDbContext();
        }

        public ApplicationDatabase CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDatabase>()
                .UseNpgsql(Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRES"), options =>
                {
                    options.EnableRetryOnFailure();
                });

            return new ApplicationDatabase(builder.Options);
        }

        public static void Main(string[] args)
        {
        }
    }
}
