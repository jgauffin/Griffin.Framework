using System;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin
{
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        //credits: http://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task
        public static Task AsTask(this WaitHandle handle)
        {
            return AsTask(handle, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        // credits: http://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task
        public static Task AsTask(this WaitHandle handle, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<object>();
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<object>) state;
                if (timedOut)
                    localTcs.TrySetCanceled();
                else
                    localTcs.TrySetResult(null);
            }, tcs, timeout, executeOnlyOnce: true);
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle) state).Unregister(null), registration,
                TaskScheduler.Default);
            return tcs.Task;
        }
    }
}