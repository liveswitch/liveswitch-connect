using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("intercept", HelpText = "Forwards packets for lawful intercept.")]
    class InterceptOptions : ReceiveOptions
    {
        [Option("audio-port", Required = false, HelpText = "The destination port for audio packets.")]
        public int AudioPort { get; set; }

        [Option("video-port", Required = false, HelpText = "The destination port for video packets.")]
        public int VideoPort { get; set; }

        [Option("audio-ip-address", Required = false, Default = "127.0.0.1", HelpText = "The destination IP address for audio packets.")]
        public string AudioIPAddress { get; set; }

        [Option("video-ip-address", Required = false, Default = "127.0.0.1", HelpText = "The destination IP address for video packets.")]
        public string VideoIPAddress { get; set; }
    }
}
