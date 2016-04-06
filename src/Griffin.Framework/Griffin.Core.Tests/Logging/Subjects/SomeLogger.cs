using System;
using System.Collections.Generic;
using Griffin.Logging;
using Griffin.Logging.Loggers;

namespace Griffin.Core.Tests.Logging.Subjects
{
    internal class SomeLogger : BaseLogger
    {
        public SomeLogger(Type type, string name)
            : base(type)
        {
            Name = name;
            Entries = new List<LogEntry>();
        }

        public List<LogEntry> Entries { get; set; }
        public string Name { get; }

        public override void Write(LogEntry entry)
        {
            Entries.Add(entry);
        }
    }
}