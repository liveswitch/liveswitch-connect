using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("shell", HelpText = "Starts an interactive shell.")]
    class ShellOptions
    {
        public bool DisableOpenH264 { get; set; }

        public bool DisableNvidia { get; set; }

        [Option("gateway-url", Required = true, HelpText = "The gateway URL.")]
        public string GatewayUrl { get; set; }

        [Option("application-id", Required = true, HelpText = "The application ID.")]
        public string ApplicationId { get; set; }

        [Option("shared-secret", Required = true, HelpText = "The shared secret for the application ID.")]
        public string SharedSecret { get; set; }
    }
}
