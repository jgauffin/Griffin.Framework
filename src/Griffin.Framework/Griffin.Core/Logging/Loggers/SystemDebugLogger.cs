using System;
using System.Diagnostics;
using System.Text;

namespace Griffin.Logging.Loggers
{
    /// <summary>
    /// Logs to the debug window in Visual Studio
    /// </summary>
    public class SystemDebugLogger : BaseLogger
    {
        private int _frameSkipCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDebugLogger"/> class.
        /// </summary>
        /// <param name="typeThatLogs">Type of the class which uses this log. The type is used to write in the log file where the lines come from.</param>
        public SystemDebugLogger(Type typeThatLogs)
            : base(typeThatLogs)
        {
        }

        /// <summary>
        /// Specifies if we should use stack frame to identify the caller (instead of <c>loggedType</c>)
        /// </summary>
        /// <remarks>
        /// <para>Do note that there is a performance hit by using this class.</para>
        /// </remarks>
        public bool UseStackFrame { get; set; }

#if NET451
        /// <summary>
        /// Used to get the correct frame when <see cref="UseStackFrame"/> is set to true.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>Will traverse all frames in the stack the first time this method is called. Will only get the correct frame all other times.</para>
        /// </remarks>
        protected virtual StackFrame GetStackFrame()
        {
            if (_frameSkipCount != -1)
                return new StackFrame(_frameSkipCount);

            var frames = new StackTrace(2).GetFrames();
            _frameSkipCount = 2;
            foreach (var stackFrame in frames)
            {
                var reflectedType = stackFrame.GetMethod().ReflectedType;
                if (reflectedType.Namespace != null
                    && reflectedType.Namespace.StartsWith("Griffin.Logging"))
                    _frameSkipCount++;

                return stackFrame;
            }

            return new StackFrame(_frameSkipCount);
        }
#endif


        /// <summary>
        /// Write entry to the destination.
        /// </summary>
        /// <param name="entry">Entry to write</param>
        public override void Write(LogEntry entry)
        {
            string caller;
            if (UseStackFrame)
            {
#if NET451
                var frame = GetStackFrame();
                caller = frame.GetMethod().ReflectedType.Name + "." +
                             frame.GetMethod().Name + "():" + frame.GetFileLineNumber();
#else
                caller = "Unknown";
#endif
            }
            else
            {
                caller = LoggedType.Namespace ?? "";
            }

            var color = Console.ForegroundColor;
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + caller.PadRight(50) +
                              entry.LogLevel.ToString().PadRight(10) + entry.Message);

            if (entry.Exception != null)
            {
                var sb = new StringBuilder();
                BuildExceptionDetails(entry.Exception, 4, sb);
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }


            Console.ForegroundColor = color;
        }
    }
}