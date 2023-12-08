using CleanArchitecture.Domain.Documents;
using Xunit;
using CleanArchitecture.Application.Documents;
using Moq;
using MediatR;

namespace CleanArchitecture.Application.Tests.Documents
{
    public class CreateDocumentCommandHandlerTests
    {
        private readonly Mock<IAsyncRepository<Document>> _repository;
        private readonly Mock<IBlobRepository> _blobRepository;

        public CreateDocumentCommandHandlerTests()
        {
            _repository = new Mock<IAsyncRepository<Document>>();
            _blobRepository = new Mock<IBlobRepository>();
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldAddDocumentToDatabase()
        {
            // Arrange
            var command = new CreateDocumentCommand(Guid.NewGuid(), "title", "documentName", null);
            var handler = new CreateDocumentCommandHandler(_repository.Object, _blobRepository.Object);

            _repository.Setup(x => x.AddAsync(It.Is<Document>(d => d.Id == command.Id && d.Title == command.Title && d.DocumentName == command.DocumentName)))
                .ReturnsAsync(new Document(command.Id, command.Title, command.DocumentName, null, null))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Mock.Verify(_repository);
            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_WhenCalledWithoutContent_ShouldNotUploadBlob()
        {
            // Arrange
            var command = new CreateDocumentCommand(Guid.NewGuid(), "title", "documentName", null);
            var handler = new CreateDocumentCommandHandler(_repository.Object, _blobRepository.Object);

            _blobRepository.Setup(x => x.UploadBlob(It.IsAny<string>(), It.IsAny<Stream>()))
                .Verifiable(Times.Never());

            _repository.Setup(x => x.AddAsync(It.Is<Document>(d => d.Id == command.Id && d.Title == command.Title && d.DocumentName == command.DocumentName)))
                .ReturnsAsync(new Document(command.Id, command.Title, command.DocumentName, null, null))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Mock.Verify(_repository);
            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldUploadBlobAndSaveUriToDatabase()
        {
            // Arrange
            var stream = GenerateRandomStream();
            var command = new CreateDocumentCommand(Guid.NewGuid(), "title", "documentName", stream);
            var handler = new CreateDocumentCommandHandler(_repository.Object, _blobRepository.Object);

            var blobResult = new UploadBlobResult("uri", "blobName");

            _blobRepository.Setup(x => x.UploadBlob(It.IsAny<string>(), It.Is<Stream>(s => s == stream)))
                .ReturnsAsync(blobResult)
                .Verifiable(Times.Once());

            _repository.Setup(x => x.AddAsync(It.Is<Document>(d => d.BlobName == blobResult.BlobName && d.BlobUri == blobResult.Uri)))
                .ReturnsAsync(new Document(command.Id, command.Title, command.DocumentName, blobResult.Uri, blobResult.BlobName))
                .Verifiable(Times.Once());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Mock.Verify(_blobRepository, _repository);
            Assert.Equal(Unit.Value, result);
        }

        private Stream GenerateRandomStream()
        {
            var random = new Random();
            var buffer = new byte[1024];
            random.NextBytes(buffer);
            return new MemoryStream(buffer);
        }
    }
}
