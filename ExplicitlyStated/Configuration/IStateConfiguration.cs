using System;

namespace ExplicitlyStated.Configuration
{
    public interface IStateConfiguration<TSpecificState, TMachineState, TMachineEvent>
        where TSpecificState : TMachineState
    {
        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnEnter(Action<TSpecificState> onEnter);

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnLeave(Action<TSpecificState> onLeave);

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> Transition<TSpecificEvent>(
            Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction);
    }
}
