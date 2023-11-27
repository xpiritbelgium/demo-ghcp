using CleanArchitecture.Application;
using CleanArchitecture.Domain.Documents;

namespace CleanArchitecture.Persistence.Documents
{
    public class PublishedDocumentsReadOnlyRepository: BaseReadOnlyRepository<PublishedDocument>, IAsyncReadOnlyRepository<PublishedDocument>
    {
        public PublishedDocumentsReadOnlyRepository(CleanArchitectureDbContext dbContext) : base(dbContext)
        {
        }

        // Add custom methods here
    }

    public class PublishedDocumentsRepository: BaseRepository<PublishedDocument>, IAsyncRepository<PublishedDocument>
    {
        public PublishedDocumentsRepository(CleanArchitectureDbContext dbContext) : base(dbContext)
        {
        }

        // Add custom methods here
    }
}
