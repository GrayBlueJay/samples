namespace Contracts;

public interface SubmitBatch
{
    Guid BatchId { get; }
    DateTime Timestamp { get; }
    Guid[] TransactionIds { get; }
}