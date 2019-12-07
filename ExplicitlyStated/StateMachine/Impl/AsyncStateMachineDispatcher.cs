using ExplicitlyStated.Configuration;

namespace ExplicitlyStated.StateMachine.Impl
{
    class AsyncStateMachineDispatcher<TMachineState, TMachineEvent>
        : StateMachineDispatcher<TMachineState, TMachineEvent>,
          IAsyncStateMachineConfiguration<TMachineState, TMachineEvent>
    {
        public IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureAsyncState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState
        {
            var dispatcher = new AsyncStateDispatcher<TSpecificState, TMachineState, TMachineEvent>(numEvents);
            AddDispatcher(typeof(TSpecificState), dispatcher);

            return dispatcher;
        }
    }
}
