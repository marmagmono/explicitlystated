using System;
using System.Threading.Tasks;

namespace ExplicitlyStated.Configuration
{
    public interface IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent>
        where TSpecificState : TMachineState
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> RunAsync<TCompletionEvent>(Func<TSpecificState, Task<TCompletionEvent>> action)
            where TCompletionEvent : TMachineEvent;

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnLeave(Action<TSpecificState> onLeave);

        IStateConfiguration<TSpecificState, TMachineState, TMachineEvent> Transition<TSpecificEvent>(
            Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction);
    }
}
