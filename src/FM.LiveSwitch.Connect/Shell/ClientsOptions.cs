using CommandLine;

namespace FM.LiveSwitch.Connect.Shell
{
    [Verb("clients", HelpText = "Prints remote client details to stdout.")]
    class ClientsOptions
    {
        [Option("ids", Required = false, HelpText = "Print IDs only.")]
        public bool Ids { get; set; }

        [Option("listen", Required  = false, HelpText = "Listen for join/leave events. (Press Q to stop.)")]
        public bool Listen { get; set; }
    }
}
