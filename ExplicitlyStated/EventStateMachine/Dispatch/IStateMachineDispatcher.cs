namespace ExplicitlyStated.EventStateMachine.Dispatch
{
    internal interface IStateMachineDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>
    {
        IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent> FindStateDispatcher(TMachineState state);
    }
}
