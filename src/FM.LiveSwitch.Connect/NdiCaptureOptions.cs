using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("ndicapture", HelpText = "Captures media from an ndi stream.")]
    class NdiCaptureOptions : SendOptions
    {
        [Option("stream-name", Required = true, HelpText = "Name of the NDI stream to capture.")]
        public string StreamName { get; set; }

        [Option("audio-clock-rate", Required = false, Default = 48000, HelpText = "The audio clock rate in Hz. Minimum value is 8000. Maximum value is 48000.")]
        public int AudioClockRate { get; set; }

        [Option("audio-channel-count", Required = false, Default = 2, HelpText = "The audio channel count. Minimum value is 1. Maximum value is 4.")]
        public int AudioChannelCount { get; set; }

        [Option("video-format", Required = false, Default = ImageFormat.Bgra, HelpText = "The video format. (rgb, bgr, rgba, bgra)")]
        public ImageFormat VideoFormat { get; set; }
        
        [Option("video-width", Required = false, Default = 1920, HelpText = "The video width.")]
        public new int VideoWidth { get; set; }

        [Option("video-height", Required = false, Default = 1080, HelpText = "The video height.")]
        public new int VideoHeight { get; set; }
    }
}
