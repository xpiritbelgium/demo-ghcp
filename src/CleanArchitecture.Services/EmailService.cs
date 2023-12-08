using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CleanArchitecture.Application;

namespace CleanArchitecture.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailClient _emailClient;
        private readonly EmailClientSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, EmailClient emailClient, IOptions<EmailClientSettings> emailSettings)
        {
            _logger = logger;
            _emailClient = emailClient;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string subject, string htmlContent, string recipient, CancellationToken cancellationToken = default)
        {
            try
            {
                var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, _emailSettings.Sender, recipient, subject, htmlContent, cancellationToken: cancellationToken);

                _logger.LogInformation($"Email send operation started, operation id = {emailSendOperation.Id}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                _logger.LogError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }

        public async Task SendEmailAsync(EmailMessageModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var emailMessage = new EmailMessage(_emailSettings.Sender,
                    new EmailRecipients(model.Recipients.Select(x => new EmailAddress(x)), model.CcRecipients?.Select(x => new EmailAddress(x)), model.BccRecipients?.Select(x => new EmailAddress(x))),
                    new EmailContent(model.Subject) { Html = model.HtmlContent});

                if (model.Attachments != null)
                    foreach (var attachment in model.Attachments)
                        emailMessage.Attachments.Add(new EmailAttachment(attachment.Name, attachment.ContentType, attachment.Content));

                var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);

                _logger.LogInformation($"Email send operation started, operation id = {emailSendOperation.Id}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                _logger.LogError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }
    }

    public class EmailClientSettings
    {
        public string Sender { get; set; } = String.Empty;
    }
}
