using CleanArchitecture.Application.Documents;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.WebJob
{
    public class Functions
    {
        private readonly IMediator _mediator;

        public Functions(IMediator mediator)
        {
            _mediator = mediator;
        }

        // WebJob function that runs on a schedule
        public async Task PublishDocuments([TimerTrigger("0 * * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation($"WebJob function PublishDocuments started at: {DateTime.Now}");

            var documents = await _mediator.Send(new ListDocumentsToBePublishedQuery());

            foreach (var document in documents)
            {
                try
                {
                    logger.LogInformation($"Publishing document: {document.Title}");
                    await _mediator.Send(new PublishDocumentCommand(document.Id));
                }
                catch (Exception ex)
                {
                    // very naive implementation: add logic to handle retries and dead lettering
                    logger.LogError(ex, $"Failed to publish document: {document.Title}");
                }
            }

            logger.LogInformation($"WebJob function PublishDocuments ended at: {DateTime.Now}");
        }
    }
}
