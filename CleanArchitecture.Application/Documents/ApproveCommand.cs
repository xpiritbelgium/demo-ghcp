using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.Documents;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Documents
{
    public class ApproveCommand: IRequest<Unit>
    {
        public ApproveCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ApproveCommandHandler : IRequestHandler<ApproveCommand, Unit>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IValidator<ApproveCommand> _validator;
        private readonly IApplicationUserAccessor _userAccessor;
        private readonly IEmailService _emailService;
        private readonly IUriHelper _uriHelper;

        public ApproveCommandHandler(IAsyncRepository<Document> repository, IValidator<ApproveCommand> validator, IApplicationUserAccessor userAccessor, IEmailService emailService, IUriHelper uriHelper)
        {
            _repository = repository;
            _validator = validator;
            _userAccessor = userAccessor;
            _emailService = emailService;
            _uriHelper = uriHelper;
        }

        public async Task<Unit> Handle(ApproveCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new Exceptions.ValidationException(validationResult);

            var document = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException(nameof(Document), request.Id);
            var approver = await _userAccessor.GetUserAsync();

            document.Status = DocumentStatus.Approved;
            document.Approver = approver.UserName;

            await _repository.UpdateAsync(document);

            await SendEmail(document, approver.EmailAddress);

            return Unit.Value;
        }

        private async Task SendEmail(Document document, string submitterEmail)
        {
            var subject = $"Document approved: {document.Title}";
            var htmlContent = $"<html><body><h1>Document approved: {document.Title}</h1><p>Please go to <a href='{_uriHelper.GetDocumentDetailsUri(document.Id)}'>this link</a> to view the details</p></body></html>";
            var recipient = "kluyten@xpirit.com"; //replace with configurable list of receivers

            //simple mail:
            //await _emailService.SendEmailAsync(subject, htmlContent, recipient);

            await _emailService.SendEmailAsync(new EmailMessageModel(subject, htmlContent, recipient).AddCcRecipient(submitterEmail));
        }
    }

    public class ApproveCommandValidator : AbstractValidator<ApproveCommand>, IValidator<ApproveCommand>
    {
        // add validation logic
    }
}
