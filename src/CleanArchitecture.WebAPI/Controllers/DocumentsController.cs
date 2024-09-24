using CleanArchitecture.Application.Documents;
using CleanArchitecture.WebAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CleanArchitecture.WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing documents.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentsController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="mediator">The mediator instance.</param>
        public DocumentsController(ILogger<DocumentsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        
        [HttpGet]
        public async Task<IEnumerable<DocumentModel>> Get()
        {
            var result = await _mediator.Send(new ListPublishedDocumentsQuery());

            var documents = result.Select(x => new DocumentModel(x));

            return documents;
        }

        
        /// <summary>
        /// Get a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>The document.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentModel>> Get(Guid id)
        {
            var result = await _mediator.Send(new GetPublishedDocumentQuery(id));
        
            if (result == null)
            {
                return NotFound();
            }
        
            var document = new DocumentModel(result);
        
            return Ok(document);
        }
    }
}
