using CleanArchitecture.Application.Documents;
using CleanArchitecture.WebAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CleanArchitecture.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IMediator _mediator;

        public DocumentsController(ILogger<DocumentsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/<DocumentsController>
        [HttpGet]
        public async Task<IEnumerable<DocumentModel>> Get()
        {
            var result = await _mediator.Send(new ListPublishedDocumentsQuery());

            var documents = result.Select(x => new DocumentModel(x));

            return documents;
        }

        // GET api/<DocumentsController>/5
        [HttpGet("{id}")]
        public async Task<DocumentModel> Get(Guid id)
        {
            var result = await _mediator.Send(new GetPublishedDocumentQuery(id));

            var document = new DocumentModel(result);

            return document;
        }
    }
}
