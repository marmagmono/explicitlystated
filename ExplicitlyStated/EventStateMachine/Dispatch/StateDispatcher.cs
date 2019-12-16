using System;
using System.Threading.Tasks;
using ExplicitlyStated.EventStateMachine.Configuration;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated.EventStateMachine.Dispatch
{
    internal class StateDispatcher<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>
        : IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent>,
          IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>
        where TSpecificState : TMachineState
    {
        private SimpleDispatch<TransitionFunction<TMachineState, TMachineEvent, TMachineState, TGeneratedEvent>> dispatch;
        private Action<TSpecificState> onEnter;
        private Action<TSpecificState> onLeave;

        public StateDispatcher(int numHandledEvents)
        {
            this.dispatch = new SimpleDispatch<TransitionFunction<TMachineState, TMachineEvent, TMachineState, TGeneratedEvent>>(
                numHandledEvents);
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> OnEnter(Action<TSpecificState> onEnter)
        {
            this.onEnter = onEnter;
            return this;
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> OnLeave(Action<TSpecificState> onLeave)
        {
            this.onLeave = onLeave;
            return this;
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> Transition<TSpecificEvent>(Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction)
            where TSpecificEvent : TMachineEvent
        {
            this.dispatch.AddEntry(new SimpleDispatchEntry<TransitionFunction<TMachineState, TMachineEvent, TMachineState, TGeneratedEvent>>(
                typeof(TSpecificEvent),
                (TMachineState s, TMachineEvent e) =>
                {
                    var specificState = (TSpecificState)s;
                    var newState = transitionFunction(specificState, (TSpecificEvent)e);
                    return (newState, default);
                }));

            return this;
        }

        public IStateConfiguration<TSpecificState, TMachineState, TMachineEvent, TGeneratedEvent> TransitionWithEvent<TSpecificEvent>(TransitionFunction<TSpecificState, TSpecificEvent, TMachineState, TGeneratedEvent> transitionFunction)
            where TSpecificEvent : TMachineEvent
        {
            this.dispatch.AddEntry(new SimpleDispatchEntry<TransitionFunction<TMachineState, TMachineEvent, TMachineState, TGeneratedEvent>>(
                typeof(TSpecificEvent),
                (TMachineState s, TMachineEvent e) =>
                {
                    var specificState = (TSpecificState)s;
                    return transitionFunction(specificState, (TSpecificEvent)e);
                }));

            return this;
        }

        public EnteredStateType OnEnter(TMachineState state, out Task<TMachineEvent> asyncEvent)
        {
            this.onEnter?.Invoke((TSpecificState)state);
            asyncEvent = null;
            return EnteredStateType.Normal;
        }

        public void OnLeave(TMachineState state)
        {
            this.onLeave?.Invoke((TSpecificState)state);
        }

        public TransitionData<TMachineState, TGeneratedEvent> Transition(TMachineState state, TMachineEvent ev)
        {
            var transitionFunction = this.dispatch.FindEntryOrDefault(ev.GetType());
            if (transitionFunction != null)
            {
                var (newState, generatedEvent) = transitionFunction(state, ev);
                return TransitionData<TMachineState, TGeneratedEvent>.Transition(newState, generatedEvent);
            }

            return TransitionData<TMachineState, TGeneratedEvent>.NoTransition();
        }
    }
}
