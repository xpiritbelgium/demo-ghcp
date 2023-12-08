using CleanArchitecture.Application.Documents;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.Documents;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;

namespace CleanArchitecture.Application.Tests.Documents
{
    public class AcceptCrossCheckCommandHandlerTests
    {
        private readonly Mock<IAsyncRepository<Document>> _repository;
        private readonly Mock<IApplicationUserAccessor> _userAccessor;
        private readonly Mock<IValidator<AcceptCrossCheckCommand>> _validator;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<IUriHelper> _uriHelper;
        public AcceptCrossCheckCommandHandlerTests()
        {
            _repository = new Mock<IAsyncRepository<Document>>();
            _userAccessor = new Mock<IApplicationUserAccessor>();
            _validator = new Mock<IValidator<AcceptCrossCheckCommand>>();
            _emailService = new Mock<IEmailService>();
            _uriHelper = new Mock<IUriHelper>();
        }

        [Fact]
        public async Task Handler_WhenCalled_ShouldThrowExceptionWhenValidationFails()
        {
            // Arrange
            var command = new AcceptCrossCheckCommand(Guid.NewGuid());
            var handler = new AcceptCrossCheckCommandHandler(_repository.Object, _userAccessor.Object, _validator.Object, _emailService.Object, _uriHelper.Object);

            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("test", "test"));
            _validator.Setup(x => x.ValidateAsync(It.Is<AcceptCrossCheckCommand>(c => c.Id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult)
                .Verifiable(Times.Once());

            // Act
            // Assert
            var exception = await Assert.ThrowsAsync<Exceptions.ValidationException>(() => handler.Handle(command, CancellationToken.None));

            _validator.Verify();
        }

        [Fact]
        public async Task Handler_WhenCalled_ShouldUpdateDocumentStatusToCrossCheckedAndSetCrossCheckerUserName()
        {
            // Arrange
            var command = new AcceptCrossCheckCommand(Guid.NewGuid());
            var handler = new AcceptCrossCheckCommandHandler(_repository.Object, _userAccessor.Object, _validator.Object, _emailService.Object, _uriHelper.Object);

            var validationResult = new FluentValidation.Results.ValidationResult();
            _validator.Setup(x => x.ValidateAsync(It.Is<AcceptCrossCheckCommand>(c => c.Id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var document = new Document(command.Id, "title", "documentName", null, null);
            document.Status = DocumentStatus.Submitted;

            _repository.Setup(x => x.GetByIdAsync(It.Is<Guid>(i => i == command.Id)))
                .ReturnsAsync(document);

            var user = new User { EmailAddress = "foo@bar.com", UserName = "foo", IsAdmin = false };
            _userAccessor.Setup(x => x.GetUserAsync())
                .ReturnsAsync(user);

            _repository.Setup(x => x.UpdateAsync(It.Is<Document>(d => d.Id == command.Id && d.Status == DocumentStatus.CrossChecked && d.CrossChecker == user.UserName)))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(DocumentStatus.CrossChecked, document.Status);
            Assert.Equal(user.UserName, document.CrossChecker);
            _repository.Verify();

            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldSendEmailToRecipientContainingUrl()
        {
            // Arrange
            var command = new AcceptCrossCheckCommand(Guid.NewGuid());
            var handler = new AcceptCrossCheckCommandHandler(_repository.Object, _userAccessor.Object, _validator.Object, _emailService.Object, _uriHelper.Object);

            var validationResult = new FluentValidation.Results.ValidationResult();
            _validator.Setup(x => x.ValidateAsync(It.Is<AcceptCrossCheckCommand>(c => c.Id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            
            var document = new Document(command.Id, "title", "documentName", null, null);
            document.Status = DocumentStatus.Submitted;

            _repository.Setup(x => x.GetByIdAsync(It.Is<Guid>(i => i == command.Id)))
                .ReturnsAsync(document);

            var user = new User { EmailAddress = "foo@bar.com", UserName = "foo", IsAdmin = false };
            _userAccessor.Setup(x => x.GetUserAsync())
                .ReturnsAsync(user);

            var detailUri = "http://foo.bar";
            _uriHelper.Setup(x => x.GetDocumentDetailsUri(It.Is<Guid>(i => i == command.Id)))
                .Returns(detailUri);

            _emailService.Setup(e => e.SendEmailAsync(It.Is<EmailMessageModel>(m => m.Subject.Contains("accepted") && m.HtmlContent.Contains("accepted") && m.HtmlContent.Contains(detailUri) && m.Recipients.Contains("kluyten@xpirit.com") && m.CcRecipients.Contains(user.EmailAddress)), It.IsAny<CancellationToken>()))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            _emailService.Verify();
            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldThrowExceptionWhenDocumentIsNotFound()
        {
            // Arrange
            var command = new AcceptCrossCheckCommand(Guid.NewGuid());
            var handler = new AcceptCrossCheckCommandHandler(_repository.Object, _userAccessor.Object, _validator.Object, _emailService.Object, _uriHelper.Object);

            var validationResult = new FluentValidation.Results.ValidationResult();
            _validator.Setup(x => x.ValidateAsync(It.Is<AcceptCrossCheckCommand>(c => c.Id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(null as Document);

            // Act
            // Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
