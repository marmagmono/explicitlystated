using System;

namespace ExplicitlyStated.StateMachine.Synchronization
{
    internal interface ISynchronizationQueueFactory
    {
        ISynchronizationQueue<TMachineEvent> Create<TMachineEvent>(Action<TMachineEvent> processingDelegate);
    }
}
