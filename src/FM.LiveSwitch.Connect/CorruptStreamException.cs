using System;

namespace FM.LiveSwitch.Connect
{
    public class CorruptStreamException : Exception
    {
        public CorruptStreamException(string message)
            : base(message)
        { }
    }
}
