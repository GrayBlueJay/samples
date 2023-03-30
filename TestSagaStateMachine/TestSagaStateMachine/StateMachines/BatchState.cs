namespace TestSagaStateMachine.StateMachines
{
    using MassTransit;
    
    public class BatchState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid BatchId { get; set; }
        public int BatchSize { get; set; }
    }
}