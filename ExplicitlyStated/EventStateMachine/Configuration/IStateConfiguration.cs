using System;

namespace ExplicitlyStated.EventStateMachine.Configuration
{
    public interface IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>
        where TSpecificState : TMachineState
    {
        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> TransitionWithEvent<TSpecificEvent>(
            TransitionFunction<TSpecificState, TSpecificEvent, TMachineState, TGeneratedEvent> transitionFunction)
            where TSpecificEvent : TMachineEvent;

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> OnEnter(Action<TSpecificState> onEnter);

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> OnLeave(Action<TSpecificState> onLeave);

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> Transition<TSpecificEvent>(
            Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction)
            where TSpecificEvent : TMachineEvent;
    }
}
