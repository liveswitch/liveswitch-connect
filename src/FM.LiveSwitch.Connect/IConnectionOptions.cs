namespace FM.LiveSwitch.Connect
{
    interface IConnectionOptions
    {
        bool DisableOpenH264 { get; }

        bool DisableNvidia { get; }

        string ConnectionTag { get; }

        string DataChannelLabel { get; }

        bool NoAudio { get; set; }

        bool NoVideo { get; set; }

        AudioCodec AudioCodec { get; }

        VideoCodec VideoCodec { get; }

        H264Encoder H264Encoder { get; }

        H264Decoder H264Decoder { get; }
    }
}
