using MassTransit;
using Contracts;

namespace TestSagaBatchProducer
{
    public class BatchSubmitter : IBatchSubmitter
    {
        private readonly ILogger<BatchSubmitter> _logger;
        private readonly IRequestClient<SubmitBatch> _submitBatchClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public BatchSubmitter(ILogger<BatchSubmitter> logger, IPublishEndpoint publishEndpoint,
            IRequestClient<SubmitBatch> submitBatchClient)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _submitBatchClient = submitBatchClient;
        }

        public async Task<Guid> SubmitBatch(CancellationToken stoppingToken)
        {
            var batchId = NewId.NextGuid();
            var transactions = new List<Guid>();

            for (var i = 0; i < 100; i++)
            {
                transactions.Add(NewId.NextGuid());
            }

            var (accepted, rejected) = await _submitBatchClient.GetResponse<BatchSubmitted, BatchRejected>(new
            {
                BatchId = batchId,
                InVar.Timestamp,
                TransactionIds = transactions.ToArray()
            }, stoppingToken);

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                _logger.LogInformation("Batch {BatchId} submitted: {Timestamp}", response.Message.BatchId,
                    response.Message.Timestamp);
                return response.Message.BatchId;
            }
            else
            {
                var response = await rejected;
                _logger.LogInformation("Batch {BatchId} rejected: {Timestamp}", response.Message.BatchId,
                    response.Message.Timestamp);
                throw new Exception(response.Message.Reason);
            }
        }
    }
}
