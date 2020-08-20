namespace FM.LiveSwitch.Connect
{
    interface IConnectionOptions
    {
        bool OpenH264Supported { get; }

        bool NvidiaSupported { get; }

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
