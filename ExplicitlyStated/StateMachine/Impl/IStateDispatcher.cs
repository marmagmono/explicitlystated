namespace ExplicitlyStated.StateMachine.Impl
{
    internal enum EnteredStateType
    {
        Normal,
        Async
    }

    internal interface IStateDispatcher<TMachineState, TMachineEvent>
    {
        bool TryTransition(TMachineState state, TMachineEvent ev, out TMachineState newState);

        EnteredStateType OnEnter(TMachineState state);

        void OnLeave(TMachineState state);
    }
}
