using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    interface IClientOptions
    {
        string GatewayUrl { get; }

        string ApplicationId { get; }

        string SharedSecret { get; }

        string Region { get; }

        string UserId { get; }

        string UserAlias { get; }

        string DeviceId { get; }

        string DeviceAlias { get; }

        string ClientTag { get; }

        IEnumerable<string> ClientRoles { get; }
    }
}
