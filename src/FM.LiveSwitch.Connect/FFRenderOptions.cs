using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("ffrender", HelpText = "Renders remote media to FFmpeg.")]
    class FFRenderOptions : ReceiveOptions
    {
        [Option("output-args", Required = true, HelpText = "The FFmpeg output arguments.")]
        public string OutputArgs { get; set; }
    }
}
