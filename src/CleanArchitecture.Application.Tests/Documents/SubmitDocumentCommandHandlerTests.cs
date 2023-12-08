using CleanArchitecture.Application.Documents;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.Documents;
using MediatR;
using Moq;
using Xunit;

namespace CleanArchitecture.Application.Tests.Documents
{
    public class SubmitDocumentCommandHandlerTests
    {
        private readonly Mock<IAsyncRepository<Document>> _repository;
        private readonly Mock<IApplicationUserAccessor> _userAccessor;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<IUriHelper> _uriHelper;

        public SubmitDocumentCommandHandlerTests()
        {
            _repository = new Mock<IAsyncRepository<Document>>();
            _userAccessor = new Mock<IApplicationUserAccessor>();
            _emailService = new Mock<IEmailService>();
            _uriHelper = new Mock<IUriHelper>();
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldUpdateDocumentStatusToSubmittedAndSetSubmitterUserName()
        {
            // Arrange
            var command = new SubmitDocumentCommand(Guid.NewGuid());
            var handler = new SubmitDocumentCommandHandler(_repository.Object, _userAccessor.Object, _emailService.Object, _uriHelper.Object);

            var document = new Document(command.Id, "title", "documentName", null, null);
            Assert.Equal(DocumentStatus.Draft, document.Status);

            _repository.Setup(x => x.GetByIdAsync(It.Is<Guid>(i => i == command.Id)))
                .ReturnsAsync(document);

            var user = new User { EmailAddress = "foo@bar.com", UserName = "foo", IsAdmin = false };
            _userAccessor.Setup(x => x.GetUserAsync())
                .ReturnsAsync(user);

            _repository.Setup(x => x.UpdateAsync(It.Is<Document>(d => d.Id == command.Id && d.Status == DocumentStatus.Submitted && d.Submitter == user.UserName)))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(DocumentStatus.Submitted, document.Status);
            Assert.Equal(user.UserName, document.Submitter);
            _repository.Verify();

            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldSendEmailToRecipientContainingUrl()
        {
            // Arrange
            var command = new SubmitDocumentCommand(Guid.NewGuid());
            var handler = new SubmitDocumentCommandHandler(_repository.Object, _userAccessor.Object, _emailService.Object, _uriHelper.Object);

            var document = new Document(command.Id, "title", "documentName", null, null);
            Assert.Equal(DocumentStatus.Draft, document.Status);

            _repository.Setup(x => x.GetByIdAsync(It.Is<Guid>(i => i == command.Id)))
                .ReturnsAsync(document);

            var user = new User { EmailAddress = "foo@bar.com", UserName = "foo", IsAdmin = false };
            _userAccessor.Setup(x => x.GetUserAsync())
                .ReturnsAsync(user);

            var detailUri = "http://foo.bar";
            _uriHelper.Setup(x => x.GetDocumentDetailsUri(It.Is<Guid>(i => i == command.Id)))
                .Returns(detailUri);

            _emailService.Setup(e => e.SendEmailAsync(It.Is<EmailMessageModel>(m => m.Subject.Contains("submitted") && m.HtmlContent.Contains("submitted") && m.HtmlContent.Contains(detailUri) && m.Recipients.Contains("kluyten@xpirit.com") && m.CcRecipients.Contains(user.EmailAddress)), It.IsAny<CancellationToken>() ))
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
            var command = new SubmitDocumentCommand(Guid.NewGuid());
            var handler = new SubmitDocumentCommandHandler(_repository.Object, _userAccessor.Object, _emailService.Object, _uriHelper.Object);

            _repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(null as Document);

            // Act
            // Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
