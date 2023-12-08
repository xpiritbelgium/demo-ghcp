using CleanArchitecture.Application;
using CleanArchitecture.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Persistence.Documents
{
    public class DocumentsRepository : BaseRepository<Document>, IDocumentsRepository
    {
        public DocumentsRepository(CleanArchitectureDbContext dbContext) : base(dbContext)
        {
        }

        // Add custom methods here
        public async Task<IEnumerable<Document>> GetDocumentsToBePublishedAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Documents.Where(d => d.Status == DocumentStatus.Approved).ToListAsync(cancellationToken);
        }
    }
}
