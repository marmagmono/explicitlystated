namespace ExplicitlyStated.Configuration
{
    public interface IStateMachineConfiguration<TMachineState, TMachineEvent>
    {
        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState;
    }
}
