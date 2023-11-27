using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class DownloadDocumentQuery: IRequest<Stream>
    {
        public DownloadDocumentQuery(string fileName)
        {
            BlobName = fileName;
        }

        public string BlobName { get; }
    }

    public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, Stream>
    {
        private readonly IBlobRepository _blobRepository;

        public DownloadDocumentQueryHandler(IBlobRepository blobRepository)
        {
            _blobRepository = blobRepository;
        }

        public Task<Stream> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
        {
            var blob = _blobRepository.DownloadBlob(request.BlobName);

            return blob;
        }
    }
}
