using Azure.Storage.Blobs;
using CleanArchitecture.Application;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Storage
{
    public class BlobRepository: IBlobRepository
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<BlobClientSettings> _options;

        public BlobRepository(BlobServiceClient blobServiceClient, IOptions<BlobClientSettings> options)
        {
            _blobServiceClient = blobServiceClient;
            _options = options;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(options.Value.SourceBlobsContainer);
            _blobContainerClient.CreateIfNotExists();            
        }

        public async Task<UploadBlobResult> UploadBlob(string blobName, Stream content)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(content, true);

            var uri = blobClient.Uri.AbsoluteUri;
            var filepath = blobClient.Name;

            return new UploadBlobResult(uri, filepath);
        }

        public async Task<Stream> DownloadBlob(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadAsync();

            return response.Value.Content;
        }

        public async Task<UploadBlobResult> PublishBlob(string blobName)
        {
            var sourceBlobClient = _blobContainerClient.GetBlobClient(blobName);
                        
            var publishedBlobContainerClient = _blobServiceClient.GetBlobContainerClient(_options.Value.PublishedBlobsContainer);
            publishedBlobContainerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            var publishedBlobClient = publishedBlobContainerClient.GetBlobClient(blobName);
            await publishedBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            var uri = publishedBlobClient.Uri.AbsoluteUri;
            var filepath = publishedBlobClient.Name;

            return new UploadBlobResult(uri, filepath);
        }
    }

    public class  BlobClientSettings
    {
        public string SourceBlobsContainer { get; set; } = String.Empty;

        public string PublishedBlobsContainer { get; set; } = String.Empty;
    }
}
