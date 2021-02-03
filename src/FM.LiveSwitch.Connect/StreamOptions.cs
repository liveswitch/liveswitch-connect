using CommandLine;
using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    abstract class StreamOptions : Options, IConnectionOptions, IChannelOptions, IClientOptions
    {
        [Option("channel-id", Required = true, HelpText = "The channel ID.")]
        public string ChannelId { get; set; }

        [Option("data-channel-label", Required = false, HelpText = "The data channel label.")]
        public string DataChannelLabel { get; set; }

        [Option("region", Required = false, HelpText = "The local region.")]
        public string Region { get; set; }

        [Option("user-id", Required = false, HelpText = "The local user ID.")]
        public string UserId { get; set; }

        [Option("user-alias", Required = false, HelpText = "The local user alias.")]
        public string UserAlias { get; set; }

        [Option("device-id", Required = false, HelpText = "The local device ID.")]
        public string DeviceId { get; set; }

        [Option("device-alias", Required = false, HelpText = "The local device alias.")]
        public string DeviceAlias { get; set; }

        [Option("client-tag", Required = false, HelpText = "The local client tag.")]
        public string ClientTag { get; set; }

        [Option("client-roles", Required = false, HelpText = "The local client roles.")]
        public IEnumerable<string> ClientRoles { get; set; }

        [Option("connection-tag", Required = false, HelpText = "The local connection tag.")]
        public string ConnectionTag { get; set; }

        [Option("no-audio", Required = false, HelpText = "Do not process audio.")]
        public bool NoAudio { get; set; }

        [Option("no-video", Required = false, HelpText = "Do not process video.")]
        public bool NoVideo { get; set; }

        [Option("audio-codec", Required = false, Default = AudioCodec.Any, HelpText = "The audio codec to negotiate with LiveSwitch.")]
        public AudioCodec AudioCodec { get; set; }

        [Option("video-codec", Required = false, Default = VideoCodec.Any, HelpText = "The video codec to negotiate with LiveSwitch.")]
        public VideoCodec VideoCodec { get; set; }

        [Option("h264-encoder", Required = false, Default = H264Encoder.Auto, HelpText = "The H.264 encoder to use.")]
        public H264Encoder H264Encoder { get; set; }

        [Option("h264-decoder", Required = false, Default = H264Decoder.Auto, HelpText = "The H.264 decoder to use.")]
        public H264Decoder H264Decoder { get; set; }
    }
}
