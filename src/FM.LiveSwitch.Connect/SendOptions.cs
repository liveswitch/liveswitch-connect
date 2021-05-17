using CommandLine;

namespace FM.LiveSwitch.Connect
{
    abstract class SendOptions : StreamOptions, ISendOptions
    {
        [Option("media-id", Required = false, HelpText = "The local media ID.")]
        public string MediaId { get; set; }

        [Option("audio-bitrate", Required = false, HelpText = "The audio bitrate in kbps.")]
        public int? AudioBitrate { get; set; }

        [Option("video-bitrate", Required = false, HelpText = "The video bitrate in kbps.")]
        public int? VideoBitrate { get; set; }

        [Option("video-width", Required = false, HelpText = "The video width, if known, for signalling.")]
        public int? VideoWidth { get; set; }

        [Option("video-height", Required = false, HelpText = "The video height, if known, for signalling.")]
        public int? VideoHeight { get; set; }

        [Option("video-frame-rate", Required = false, HelpText = "The video frame-rate, if known, for signalling.")]
        public double? VideoFrameRate { get; set; }

        public bool AudioTranscode { get; set; }

        public bool VideoTranscode { get; set; }
    }
}
