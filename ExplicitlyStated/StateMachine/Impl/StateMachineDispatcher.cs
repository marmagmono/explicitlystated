using ExplicitlyStated.Configuration;
using ExplicitlyStated.Dispatch;

namespace ExplicitlyStated.StateMachine.Impl
{
    internal class StateMachineDispatcher<TMachineState, TMachineEvent>
        : IStateMachineConfiguration<TMachineState, TMachineEvent>,
          IStateMachineDispatcher<TMachineState, TMachineEvent>
    {
        private readonly SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent>> dispatch =
            new SimpleDispatch<IStateDispatcher<TMachineState, TMachineEvent>>(8);

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> ConfigureState<TSpecificState>(int numEvents)
            where TSpecificState : TMachineState
        {
            var dispatcher = new StateDispatcher<TSpecificState, TMachineState, TMachineEvent>(numEvents);
            this.dispatch.AddEntry(new SimpleDispatchEntry<IStateDispatcher<TMachineState, TMachineEvent>>(
                typeof(TSpecificState),
                dispatcher));

            return dispatcher;
        }

        public IStateDispatcher<TMachineState, TMachineEvent> ResolveStateDispatcher(TMachineState state) =>
            this.dispatch.FindEntryOrDefault(state.GetType());

    }
}
