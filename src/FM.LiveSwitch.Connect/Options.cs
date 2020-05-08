using CommandLine;
using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    abstract class Options : IChannelOptions, IClientOptions
    {
        public bool DisableOpenH264 { get; set; }

        [Option("gateway-url", Required = true, HelpText = "The gateway URL.")]
        public string GatewayUrl { get; set; }

        [Option("application-id", Required = true, HelpText = "The application ID.")]
        public string ApplicationId { get; set; }

        [Option("shared-secret", Required = true, HelpText = "The shared secret for the application ID.")]
        public string SharedSecret { get; set; }

        [Option("channel-id", Required = true, HelpText = "The channel ID.")]
        public string ChannelId { get; set; }

        [Option("data-channel-label", Required = false, HelpText = "The data channel label.")]
        public string DataChannelLabel { get; set; }

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

        [Option("connection-tag", Required = false, HelpText = "The local connection tag.")]
        public string ConnectionTag { get; set; }
    }
}
