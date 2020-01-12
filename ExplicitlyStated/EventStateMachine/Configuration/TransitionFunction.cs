namespace ExplicitlyStated.EventStateMachine.Configuration
{
    public delegate (TMachineState newState, TGeneratedEvent ev)
        TransitionFunction<TSpecificState, TSpecificEvent, TMachineState, TGeneratedEvent>(
            TSpecificState currentState,
            TSpecificEvent receivedEvent);
}
