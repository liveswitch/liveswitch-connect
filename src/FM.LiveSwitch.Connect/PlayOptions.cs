using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("play", HelpText = "Sends media from a local file.")]
    class PlayOptions : SendOptions
    {
        [Option("audio-path", Required = false, HelpText = "The audio file path.")]
        public string AudioPath { get; set; }

        [Option("video-path", Required = false, HelpText = "The video file path.")]
        public string VideoPath { get; set; }
    }
}
