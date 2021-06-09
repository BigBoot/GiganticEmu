using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiganticEmu.Shared.Backend
{
    public class ApplicationDatabase : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            this.ValidateEntitíes();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.ValidateEntitíes();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ValidateEntitíes();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ValidateEntitíes();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Cascade;
            }

            base.OnModelCreating(modelBuilder);
        }

        private void ValidateEntitíes()
        {
            var addedOrModifiedEntities = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var errors = new List<EntityValidationResult>();
            var validationResults = new List<ValidationResult>();
            foreach (var entity in addedOrModifiedEntities)
            {
                if (!Validator.TryValidateObject(entity.Entity, new ValidationContext(entity.Entity), validationResults))
                {
                    errors.Add(new EntityValidationResult(entity.Entity, validationResults));
                    validationResults = new List<ValidationResult>();
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }
        }
    }

}
