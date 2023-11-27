using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class CreateDocumentCommand : IRequest<Unit>
    {
        public CreateDocumentCommand(Guid id, string title, string documentName, Stream content)
        {
            Id = id;
            Title = title;
            DocumentName = documentName;
            Content = content;
        }

        public Guid Id { get; }
        public string Title { get; }
        public string DocumentName { get; set; }
        public Stream Content { get; set; }
    }

    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Unit>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IBlobRepository _blobRepository;

        public CreateDocumentCommandHandler(IAsyncRepository<Document> repository, IBlobRepository blobRepository)
        {
            _repository = repository;
            _blobRepository = blobRepository;
        }

        public async Task<Unit> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
        {
            UploadBlobResult? uploadresult = null;
            if (request.Content != null)
                uploadresult = await UploadBlob(request);
            
            await AddToDatabase(request, uploadresult?.Uri, uploadresult?.BlobName);

            return Unit.Value;
        }

        private async Task<Document> AddToDatabase(CreateDocumentCommand request, string? blobUri, string? blobName)
        {
            var document = new Document(request.Id, request.Title, request.DocumentName, blobUri, blobName);

            await _repository.AddAsync(document);
            return document;
        }

        private async Task<UploadBlobResult> UploadBlob(CreateDocumentCommand request)
        {
            var uniqueblobName = $"{request.Id}/{request.DocumentName}";

            var result = await _blobRepository.UploadBlob(uniqueblobName, request.Content);

            return result;
        }
    }
}
