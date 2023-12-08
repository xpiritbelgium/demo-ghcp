namespace CleanArchitecture.Domain.Documents
{
    public class PublishedDocument
    {
        private PublishedDocument()
        {
            
        }
        public PublishedDocument(Guid? id, string title, string documentName, string? blobUri, string? blobName)
        {
            Id = id;
            Title = title;
            DocumentName = documentName;
            BlobUri = blobUri;
            BlobName = blobName;
        }

        public Guid? Id { get; protected set; }
        public string Title { get; protected set; }
        public string DocumentName { get; protected set; }
        public string? BlobUri { get; protected set; }
        public string? BlobName { get; protected set; }
    }
}
