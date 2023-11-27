using CleanArchitecture.Domain.Documents;

namespace CleanArchitecture.Application
{
    public interface IAsyncRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> GetPagedReponseAsync(int page, int size);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public interface IAsyncReadOnlyRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> GetPagedReponseAsync(int page, int size);
    }

    public interface IDocumentsRepository : IAsyncRepository<Document>
    {
        Task<IEnumerable<Document>> GetDocumentsToBePublishedAsync(CancellationToken cancellationToken);
    }
}
