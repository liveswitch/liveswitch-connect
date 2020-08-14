using CommandLine;

namespace FM.LiveSwitch.Connect
{
    abstract class ReceiveOptions : StreamOptions, IReceiveOptions
    {
        [Option("connection-id", Required = true, HelpText = "The remote connection ID or 'mcu'.")]
        public string ConnectionId { get; set; }
    }
}
