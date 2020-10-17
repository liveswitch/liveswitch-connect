using System;

namespace FM.LiveSwitch.Connect
{
    public class ExecuteException : Exception
    {
        public ExecuteException(string message)
            : base(message)
        { }

        public ExecuteException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
