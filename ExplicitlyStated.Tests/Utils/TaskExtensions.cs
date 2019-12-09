using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExplicitlyStated.Tests.Utils
{
    public static class TaskExtensions
    {
        public static Task<TResult> Timeout<TResult>(this Task<TResult> t, TimeSpan timeout)
        {
            if (t.IsCompleted)
            {
                return t;
            }

            var tcs = new TaskCompletionSource<TResult>();

            var cts = new CancellationTokenSource();
            CancellationTokenRegistration registration = default;
            cts.Token.Register(
                () =>
                {
                    tcs.TrySetCanceled();
                    registration.Dispose();
                });

            t.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted)
                {
                    tcs.TrySetException(t.Exception);
                }
                else if (t.Status == TaskStatus.Canceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }

                registration.Dispose();
            });


            cts.CancelAfter(timeout);
            return tcs.Task;
        }
    }
}
