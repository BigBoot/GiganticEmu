using System;
using System.Linq;
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
                .UseNpgsql(Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRES")!, options =>
                {
                    options.EnableRetryOnFailure();
                });

            var db = new ApplicationDatabase(builder.Options);

            if (db.Database.GetPendingMigrations().Count() > 0)
            {
                db.Database.Migrate();
            }

            return db;
        }

        public static void Main(string[] args)
        {
        }
    }
}
