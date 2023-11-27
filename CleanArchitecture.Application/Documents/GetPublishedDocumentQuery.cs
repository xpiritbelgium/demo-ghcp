using CleanArchitecture.Domain.Documents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Documents
{
    public class GetPublishedDocumentQuery: IRequest<PublishedDocument>
    {
        public GetPublishedDocumentQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetPublishedDocumentQueryHandler: IRequestHandler<GetPublishedDocumentQuery, PublishedDocument>
    {
        private readonly IAsyncReadOnlyRepository<PublishedDocument> _repository;

        public GetPublishedDocumentQueryHandler(IAsyncReadOnlyRepository<PublishedDocument> repository)
        {
            _repository = repository;
        }

        public async Task<PublishedDocument> Handle(GetPublishedDocumentQuery request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync(request.Id);

            if (document == null)
                throw new Exceptions.NotFoundException(nameof(Document), request.Id);

            return document;
        }
    }
}
