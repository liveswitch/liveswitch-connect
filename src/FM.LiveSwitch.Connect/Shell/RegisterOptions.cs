using CommandLine;
using System.Collections.Generic;

namespace FM.LiveSwitch.Connect.Shell
{
    [Verb("register", HelpText = "Registers a client.")]
    class RegisterOptions : IClientOptions
    {
        public string GatewayUrl { get; set; }

        public string ApplicationId { get; set; }

        public string SharedSecret { get; set; }

        [Option("user-id", Required = false, HelpText = "The local user ID.")]
        public string UserId { get; set; }

        [Option("user-alias", Required = false, HelpText = "The local user alias.")]
        public string UserAlias { get; set; }

        [Option("device-id", Required = false, HelpText = "The local device ID.")]
        public string DeviceId { get; set; }

        [Option("device-alias", Required = false, HelpText = "The local device alias.")]
        public string DeviceAlias { get; set; }

        [Option("client-tag", Required = false, HelpText = "The local client tag.")]
        public string ClientTag { get; set; }

        [Option("client-roles", Required = false, HelpText = "The local client roles.")]
        public IEnumerable<string> ClientRoles { get; set; }
    }
}
