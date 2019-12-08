using System;
using System.Threading.Tasks.Dataflow;

namespace ExplicitlyStated.StateMachine.Synchronization
{
    internal class DataFlowSynchronizationQueue<TMachineEvent> : ISynchronizationQueue<TMachineEvent>
    {
        private readonly ActionBlock<TMachineEvent> actionBlock;

        public DataFlowSynchronizationQueue(Action<TMachineEvent> processingDelegate)
        {
            this.actionBlock = new ActionBlock<TMachineEvent>(
                processingDelegate,
                new ExecutionDataflowBlockOptions
                {
                    EnsureOrdered = true,
                    MaxDegreeOfParallelism = 1
                });
        }

        public void AddEvent(TMachineEvent ev)
        {
            this.actionBlock.Post(ev);
        }
    }
}
