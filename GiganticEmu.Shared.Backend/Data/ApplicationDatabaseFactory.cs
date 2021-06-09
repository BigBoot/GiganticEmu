using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GiganticEmu.Shared.Backend
{
    public class ApplicationDatabaseFactory : IDesignTimeDbContextFactory<ApplicationDatabase>
    {
        public ApplicationDatabase CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDatabase>();

            builder.UseSqlite("DataSource=app.db;Cache=Shared");

            return new ApplicationDatabase(builder.Options);
        }

        public static void Main(string[] args)
        {
        }
    }
}
