using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class PublishDocumentCommand : IRequest<Unit>
    {
        public PublishDocumentCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }

    public class PublishDocumentCommandHandler : IRequestHandler<PublishDocumentCommand, Unit>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IBlobRepository _blobRepository;
        private readonly IAsyncRepository<PublishedDocument> _publishedDocumentRepository;

        public PublishDocumentCommandHandler(IAsyncRepository<Document> repository, IBlobRepository blobRepository, IAsyncRepository<PublishedDocument> publishedDocumentRepository)
        {
            _repository = repository;
            _blobRepository = blobRepository;
            _publishedDocumentRepository = publishedDocumentRepository;
        }
        public async Task<Unit> Handle(PublishDocumentCommand request, CancellationToken cancellationToke = default)
        {
            //TODO: add validator

            var document = await _repository.GetByIdAsync(request.Id);
            if (document == null)
            {
                throw new Exceptions.NotFoundException(nameof(document), request.Id);
            }

            if (string.IsNullOrWhiteSpace(document.BlobName))
                throw new Exceptions.BadRequestException(nameof(document.BlobName)); //TODO: replace with validator logic

            var publishedBlob = await _blobRepository.PublishBlob(document.BlobName);

            document.Status = DocumentStatus.Published;
            document.PublishedDocumentName = publishedBlob.BlobName;
            document.PublishedBlobUri = publishedBlob.Uri;

            var publisheddocument = new PublishedDocument(document.Id, document.Title, document.DocumentName, document.PublishedDocumentName, document.PublishedBlobUri);

            await _repository.UpdateAsync(document);
            await _publishedDocumentRepository.AddAsync(publisheddocument);

            return Unit.Value;
        }
    }
}
