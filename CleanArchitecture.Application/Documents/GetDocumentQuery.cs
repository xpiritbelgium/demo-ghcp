using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class GetDocumentQuery: IRequest<Document>
    {
        public GetDocumentQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, Document>
    {
        private readonly IAsyncRepository<Document> _repository;

        public GetDocumentQueryHandler(IAsyncRepository<Document> repository)
        {
            _repository = repository;
        }

        public async Task<Document> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync(request.Id);

            return document;
        }
    }
}
