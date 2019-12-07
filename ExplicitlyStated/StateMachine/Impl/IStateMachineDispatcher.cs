namespace ExplicitlyStated.StateMachine.Impl
{
    internal interface IStateMachineDispatcher<TMachineState, TMachineEvent>
    {
        IStateDispatcher<TMachineState, TMachineEvent> FindStateDispatcher(TMachineState state);
    }
}
