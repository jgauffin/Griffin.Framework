using System;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin
{
    /// <summary>
    /// Extensions to make it easier to work with thread synchronization objects.
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Convert a wait handle to a TPL Task.
        /// </summary>
        /// <param name="handle">Handle to convert</param>
        /// <returns>Generated task.</returns>
        /// 
        //credits: http://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task
        public static Task AsTask(this WaitHandle handle)
        {
            return AsTask(handle, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Convert a wait handle to a task
        /// </summary>
        /// <param name="handle">Wait handle</param>
        /// <param name="timeout">Max time to wait</param>
        /// <returns>Created task.</returns>
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