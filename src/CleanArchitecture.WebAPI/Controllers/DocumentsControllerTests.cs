using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Documents;
using CleanArchitecture.WebAPI.Controllers;
using CleanArchitecture.WebAPI.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.WebAPI.Controllers.Tests
{
    public class DocumentsControllerTest
    {
        private readonly Mock<ILogger<DocumentsController>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly DocumentsController _controller;

        public DocumentsControllerTest()
        {
            _loggerMock = new Mock<ILogger<DocumentsController>>();
            _mediatorMock = new Mock<IMediator>();
            _controller = new DocumentsController(_loggerMock.Object, _mediatorMock.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnDocuments()
        {
            // Arrange
            var documents = new List<DocumentDto>
            {
                new DocumentDto { Id = Guid.NewGuid(), Title = "Document 1" },
                new DocumentDto { Id = Guid.NewGuid(), Title = "Document 2" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListPublishedDocumentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(documents);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Title.Should().Be("Document 1");
        }

        [Fact]
        public async Task Get_ById_ShouldReturnDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentDto { Id = documentId, Title = "Document 1" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublishedDocumentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            // Act
            var result = await _controller.Get(documentId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeOfType<DocumentModel>();
            var documentModel = okResult.Value as DocumentModel;
            documentModel.Title.Should().Be("Document 1");
        }

        [Fact]
        public async Task Get_ById_ShouldReturnNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublishedDocumentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DocumentDto)null);

            // Act
            var result = await _controller.Get(documentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}