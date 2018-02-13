using System;
using System.Diagnostics;
using System.Text;

namespace Griffin.Logging.Loggers
{
    /// <summary>
    /// Log everything to the console
    /// </summary>
    /// <remarks>Prints one stack frame using colored output.</remarks>
    public class ConsoleLogger : BaseLogger
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="typeThatLogs">Type being logged.</param>
        public ConsoleLogger(Type typeThatLogs)
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
        private int _frameSkipCount = -1;

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
                caller = (Environment.StackTrace ?? "").Split('\n')[0].Trim('\r');
#endif
                
            }
            else
            {
                caller = LoggedType.Namespace ?? "";
            }

            var color = Console.ForegroundColor;
            Console.ForegroundColor = GetColor(entry.LogLevel);
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + caller.PadRight(50) +
                              entry.LogLevel.ToString().PadRight(10) + entry.Message);

            if (entry.Exception != null)
            {
                var sb = new StringBuilder();
                BuildExceptionDetails(entry.Exception, 4, sb);
                Console.WriteLine(sb.ToString());
            }


            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Get a color for a specific log level
        /// </summary>
        /// <param name="logLevel">Level to get color for</param>
        /// <returns>Level color</returns>
        protected virtual ConsoleColor GetColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return ConsoleColor.DarkGray;
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.White;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.Blue;
            }
        }
    }
}