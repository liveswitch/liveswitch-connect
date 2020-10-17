using System;

namespace FM.LiveSwitch.Connect
{
    public class NegotiateException : Exception
    {
        public NegotiateException(string message)
            : base(message)
        { }
    }
}
