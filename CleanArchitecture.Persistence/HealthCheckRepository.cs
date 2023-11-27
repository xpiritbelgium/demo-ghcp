using CleanArchitecture.Application;
using CleanArchitecture.Domain.HealthChecks;

namespace CleanArchitecture.Persistence
{
    public class HealthCheckRepository : BaseRepository<HealthCheck>, IAsyncRepository<HealthCheck>
    {
        public HealthCheckRepository(CleanArchitectureDbContext dbContext) : base(dbContext)
        {
        }
    }
}
