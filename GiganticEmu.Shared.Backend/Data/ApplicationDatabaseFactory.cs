using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GiganticEmu.Shared.Backend
{
    public class ApplicationDatabaseFactory : IDesignTimeDbContextFactory<ApplicationDatabase>
    {
        public ApplicationDatabase CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDatabase>();

            builder.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRES"));

            return new ApplicationDatabase(builder.Options);
        }

        public static void Main(string[] args)
        {
        }
    }
}
