﻿namespace FM.LiveSwitch.Connect
{
    interface IReceiveOptions : IConnectionOptions, IChannelOptions, IClientOptions
    {
        string MediaId { get; }

        string ConnectionId { get; set; }

        bool AudioTranscode { get; }

        bool VideoTranscode { get; }

        int? AudioBitrate { get; }

        int? VideoBitrate { get; }
    }
}
