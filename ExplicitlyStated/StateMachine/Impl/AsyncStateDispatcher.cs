using System;
using System.Threading.Tasks;
using ExplicitlyStated.Configuration;

namespace ExplicitlyStated.StateMachine.Impl
{
    internal class AsyncStateDispatcher<TSpecificState, TMachineState, TMachineEvent>
        : IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent>,
          IStateDispatcher<TMachineState, TMachineEvent>
        where TSpecificState : TMachineState
    {
        private delegate TMachineState TransitionFunction(TMachineState state, TMachineEvent e);
        
        private SimpleDispatch<TransitionFunction> dispatch;
        private Func<TSpecificState, Task<TMachineEvent>> stateAction;
        private Action<TSpecificState> onLeave;

        public AsyncStateDispatcher(int numHandledEvents)
        {
            this.dispatch = new SimpleDispatch<TransitionFunction>(numHandledEvents);
        }

        public IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> RunAsync(Func<TSpecificState, Task<TMachineEvent>> action)
        {
            this.stateAction = action;
            return this;
        }

        public IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> Transition<TSpecificEvent>(Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction)
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

        public IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnLeave(Action<TSpecificState> onLeave)
        {
            this.onLeave = onLeave;
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

        public EnteredStateType OnEnter(TMachineState state, out Task<TMachineEvent> asyncEvent)
        {
            asyncEvent = this.stateAction((TSpecificState)state);
            return EnteredStateType.Async;
        }

        public void OnLeave(TMachineState state)
        {
            this.onLeave?.Invoke((TSpecificState)state);
        }
    }
}
