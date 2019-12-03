namespace ExplicitlyStated.Configuration
{
    public interface IAsyncStateMachineConfiguration<TMachineState, TMachineEvent> : IStateMachineConfiguration<TMachineState, TMachineEvent>
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureAsyncState<TSpecificState>()
            where TSpecificState : TMachineState;
    }
}
