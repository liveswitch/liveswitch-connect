using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("spoutrender", HelpText = "Renders remote media to a spout buffer.")]
    class SpoutRenderOptions : ReceiveOptions
    {
        [Option("spout-name", Required = false, Default = "LiveswitchConnect", HelpText = "Name of the spout buffer")]
        public string NdiName { get; set; }

        [Option("video-format", Required = false, Default = ImageFormat.Bgr, HelpText = "The video format.")]
        public ImageFormat VideoFormat { get; set; }
        
        [Option("video-width", Required = false, Default = 800, HelpText = "The video width.")]
        public int VideoWidth { get; set; }

        [Option("video-height", Required = false, Default = 800, HelpText = "The video height.")]
        public int VideoHeight { get; set; }
    }
}
