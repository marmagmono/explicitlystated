using System;
using System.Threading;
using System.Threading.Tasks;
using ExplicitlyStated.StateMachine.Dispatch;

namespace ExplicitlyStated.Utilities
{
    internal static class StateMachineUtilities
    {
        internal static void SetupStateState<TMachineEvent>(
            EnteredStateType enteredStateType,
            Task<TMachineEvent> actionTask,
            ref Task<TMachineEvent> asyncState,
            Action<TMachineEvent> post,
            Task<TMachineEvent> defaultTask)
        {
            if (enteredStateType == EnteredStateType.Normal)
            {
                asyncState = defaultTask;
                return;
            }

            if (enteredStateType != EnteredStateType.Async) throw new ArgumentOutOfRangeException(nameof(enteredStateType));

            asyncState = actionTask;
            asyncState.ContinueWith(
                t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        // TODO: How to crash effectively ?
                        return;
                    }

                    var messageToPost = t.Result;
                    post(t.Result);
                },
                default(CancellationToken),
                TaskContinuationOptions.RunContinuationsAsynchronously,
                TaskScheduler.Default);
        }
    }
}
