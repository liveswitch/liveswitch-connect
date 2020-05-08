using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    interface IConnectionOptions
    {
        bool DisableOpenH264 { get; }

        string ConnectionTag { get; }

        string DataChannelLabel { get; }
    }
}
