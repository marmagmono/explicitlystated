using System;

namespace ExplicitlyStated.StateMachine
{
    public class StateChangedEventArgs<TMachineState> : EventArgs
    {
        public TMachineState CurrentState { get; }

        public TMachineState PreviousState { get; }

        public StateChangedEventArgs(TMachineState currentState, TMachineState previousState)
        {
            CurrentState = currentState;
            PreviousState = previousState;
        }
    }
}
