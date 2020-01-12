using System;
using System.Threading.Tasks;
using ExplicitlyStated.EventStateMachine.Dispatch;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.StateMachine.Synchronization;
using ExplicitlyStated.Utilities;

namespace ExplicitlyStated.EventStateMachine.Impl
{
    internal class EventStateMachine<TMachineState, TMachineEvent, TGeneratedEvent>
        : IEventStateMachine<TMachineState, TMachineEvent, TGeneratedEvent>
    {
        private static readonly Task<TMachineEvent> CompletedActionTask = Task.FromResult(default(TMachineEvent));

        private readonly IStateMachineDispatcher<TMachineState, TMachineEvent, TGeneratedEvent> machineDispatcher;
        private readonly ISynchronizationQueue<TMachineEvent> synchronizationQueue;

        private Task<TMachineEvent> currentOperation;

        public EventStateMachine(
            TMachineState initialState,
            IStateMachineDispatcher<TMachineState, TMachineEvent, TGeneratedEvent> machineDispatcher,
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
        public event EventHandler<EventGeneratedEventArgs<TGeneratedEvent>> EventGenerated;

        public void Process(TMachineEvent ev) => this.synchronizationQueue.AddEvent(ev);

        private void ProcessImpl(TMachineEvent ev)
        {
            var stateDispatcher = this.machineDispatcher.FindStateDispatcher(CurrentState);
            var transition = stateDispatcher.Transition(CurrentState, ev);

            if (transition.TransitionType == TransitionEnum.Transitioned)
            {
                // Cleanup previous state
                var previousState = CurrentState;
                var updatedState = transition.NewState;

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

                if (transition.GeneratedEvent != default)
                {
                    NotifyEventGenerated(transition.GeneratedEvent);
                }

                NotifyStateChanged(CurrentState, previousState);
            }
        }

        private void NotifyStateChanged(TMachineState newState, TMachineState previousState) =>
            StateChanged?.Invoke(this, new StateChangedEventArgs<TMachineState>(newState, previousState));

        private void NotifyEventGenerated(TGeneratedEvent e) =>
            EventGenerated?.Invoke(this, new EventGeneratedEventArgs<TGeneratedEvent>(e));
    }
}
