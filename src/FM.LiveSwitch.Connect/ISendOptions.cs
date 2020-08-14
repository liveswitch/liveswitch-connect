namespace FM.LiveSwitch.Connect
{
    interface ISendOptions : IConnectionOptions, IChannelOptions, IClientOptions
    {
        string MediaId { get; }

        bool AudioTranscode { get; }

        bool VideoTranscode { get; }

        int? AudioBitrate { get; }

        int? VideoBitrate { get; }

        int? VideoWidth { get; set; }

        int? VideoHeight { get; set; }

        double? VideoFrameRate { get; set; }
    }
}
