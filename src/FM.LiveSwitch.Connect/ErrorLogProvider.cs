using System;

namespace FM.LiveSwitch.Connect
{
    class ErrorLogProvider : LogProvider
    {
        public ErrorLogProvider()
            : this(LogLevel.Error)
        { }

        public ErrorLogProvider(LogLevel level)
        {
            Level = level;
        }

        protected override void DoLog(LogEvent logEvent)
        {
            Console.Error.WriteLine(GenerateLogLine(logEvent));
        }
    }
}
