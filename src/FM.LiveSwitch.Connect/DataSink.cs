using System;

namespace FM.LiveSwitch.Connect
{
    class DataSink
    {
        public void ProcessReceive(DataChannelReceiveArgs args)
        {
            if (args.DataString != null)
            {
                Console.WriteLine(args.DataString);
            }
            else
            {
                Console.Error.WriteLine($"Cannot write binary message from data channel: {args.DataBytes.ToHexString()}");
            }
        }

        public void Destroy() { }
    }
}
