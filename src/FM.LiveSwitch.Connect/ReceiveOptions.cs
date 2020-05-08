using CommandLine;

namespace FM.LiveSwitch.Connect
{
    abstract class ReceiveOptions : Options, IReceiveOptions
    {
        [Option("connection-id", Required = true, HelpText = "The remote connection ID.")]
        public string ConnectionId { get; set; }
    }
}
