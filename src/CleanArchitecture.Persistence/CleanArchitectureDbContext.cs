using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Documents;
using CleanArchitecture.Domain.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Persistence
{
    public class CleanArchitectureDbContext : DbContext
    {
        private readonly IApplicationUserAccessor _applicationUserAccessor;

        public CleanArchitectureDbContext(DbContextOptions<CleanArchitectureDbContext> options, IApplicationUserAccessor applicationUserAccessor)
            : base(options)
        {
            _applicationUserAccessor = applicationUserAccessor;
        }

        public DbSet<HealthCheck> HealthChecks { get; set; }
        public DbSet<Document> Documents { get; set; }

        public DbSet<PublishedDocument> PublishedDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanArchitectureDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                User? user = null;
                switch (entry.State)
                {
                    case EntityState.Added:
                        user = await _applicationUserAccessor.GetUserAsync();
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.CreatedBy = user.UserName;
                        break;
                    case EntityState.Modified:
                        user = await _applicationUserAccessor.GetUserAsync();
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = user.UserName;
                        break;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
