using System;

namespace FM.LiveSwitch.Connect
{
    public class ParameterSetException : Exception
    {
        public ParameterSetException(string message)
            : base(message)
        { }

        public ParameterSetException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
