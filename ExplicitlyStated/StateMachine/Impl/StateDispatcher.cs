﻿using System;
using ExplicitlyStated.Configuration;
using ExplicitlyStated.StateMachine.Impl;

namespace ExplicitlyStated.Dispatch
{
    internal class StateDispatcher<TSpecificState, TMachineState, TMachineEvent>
        : IStateConfiguration<TSpecificState, TMachineState, TMachineEvent>,
          IStateDispatcher<TMachineState, TMachineEvent>
        where TSpecificState : TMachineState
    {
        private delegate TMachineState TransitionFunction(TMachineState state, TMachineEvent e);

        private SimpleDispatch<TransitionFunction> dispatch;
        private Action<TSpecificState> onEnter;
        private Action<TSpecificState> onLeave;

        public StateDispatcher(int numHandledEvents)
        {
            this.dispatch = new SimpleDispatch<TransitionFunction>(numHandledEvents);
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnEnter(Action<TSpecificState> onEnter)
        {
            this.onEnter = onEnter;
            return this;
        }

        public virtual EnteredStateType OnEnter(TMachineState state)
        {
            this.onEnter?.Invoke((TSpecificState)state);
            return EnteredStateType.Normal;
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnLeave(Action<TSpecificState> onLeave)
        {
            this.onLeave = onLeave;
            return this;
        }

        public void OnLeave(TMachineState state)
        {
            this.onLeave?.Invoke((TSpecificState)state);
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> Transition<TSpecificEvent>(Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction)
        {
            this.dispatch.AddEntry(new SimpleDispatchEntry<TransitionFunction>(
                typeof(TSpecificEvent),
                (TMachineState s, TMachineEvent e) =>
                {
                    if (e is TSpecificEvent se)
                    {
                        var specificState = (TSpecificState)s;
                        return transitionFunction(specificState, se);
                    }

                    return default;
                }));

            return this;
        }

        public bool TryTransition(TMachineState state, TMachineEvent ev, out TMachineState newState)
        {
            var transitionFunction = this.dispatch.FindEntryOrDefault(ev.GetType());
            if (transitionFunction != null)
            {
                newState = transitionFunction(state, ev);
                return true;
            }
            newState = state;
            return false;
        }
    }
}
