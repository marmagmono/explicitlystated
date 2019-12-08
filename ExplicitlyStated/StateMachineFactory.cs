using System;
using ExplicitlyStated.Configuration;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.StateMachine.Dispatch;
using ExplicitlyStated.StateMachine.Impl;
using ExplicitlyStated.StateMachine.Synchronization;

namespace ExplicitlyStated
{
    public static class StateMachineFactory
    {
        public static IStateMachine<TMachineState, TMachineEvent> CreateAsync<TMachineState, TMachineEvent>(
            TMachineState initialState,
            IAsyncStateMachineConfiguration<TMachineState, TMachineEvent> configuration)
        {
            if (configuration is AsyncStateMachineDispatcher<TMachineState, TMachineEvent> dispatcher)
            {
                var synchronizationQueueFactory = new DataFlowSynchronizationQueueFactory();
                var machine = new AsyncStateMachine<TMachineState, TMachineEvent>(initialState, dispatcher, synchronizationQueueFactory);
                return machine;
            }

            throw new ArgumentException("configuration must not be a custom implementation of the state machine configuration interface.");
        }

        public static ISimpleStateMachine<TMachineState, TMachineEvent> CreateSimple<TMachineState, TMachineEvent>(
            TMachineState initialState,
            IStateMachineConfiguration<TMachineState, TMachineEvent> configuration)
        {
            if (configuration is StateMachineDispatcher<TMachineState, TMachineEvent> dispatcher)
            {
                var machine = new SimpleStateMachine<TMachineState, TMachineEvent>(initialState, dispatcher);
                return machine;
            }

            throw new ArgumentException("configuration must not be a custom implementation of the state machine configuration interface.");
        }
    }
}
