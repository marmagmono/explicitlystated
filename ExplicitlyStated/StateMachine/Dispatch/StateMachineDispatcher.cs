using System;
using ExplicitlyStated.Configuration;

namespace ExplicitlyStated.StateMachine.Dispatch
{
    internal class StateMachineDispatcher<TMachineState, TMachineEvent>
        : IStateMachineConfiguration<TMachineState, TMachineEvent>,
          IStateMachineDispatcher<TMachineState, TMachineEvent>
    {
        private SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent>> dispatch =
            new SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent>>(8);

        protected virtual void AddDispatcher(
            Type stateType,
            IStateDispatcher<TMachineState, TMachineEvent> dispatcher)
        {
            this.dispatch.AddEntry(new SimpleDispatchEntry<IStateDispatcher<TMachineState, TMachineEvent>>(
                stateType,
                dispatcher));
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState
        {
            var dispatcher = new StateDispatcher<TSpecificState, TMachineState, TMachineEvent>(numEvents);
            AddDispatcher(typeof(TSpecificState), dispatcher);

            return dispatcher;
        }

        public IStateDispatcher<TMachineState, TMachineEvent> FindStateDispatcher(TMachineState state) =>
            this.dispatch.FindEntryOrDefault(state.GetType());
    }
}
