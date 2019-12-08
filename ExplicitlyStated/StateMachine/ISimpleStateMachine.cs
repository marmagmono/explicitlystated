namespace ExplicitlyStated.StateMachine
{
    public interface ISimpleStateMachine<TMachineState, TMachineEvent> : IStateMachine<TMachineState, TMachineEvent>
    {
        /// <summary>
        /// Tries to apply <paramref name="ev"/> to current machine state. Returns true if state has changed.
        /// </summary>
        /// <param name="ev">Event.</param>
        /// <param name="newState">New state machine state.</param>
        /// <returns>True is state has changed; false otherwise.</returns>
        bool Transition(TMachineEvent ev, out TMachineState newState);
    }
}
