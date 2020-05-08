using CommandLine;
using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    abstract class SendOptions : Options, ISendOptions
    {
        [Option("audio-codecs", Required = false, Default = new[] { AudioCodec.Opus, AudioCodec.G722, AudioCodec.PCMU, AudioCodec.PCMA }, HelpText = "The allowed audio codecs.")]
        public IEnumerable<AudioCodec> AudioCodecs { get; set; }

        [Option("video-codecs", Required = false, Default = new[] { VideoCodec.VP8, VideoCodec.VP9, VideoCodec.H264 }, HelpText = "The allowed video codecs.")]
        public IEnumerable<VideoCodec> VideoCodecs { get; set; }

        [Option("media-id", Required = false, HelpText = "The local media ID.")]
        public string MediaId { get; set; }
    }
}
