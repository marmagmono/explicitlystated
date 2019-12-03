using System;

namespace ExplicitlyStated.StateMachine
{
    public interface IStateMachine<TMachineState, TMachineEvent>
    {
        void Process(TMachineEvent ev);

        TMachineState CurrentState { get; }

        event EventHandler<StateChangedEventArgs<TMachineState>> StateChanged;
    }
}
