using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiganticEmu.Shared.Backend
{
    public class ApplicationDatabase : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Friend> Friends { get; set; } = default!;
        public DbSet<GroupInvite> GroupInvites { get; set; } = default!;

        public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Cascade;
            }

            base.OnModelCreating(modelBuilder);
        }
    }

}
