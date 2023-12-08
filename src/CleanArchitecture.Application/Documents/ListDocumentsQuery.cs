using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class ListDocumentsQuery: IRequest<List<Document>>
    {
        //Todo: add filter criteria
    }

    public class ListDocumentsQueryHandler: IRequestHandler<ListDocumentsQuery, List<Document>>
    {
        private readonly IAsyncRepository<Document> _repository;

        public ListDocumentsQueryHandler(IAsyncRepository<Document> repository)
        {
            _repository = repository;
        }

        public async Task<List<Document>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
        {
            var documents = await _repository.ListAllAsync(); //Todo: add specialized list function to add filter criteria

            return new List<Document>(documents);
        }
    }
}
