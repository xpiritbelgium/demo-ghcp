using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class ListDocumentsToBePublishedQuery: IRequest<IEnumerable<Document>>
    {
    }

    public class ListDocumentsToBePublishedQueryHandler : IRequestHandler<ListDocumentsToBePublishedQuery, IEnumerable<Document>>
    {
        private readonly IDocumentsRepository _repository;

        public ListDocumentsToBePublishedQueryHandler(IDocumentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Document>> Handle(ListDocumentsToBePublishedQuery request, CancellationToken cancellationToken)
        {
            var documents = await _repository.GetDocumentsToBePublishedAsync(cancellationToken);

            return documents;
        }
    }
}
