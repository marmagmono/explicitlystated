﻿using System;
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
