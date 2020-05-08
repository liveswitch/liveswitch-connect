using CommandLine;

namespace FM.LiveSwitch.Connect.Shell
{
    [Verb("connections", HelpText = "Prints remote connection details to stdout.")]
    class ConnectionsOptions
    {
        [Option("ids", Required = false, HelpText = "Print IDs only.")]
        public bool Ids { get; set; }

        [Option("listen", Required = false, HelpText = "Listen for open/close events. (Press Q to stop.)")]
        public bool Listen { get; set; }
    }
}
