using System;
using System.Threading;
using System.Threading.Tasks;
using ExplicitlyStated.StateMachine.Dispatch;
using ExplicitlyStated.StateMachine.Synchronization;

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

        public void Process(TMachineEvent ev)
        {
            this.synchronizationQueue.AddEvent(ev);
        }

        private void ProcessImpl(TMachineEvent ev)
        {
            var stateDispatcher = this.machineDispatcher.FindStateDispatcher(CurrentState);
            if (stateDispatcher.TryTransition(CurrentState, ev, out var updatedState))
            {
                // Cleanup previous state
                var previousState = CurrentState;
                stateDispatcher.OnLeave(previousState);

                // Initialize new state
                var newStateDispatcher = this.machineDispatcher.FindStateDispatcher(updatedState);
                var stateType = newStateDispatcher.OnEnter(updatedState, out var actionTask);
                SetupStateState(stateType, actionTask, ref this.currentOperation, Process);

                // Change state
                this.CurrentState = updatedState;
                NotifyStateChanged(CurrentState, previousState);
            }
        }

        private static void SetupStateState(
            EnteredStateType enteredStateType,
            Task<TMachineEvent> actionTask,
            ref Task<TMachineEvent> asyncState,
            Action<TMachineEvent> post)
        {
            if (enteredStateType == EnteredStateType.Normal)
            {
                asyncState = CompletedActionTask;
                return;
            }

            if (enteredStateType != EnteredStateType.Async) throw new ArgumentOutOfRangeException(nameof(enteredStateType));

            asyncState = actionTask;
            asyncState.ContinueWith(
                t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        // TODO: How to crash effectively ?
                        return;
                    }

                    var messageToPost = t.Result;
                    post(t.Result);
                },
                default(CancellationToken),
                TaskContinuationOptions.RunContinuationsAsynchronously,
                TaskScheduler.Default);
        }

        private void NotifyStateChanged(TMachineState newState, TMachineState previousState) =>
            StateChanged?.Invoke(this, new StateChangedEventArgs<TMachineState>(newState, previousState));
    }
}
