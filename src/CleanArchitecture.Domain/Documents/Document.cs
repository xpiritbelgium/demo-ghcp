namespace CleanArchitecture.Domain.Documents
{
    public class Document: IApprovalAudit, IAuditableEntity
    {
        private Document()
        {

        }

        public Document(Guid? id, string title, string documentName, string? blobUri, string? blobName)
        {
            Id = id ?? Guid.NewGuid();
            Title = title;
            DocumentName = documentName;
            BlobUri = blobUri;
            BlobName = blobName;

            Status = DocumentStatus.Draft;
        }

        public Guid Id { get; set; }

        public DocumentStatus Status { get; set; }

        public string? Submitter { get; set; }

        public string? CrossChecker { get; set; }

        public string? Approver { get; set; }

        public string Title { get; set; }
        public string? DocumentName { get; set; }
        public string? BlobUri { get; set; }

        public string? PublishedDocumentName { get; set; }
        public string? PublishedBlobUri { get; set; }

        public string? BlobName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    public enum DocumentStatus
    {
        Draft,
        Submitted,
        InCrossCheck,
        CrossChecked,
        CrossCheckRejected,
        InApproval,
        Approved,
        Rejected,
        Published
    }

    public interface IApprovalAudit
    {
        string? Submitter { get; set; }

        string? CrossChecker { get; set; }

        string? Approver { get; set; }
    }
}
