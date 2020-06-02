namespace FM.LiveSwitch.Connect
{
    interface IConnectionOptions
    {
        bool DisableOpenH264 { get; }

        string ConnectionTag { get; }

        string DataChannelLabel { get; }

        bool NoAudio { get; set; }

        bool NoVideo { get; set; }

        AudioCodec AudioCodec { get; }

        VideoCodec VideoCodec { get; }
    }
}
