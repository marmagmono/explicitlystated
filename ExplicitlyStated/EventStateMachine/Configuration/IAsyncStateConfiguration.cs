using System;
using System.Threading.Tasks;

namespace ExplicitlyStated.EventStateMachine.Configuration
{
    public interface IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>
        where TSpecificState : TMachineState
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> TransitionWithEvent<TSpecificEvent>(
            TransitionFunction<TSpecificState, TSpecificEvent, TMachineState, TGeneratedEvent> transitionFunction)
            where TSpecificEvent : TMachineEvent;

        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> RunAsync(Func<TSpecificState, Task<TMachineEvent>> action);

        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> OnLeave(Action<TSpecificState> onLeave);

        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> Transition<TSpecificEvent>(
            Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction)
            where TSpecificEvent : TMachineEvent;
    }
}
