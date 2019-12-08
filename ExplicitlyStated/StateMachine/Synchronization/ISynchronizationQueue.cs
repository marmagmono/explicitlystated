namespace ExplicitlyStated.StateMachine.Synchronization
{
    internal interface ISynchronizationQueue<TMachineEvent>
    {
        void AddEvent(TMachineEvent ev);
    }
}
