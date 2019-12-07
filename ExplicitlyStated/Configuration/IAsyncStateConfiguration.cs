using System;
using System.Threading.Tasks;

namespace ExplicitlyStated.Configuration
{
    public interface IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent>
        where TSpecificState : TMachineState
    {
        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> RunAsync(Func<TSpecificState, Task<TMachineEvent>> action);

        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> OnLeave(Action<TSpecificState> onLeave);

        IAsyncStateConfiguration<TSpecificState, TMachineState, TMachineEvent> Transition<TSpecificEvent>(
            Func<TSpecificState, TSpecificEvent, TMachineState> transitionFunction);
    }
}
