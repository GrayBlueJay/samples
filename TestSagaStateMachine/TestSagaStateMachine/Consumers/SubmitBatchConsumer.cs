namespace TestSagaStateMachine.Consumers
{
    using MassTransit;
    using Contracts;
    using Microsoft.Extensions.Logging;
    public class SubmitBatchConsumer : IConsumer<SubmitBatch>
    {
        private readonly ILogger<SubmitBatchConsumer> _logger;

        public SubmitBatchConsumer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SubmitBatchConsumer>();
        }

        public async Task Consume(ConsumeContext<SubmitBatch> context)
        {
            using (_logger.BeginScope("BatchId: {BatchId}", context.Message.BatchId))
            {
                _logger.LogDebug("Validating batch {BatchId}", context.Message.BatchId);
                _logger.LogDebug($"Destination Address: {context.DestinationAddress}" );
                //check that there are transaction ids
                if (context.Message.TransactionIds.Length == 0)
                {
                    _logger.LogWarning("Batch {BatchId} has no transactions", context.Message.BatchId);
                    await context.RespondAsync<BatchRejected>(new
                    {
                        context.Message.BatchId,
                        context.Message.Timestamp,
                        Reason = "No transactions provided"
                    });
                    return;
                }

                await context.Publish<BatchReceived>(new
                {
                    context.Message.BatchId,
                    InVar.Timestamp,
                    context.Message.TransactionIds
                });

                await context.RespondAsync<BatchSubmitted>(new
                {
                    context.Message.BatchId,
                    InVar.Timestamp
                });
                _logger.LogInformation("Published batch submission");
            }
        }
    }

    public class SubmitBatchConsumerDefinition : ConsumerDefinition<SubmitBatchConsumer>
    {
        public SubmitBatchConsumerDefinition()
        {
            ConcurrentMessageLimit = 10;
        }
    }
}
