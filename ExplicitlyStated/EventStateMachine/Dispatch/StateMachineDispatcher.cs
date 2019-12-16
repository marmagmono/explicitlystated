using System;
using ExplicitlyStated.EventStateMachine.Configuration;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated.EventStateMachine.Dispatch
{
    internal class StateMachineDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>
        : IStateMachineConfiguration<TMachineState, TMachineEvent, TGeneratedEvent>,
          IStateMachineDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>
    {
        private SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>> dispatch =
            new SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>>(8);

        public IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> ConfigureAsyncState<TSpecificState>(int numEvents) where TSpecificState : TMachineState
        {
            var dispatcher = new AsyncStateDispatcher<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>(numEvents);
            AddDispatcher(typeof(TSpecificState), dispatcher);

            return dispatcher;
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> ConfigureState<TSpecificState>(int numEvents) where TSpecificState : TMachineState
        {
            var dispatcher = new StateDispatcher<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>(numEvents);
            AddDispatcher(typeof(TSpecificState), dispatcher);

            return dispatcher;
        }

        private void AddDispatcher(
            Type stateType,
            IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent> dispatcher)
        {
            this.dispatch.AddEntry(new SimpleDispatchEntry<IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>>(
                stateType,
                dispatcher));
        }

        public IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent> FindStateDispatcher(TMachineState state) =>
            this.dispatch.FindEntryOrDefault(state.GetType());
    }
}
