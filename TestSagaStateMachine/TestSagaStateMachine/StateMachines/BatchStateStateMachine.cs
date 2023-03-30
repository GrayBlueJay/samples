namespace TestSagaStateMachine.StateMachines
{
    using MassTransit;
    using Contracts;
    public class BatchStateStateMachine : MassTransitStateMachine<BatchState>
    {
        public BatchStateStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => BatchReceived, x => x.CorrelateById(c => c.Message.BatchId));
            Initially(
                When(BatchReceived)
                    .Then(Initialize)
                    .TransitionTo(Received)
            );
        }

        private void Initialize(BehaviorContext<BatchState, BatchReceived> context)
        {
            InitializeInstance(context.Saga, context.Message);
        }

        private void InitializeInstance(BatchState instance, BatchReceived data)
        {
            instance.BatchId = data.BatchId;
            instance.BatchSize = data.TransactionIds.Length;
        }

        public State Received { get; private set; }
        public State Finished { get; private set; }
        public Event<BatchReceived> BatchReceived { get; private set; }

        public class BatchStateStateMachineDefinition : SagaDefinition<BatchState>
        {
            public BatchStateStateMachineDefinition()
            {
                ConcurrentMessageLimit = 1;
            }

            protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
                ISagaConfigurator<BatchState> sagaConfigurator)
            {
                sagaConfigurator.UseMessageRetry(r => r.Interval(2, 100));
                sagaConfigurator.UseInMemoryOutbox();

                var partition = endpointConfigurator.CreatePartitioner(8);

                sagaConfigurator.Message<BatchReceived>(x => x.UsePartitioner(partition, m => m.Message.BatchId));
            }
        }
    }
}