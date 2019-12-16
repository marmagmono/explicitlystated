using System.Threading.Tasks;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated.EventStateMachine.Dispatch
{
    internal enum TransitionEnum
    {
        NoTransition, Transitioned
    }

    internal readonly ref struct TransitionData<TMachineState, TGeneratedEvent>
    {
        public readonly TransitionEnum TransitionType;

        public readonly TMachineState NewState;

        public readonly TGeneratedEvent GeneratedEvent;

        public static TransitionData<TMachineState, TGeneratedEvent> NoTransition() =>
            new TransitionData<TMachineState, TGeneratedEvent>(
                TransitionEnum.NoTransition,
                default,
                default);

        public static TransitionData<TMachineState, TGeneratedEvent> Transition(TMachineState newState, TGeneratedEvent generatedEvent) =>
            new TransitionData<TMachineState, TGeneratedEvent>(
                TransitionEnum.Transitioned,
                newState,
                generatedEvent);

        private TransitionData(TransitionEnum transition, TMachineState newState, TGeneratedEvent generatedEvent)
        {
            TransitionType = transition;
            NewState = newState;
            GeneratedEvent = generatedEvent;
        }
    }

    internal interface IStateDispatcher<TMachineState, TMachineEvent, TGeneratedEvent>
    {
        TransitionData<TMachineState, TGeneratedEvent> Transition(TMachineState state, TMachineEvent ev);

        EnteredStateType OnEnter(TMachineState state, out Task<TMachineEvent> asyncEvent);

        void OnLeave(TMachineState state);
    }
}
