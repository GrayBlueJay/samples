namespace Contracts;

public interface BatchReceived
{
    Guid BatchId { get; }
    DateTime Timestamp { get; }
    Guid[] TransactionIds { get; }
}