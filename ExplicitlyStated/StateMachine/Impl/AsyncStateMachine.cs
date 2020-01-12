using System;
using System.Threading.Tasks;
using ExplicitlyStated.StateMachine.Dispatch;
using ExplicitlyStated.StateMachine.Synchronization;
using ExplicitlyStated.Utilities;

namespace ExplicitlyStated.StateMachine.Impl
{
    internal class AsyncStateMachine<TMachineState, TMachineEvent> : IStateMachine<TMachineState, TMachineEvent>
    {
        private static readonly Task<TMachineEvent> CompletedActionTask = Task.FromResult(default(TMachineEvent));

        private readonly IStateMachineDispatcher<TMachineState, TMachineEvent> machineDispatcher;
        private readonly ISynchronizationQueue<TMachineEvent> synchronizationQueue;

        /// <summary>
        /// Currently running operation if current state is async state. Completed task otherwise.
        /// </summary>
        private Task<TMachineEvent> currentOperation;

        public AsyncStateMachine(
            TMachineState initialState,
            IStateMachineDispatcher<TMachineState, TMachineEvent> machineDispatcher,
            ISynchronizationQueueFactory synchronizationQueueFactory)
        {
            if (synchronizationQueueFactory is null)
            {
                throw new ArgumentNullException(nameof(synchronizationQueueFactory));
            }

            CurrentState = initialState;
            this.machineDispatcher = machineDispatcher ?? throw new ArgumentNullException(nameof(machineDispatcher));
            this.synchronizationQueue = synchronizationQueueFactory.Create<TMachineEvent>(ProcessImpl);
        }

        public TMachineState CurrentState { get; private set; }

        public event EventHandler<StateChangedEventArgs<TMachineState>> StateChanged;

        public void Process(TMachineEvent ev) => this.synchronizationQueue.AddEvent(ev);

        private void ProcessImpl(TMachineEvent ev)
        {
            var stateDispatcher = this.machineDispatcher.FindStateDispatcher(CurrentState);
            if (stateDispatcher.TryTransition(CurrentState, ev, out var updatedState))
            {
                // Cleanup previous state
                var previousState = CurrentState;

                if (previousState.GetType() != updatedState.GetType())
                {
                    stateDispatcher.OnLeave(previousState);

                    // Initialize new state
                    var newStateDispatcher = this.machineDispatcher.FindStateDispatcher(updatedState);
                    var stateType = newStateDispatcher.OnEnter(updatedState, out var actionTask);
                    StateMachineUtilities.SetupStateState(
                        stateType,
                        actionTask,
                        ref this.currentOperation,
                        Process,
                        CompletedActionTask);
                }

                // Change state
                this.CurrentState = updatedState;
                NotifyStateChanged(CurrentState, previousState);
            }
        }

        private void NotifyStateChanged(TMachineState newState, TMachineState previousState) =>
            StateChanged?.Invoke(this, new StateChangedEventArgs<TMachineState>(newState, previousState));
    }
}
