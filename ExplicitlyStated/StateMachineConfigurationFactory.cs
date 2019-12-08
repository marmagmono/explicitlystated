using ExplicitlyStated.Configuration;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated
{
    public static class StateMachineConfigurationFactory
    {
        public static IAsyncStateMachineConfiguration<TMachineState, TMachineEvent> CreateAsync<TMachineState, TMachineEvent>() =>
            new AsyncStateMachineDispatcher<TMachineState, TMachineEvent>();

        public static IStateMachineConfiguration<TMachineState, TMachineEvent> Create<TMachineState, TMachineEvent>() =>
            new StateMachineDispatcher<TMachineState, TMachineEvent>();
    }
}
