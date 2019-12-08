namespace ExplicitlyStated.Configuration
{
    public interface IAsyncStateMachineConfiguration<TMachineState, TMachineEvent>
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureAsyncState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState;

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState;
    }
}
