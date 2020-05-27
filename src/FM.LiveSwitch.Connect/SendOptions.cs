using CommandLine;

namespace FM.LiveSwitch.Connect
{
    abstract class SendOptions : Options, ISendOptions
    {
        [Option("media-id", Required = false, HelpText = "The local media ID.")]
        public string MediaId { get; set; }

        [Option("audio-bitrate", Required = false, Default = 32, HelpText = "The requested audio bitrate.")]
        public int AudioBitrate { get; set; }

        [Option("video-bitrate", Required = false, Default = 1000, HelpText = "The requested video bitrate.")]
        public int VideoBitrate { get; set; }
    }
}
