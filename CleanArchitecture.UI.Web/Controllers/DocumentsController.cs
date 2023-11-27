using CleanArchitecture.Application.Documents;
using CleanArchitecture.UI.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.UI.Web.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IMediator _mediator;

        public DocumentsController(ILogger<DocumentsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var documents = await _mediator.Send(new ListDocumentsQuery());

            return View(documents);
        }

        [Authorize(Roles = "Document.Creator,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Document.Creator,Admin")]
        public async Task<IActionResult> Create(CreateDocumentViewModel model, IFormFile postedFile)
        {
            var fileStream = postedFile?.OpenReadStream();
            var fileName = postedFile?.FileName;

            var command = new CreateDocumentCommand(Guid.NewGuid(), model.Title, fileName, fileStream);

            await _mediator.Send(command);

            return RedirectToAction("Details", new { id = command.Id });
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var document = await _mediator.Send(new GetDocumentQuery(id));

            return View(document);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            var document = await _mediator.Send(new GetDocumentQuery(id));

            var stream = await _mediator.Send(new DownloadDocumentQuery(document.BlobName));

            return File(stream, "application/octet-stream", document.DocumentName);
        }

        [Authorize(Roles = "Document.Creator,Admin")]
        public async Task<IActionResult> Submit(Guid id)
        {
            await _mediator.Send(new SubmitDocumentCommand(id));

            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Document.Crosschecker,Admin")]
        public async Task<IActionResult> AcceptCrossCheck(Guid id)
        {
            await _mediator.Send(new AcceptCrossCheckCommand(id));

            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Document.Approver,Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _mediator.Send(new ApproveCommand(id));

            return RedirectToAction("Details", new { id });
        }
    }
}