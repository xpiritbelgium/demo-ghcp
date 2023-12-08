
namespace CleanArchitecture.Application
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string htmlContent, string recipient, CancellationToken cancellationToken = default);
        Task SendEmailAsync(EmailMessageModel model, CancellationToken cancellationToken = default);
    }

    public class EmailMessageModel
    {
        public EmailMessageModel(string subject, string htmlContent, params string[] recipients)
        {
            Subject = subject;
            HtmlContent = htmlContent;
            Recipients = recipients.ToList();
        }

        public string Subject { get; }
        public string HtmlContent { get; }
        public List<string> Recipients { get; }

        public List<string>? CcRecipients { get; protected set; }

        public List<string>? BccRecipients { get; protected set; }

        public List<EmailAttachmentModel>? Attachments { get; protected set; }

        public EmailMessageModel AddCcRecipient(string cc)
        {
            CcRecipients ??= new List<string>();

            CcRecipients.Add(cc);

            return this;
        }

        public EmailMessageModel AddBccRecipient(string bcc)
        {
            BccRecipients ??= new List<string>();

            BccRecipients.Add(bcc);

            return this;
        }

        public EmailMessageModel AddAttachement(string name, string contentType, BinaryData content)
        {
            Attachments ??= new List<EmailAttachmentModel>();

            Attachments.Add(new EmailAttachmentModel(name, contentType, content));

            return this;
        }

        public class EmailAttachmentModel
        {
            public EmailAttachmentModel(string name, string contentType, BinaryData content)
            {
                Name = name;
                ContentType = contentType;
                Content = content;
            }

            public string Name { get; }
            public string ContentType { get; }
            public BinaryData Content { get; }
        }
    }
}
