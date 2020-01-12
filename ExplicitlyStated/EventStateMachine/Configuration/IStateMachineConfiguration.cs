namespace ExplicitlyStated.EventStateMachine.Configuration
{
    public interface IStateMachineConfiguration<TMachineState, TMachineEvent, TGeneratedEvent>
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> ConfigureAsyncState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState;

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> ConfigureState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState;
    }
}
