using System;

namespace ExplicitlyStated.StateMachine.Synchronization
{
    internal class DataFlowSynchronizationQueueFactory : ISynchronizationQueueFactory
    {
        public ISynchronizationQueue<TMachineEvent> Create<TMachineEvent>(Action<TMachineEvent> processingDelegate) =>
            new DataFlowSynchronizationQueue<TMachineEvent>(processingDelegate);
    }
}
