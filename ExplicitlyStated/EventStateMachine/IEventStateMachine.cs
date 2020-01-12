using System;
using ExplicitlyStated.StateMachine;

namespace ExplicitlyStated.EventStateMachine
{
    public interface IEventStateMachine<TMachineState, TMachineEvent, TGeneratedEvent>
        : IStateMachine<TMachineState, TMachineEvent>
    {
        event EventHandler<EventGeneratedEventArgs<TGeneratedEvent>> EventGenerated;
    }
}
