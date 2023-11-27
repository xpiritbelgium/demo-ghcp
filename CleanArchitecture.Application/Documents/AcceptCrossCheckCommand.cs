using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.Documents;
using FluentValidation;
using MediatR;

namespace CleanArchitecture.Application.Documents
{
    public class AcceptCrossCheckCommand : IRequest<Unit>
    {
        public AcceptCrossCheckCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class AcceptCrossCheckCommandHandler : IRequestHandler<AcceptCrossCheckCommand, Unit>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IApplicationUserAccessor _userAccessor;
        private readonly IValidator<AcceptCrossCheckCommand> _validator;
        private readonly IEmailService _emailService;
        private readonly IUriHelper _uriHelper;

        public AcceptCrossCheckCommandHandler(IAsyncRepository<Document> repository, IApplicationUserAccessor userAccessor, IValidator<AcceptCrossCheckCommand> validator, IEmailService emailService, IUriHelper uriHelper)
        {
            _repository = repository;
            _userAccessor = userAccessor;
            _validator = validator;
            _emailService = emailService;
            _uriHelper = uriHelper;
        }

        public async Task<Unit> Handle(AcceptCrossCheckCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new Exceptions.ValidationException(validationResult);

            var document = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException(nameof(Document), request.Id);
            var crosschecker = await _userAccessor.GetUserAsync();

            document.Status = DocumentStatus.CrossChecked;
            document.CrossChecker = crosschecker.UserName;

            await _repository.UpdateAsync(document);

            await SendEmail(document, crosschecker.EmailAddress);

            return Unit.Value;
        }

        private async Task SendEmail(Document document, string crosscheckerEmail)
        {
            var subject = $"Document accepted: {document.Title}";
            var htmlContent = $"<html><body><h1>Document accepted: {document.Title}</h1><p>Please go to <a href='{_uriHelper.GetDocumentDetailsUri(document.Id)}'>this link</a> to view the details</p></body></html>";
            var recipient = "kluyten@xpirit.com"; //replace with configurable list of receivers

            //simple mail:
            //await _emailService.SendEmailAsync(subject, htmlContent, recipient);

            await _emailService.SendEmailAsync(new EmailMessageModel(subject, htmlContent, recipient).AddCcRecipient(crosscheckerEmail));
        }
    }

    public class AcceptCrossCheckCommandValidator : AbstractValidator<AcceptCrossCheckCommand>, IValidator<AcceptCrossCheckCommand>
    {
        private readonly IAsyncRepository<Document> _repository;
        private readonly IApplicationUserAccessor _applicationUserAccessor;

        public AcceptCrossCheckCommandValidator(IAsyncRepository<Document> repository, IApplicationUserAccessor applicationUserAccessor)
        {
            _repository = repository;
            _applicationUserAccessor = applicationUserAccessor;

            RuleFor(c => c)
                .MustAsync(CrossCheckerShouldDifferFromContentManager)
                .WithMessage("Cross checker should differ from content manager");
        }

        private async Task<bool> CrossCheckerShouldDifferFromContentManager(AcceptCrossCheckCommand command, CancellationToken token)
        {
            var user = await _applicationUserAccessor.GetUserAsync();
            
            if (user.IsAdmin) 
                return true;

            var document = await _repository.GetByIdAsync(command.Id);

            if (document?.Submitter != user.UserName)
                return true;

            return false;
        }
    }
}
