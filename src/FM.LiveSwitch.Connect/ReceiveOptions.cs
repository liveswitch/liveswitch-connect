using CommandLine;

namespace FM.LiveSwitch.Connect
{
    abstract class ReceiveOptions : StreamOptions, IReceiveOptions
    {
        [Option("media-id", Required = false, HelpText = "The remote media ID.")]
        public string MediaId { get; set; }

        [Option("connection-id", Required = false, HelpText = "The remote connection ID or 'mcu'.")]
        public string ConnectionId { get; set; }

        [Option("audio-bitrate", Required = false, HelpText = "The audio bitrate.")]
        public int? AudioBitrate { get; set; }

        [Option("video-bitrate", Required = false, HelpText = "The video bitrate.")]
        public int? VideoBitrate { get; set; }

        public bool AudioTranscode { get; set; }

        public bool VideoTranscode { get; set; }
    }
}
