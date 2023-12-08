using CleanArchitecture.Domain.Documents;

namespace CleanArchitecture.WebAPI.Models
{
    public class DocumentModel
    {
        public DocumentModel(PublishedDocument publishedDocument)
        {
            Id = publishedDocument.Id;
            Title = publishedDocument.Title;
            DocumentName = publishedDocument.DocumentName;
            BlobUri = publishedDocument.BlobUri;
        }

        public Guid? Id { get; protected set; }
        public string Title { get; protected set; }
        public string DocumentName { get; protected set; }
        public string? BlobUri { get; protected set; }
        public string? BlobName { get; protected set; }
    }
}
