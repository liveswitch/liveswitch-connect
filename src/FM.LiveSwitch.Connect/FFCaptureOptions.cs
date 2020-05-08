using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("ffcapture", HelpText = "Captures local media from FFmpeg.")]
    class FFCaptureOptions : SendOptions
    {
        [Option("input-args", Required = true, HelpText = "The FFmpeg input arguments.")]
        public string InputArgs { get; set; }

        [Option("no-audio", Required = false, HelpText = "Do not capture audio.")]
        public bool NoAudio { get; set; }

        [Option("no-video", Required = false, HelpText = "Do not capture video.")]
        public bool NoVideo { get; set; }
    }
}
