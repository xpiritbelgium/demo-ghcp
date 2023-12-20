
using CleanArchitecture.Application.Documents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.UI.Web.BackgroundTask
{
    public class PublishDocumentsBackgroundTask : IHostedService, IDisposable
    {
        private readonly ILogger<PublishDocumentsBackgroundTask> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IServiceScope _scope;
        private readonly IMediator _mediator;

        private Timer? _timer = null;

        //public PublishDocumentsBackgroundTask(ILogger<PublishDocumentsBackgroundTask> logger, IMediator mediator)
        //{
        //    _logger = logger;
        //    _mediator = mediator;
        //}

        public PublishDocumentsBackgroundTask(ILogger<PublishDocumentsBackgroundTask> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _scope = _serviceScopeFactory.CreateScope();

            _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timer Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation($"Background Task PublishDocuments started at: {DateTime.Now}");

            var documents = Task.Run(async () => await _mediator.Send(new ListDocumentsToBePublishedQuery())).Result;

            foreach (var document in documents)
            {
                try
                {
                    _logger.LogInformation($"Publishing document: {document.Title}");
                    Task.Run(async () => await _mediator.Send(new PublishDocumentCommand(document.Id)));
                }
                catch (Exception ex)
                {
                    // very naive implementation: add logic to handle retries and dead lettering
                    _logger.LogError(ex, $"Failed to publish document: {document.Title}");
                }
            }

            _logger.LogInformation($"Background Task PublishDocuments ended at: {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timer Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _scope.Dispose();
        }
    }
}
