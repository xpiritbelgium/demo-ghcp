using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class ListPublishedDocumentsQuery: IRequest<IEnumerable<PublishedDocument>>
    {
    }

    public class ListPublishedDocumentsQueryHandler: IRequestHandler<ListPublishedDocumentsQuery, IEnumerable<PublishedDocument>>
    {
        private readonly IAsyncReadOnlyRepository<PublishedDocument> _repository;

        public ListPublishedDocumentsQueryHandler(IAsyncReadOnlyRepository<PublishedDocument> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PublishedDocument>> Handle(ListPublishedDocumentsQuery request, CancellationToken cancellationToken)
        {
            var documents = await _repository.ListAllAsync();

            return documents;
        }
    }
}
