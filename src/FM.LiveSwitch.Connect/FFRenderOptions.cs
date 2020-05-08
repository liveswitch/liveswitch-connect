using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("ffrender", HelpText = "Renders remote media to FFmpeg.")]
    class FFRenderOptions : ReceiveOptions
    {
        [Option("output-args", Required = true, HelpText = "The FFmpeg output arguments.")]
        public string OutputArgs { get; set; }

        [Option("no-audio", Required = false, HelpText = "Do not render audio.")]
        public bool NoAudio { get; set; }

        [Option("no-video", Required = false, HelpText = "Do not render video.")]
        public bool NoVideo { get; set; }
    }
}
