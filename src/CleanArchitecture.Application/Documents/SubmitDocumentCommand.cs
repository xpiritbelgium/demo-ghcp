using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.Documents;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class SubmitDocumentCommand: IRequest<Unit>
    {
        public SubmitDocumentCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class SubmitDocumentCommandHandler: IRequestHandler<SubmitDocumentCommand, Unit>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IApplicationUserAccessor _userAccessor;
        private readonly IEmailService _emailService;
        private readonly IUriHelper _uriHelper;

        public SubmitDocumentCommandHandler(IAsyncRepository<Document> repository, IApplicationUserAccessor userAccessor, IEmailService emailService, IUriHelper uriHelper)
        {
            _repository = repository;
            _userAccessor = userAccessor;
            _emailService = emailService;
            _uriHelper = uriHelper;
        }

        public async Task<Unit> Handle(SubmitDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException(nameof(Document), request.Id);
            var submitter = await _userAccessor.GetUserAsync();

            document.Status = DocumentStatus.Submitted;
            document.Submitter = submitter.UserName;

            await _repository.UpdateAsync(document);

            await SendEmail(document, submitter.EmailAddress);

            return Unit.Value;
        }

        private async Task SendEmail(Document document, string submitterEmail)
        {
            var subject = $"Document submitted: {document.Title}";
            var htmlContent = $"<html><body><h1>Document submitted: {document.Title}</h1><p>Please go to <a href='{_uriHelper.GetDocumentDetailsUri(document.Id)}'>this link</a> to view the details</p></body></html>";
            var recipient = "kluyten@xpirit.com"; //replace with configurable list of receivers

            //simple mail:
            //await _emailService.SendEmailAsync(subject, htmlContent, recipient);

            await _emailService.SendEmailAsync(new EmailMessageModel(subject, htmlContent, recipient).AddCcRecipient(submitterEmail));
        }
    }
}
