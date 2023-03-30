using Contracts;
using MassTransit;
using TestSagaBatchProducer;

namespace TestSagaBatchProducer
{
    public sealed class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;


        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            (_serviceProvider, _logger) = (serviceProvider, logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"[{nameof(Worker)}] is running");
            await SubmitBatchAsync(stoppingToken);
        }
        private async Task SubmitBatchAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"[{nameof(Worker)}] is working");

            using (var scope = _serviceProvider.CreateScope())
            {
                var batchSubmitter = scope.ServiceProvider.GetRequiredService<IBatchSubmitter>();
                var batchId = await batchSubmitter.SubmitBatch(stoppingToken);
                _logger.LogInformation($"[{nameof(Worker)}] submitted batch {batchId}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{nameof(Worker)}] is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}