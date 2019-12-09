using System;
using System.Threading.Tasks;

namespace ExplicitlyStated.Tests.Utils
{
    public static class TaskEventsUtils
    {
        public static Task<TEvent> FromEvent<TEvent>(
            Action<EventHandler<TEvent>> subscribe,
            Action<EventHandler<TEvent>> unsubscribe)
        {
            var tcs = new TaskCompletionSource<TEvent>();
            EventHandler<TEvent> d = null;
            d = (s, e) =>
            {
                unsubscribe(d);
                tcs.TrySetResult(e);
            };
            subscribe(d);

            return tcs.Task;
        }
    }
}
