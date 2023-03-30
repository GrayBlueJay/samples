namespace Contracts;

using System;


public interface BatchSubmitted
{
    Guid BatchId { get; }

    DateTime Timestamp { get; }
}