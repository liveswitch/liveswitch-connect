using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("fake", HelpText = "Sends media from a fake source.")]
    class FakeOptions : SendOptions
    {
        [Option("audio-clock-rate", Required = false, Default = 48000, HelpText = "The audio clock rate in Hz. Must be a multiple of 8000. Minimum value is 8000. Maximum value is 96000.")]
        public int AudioClockRate { get; set; }

        [Option("audio-channel-count", Required = false, Default = 2, HelpText = "The audio channel count. Minimum value is 1. Maximum value is 2.")]
        public int AudioChannelCount { get; set; }

        [Option("audio-frequency", Required = false, Default = 440, HelpText = "The audio frequency in Hz. Minimum value is 20. Maximum value is 20000.")]
        public float AudioFrequency { get; set; }

        [Option("audio-amplitude", Required = false, Default = 16383, HelpText = "The audio amplitude. Minimum value is 1. Maximum value is 32767.")]
        public int AudioAmplitude { get; set; }

        [Option("video-format", Required = false, Default = ImageFormat.Bgr, HelpText = "The video format.")]
        public ImageFormat VideoFormat { get; set; }

        [Option("video-width", Required = false, Default = 640, HelpText = "The video width. Must be a multiiple of 2.")]
        public int VideoWidth { get; set; }

        [Option("video-height", Required = false, Default = 480, HelpText = "The video height. Must be a multiiple of 2.")]
        public int VideoHeight { get; set; }

        [Option("video-frame-rate", Required = false, Default = 30, HelpText = "The video frame rate. Minimum value is 1. Maximum value is 120.")]
        public double VideoFrameRate { get; set; }

        [Option("no-audio", Required = false, HelpText = "Do not fake audio.")]
        public bool NoAudio { get; set; }

        [Option("no-video", Required = false, HelpText = "Do not fake video.")]
        public bool NoVideo { get; set; }
    }
}
