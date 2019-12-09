using System;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated.StateMachine.Impl
{
    internal class SimpleStateMachine<TMachineState, TMachineEvent> : ISimpleStateMachine<TMachineState, TMachineEvent>
    {
        private readonly IStateMachineDispatcher<TMachineState, TMachineEvent> machineDispatcher;

        public TMachineState CurrentState { get; private set; }

        public event EventHandler<StateChangedEventArgs<TMachineState>> StateChanged;

        public SimpleStateMachine(
            TMachineState initialState,
            IStateMachineDispatcher<TMachineState, TMachineEvent> machineDispatcher)
        {
            this.CurrentState = initialState;
            this.machineDispatcher = machineDispatcher;
        }

        public void Process(TMachineEvent ev)
        {
            Transition(ev, out var _);
        }

        public bool Transition(TMachineEvent ev, out TMachineState newState)
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
                    newStateDispatcher.OnEnter(updatedState, out var _);
                }

                // Change state
                this.CurrentState = updatedState;
                NotifyStateChanged(CurrentState, previousState);

                newState = CurrentState;
                return true;
            }

            newState = CurrentState;
            return false;
        }

        private void NotifyStateChanged(TMachineState newState, TMachineState previousState) =>
            StateChanged?.Invoke(this, new StateChangedEventArgs<TMachineState>(newState, previousState));
    }
}
