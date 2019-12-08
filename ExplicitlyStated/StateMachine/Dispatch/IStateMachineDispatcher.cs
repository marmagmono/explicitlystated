namespace ExplicitlyStated.StateMachine.Dispatch
{
    internal interface IStateMachineDispatcher<TMachineState, TMachineEvent>
    {
        IStateDispatcher<TMachineState, TMachineEvent> FindStateDispatcher(TMachineState state);
    }
}
