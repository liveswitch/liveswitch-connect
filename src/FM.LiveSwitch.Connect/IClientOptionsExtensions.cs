using System;
using System.Linq;

namespace FM.LiveSwitch.Connect
{
    static class IClientOptionsExtensions
    {
        public static Client CreateClient(this IClientOptions options, bool logState = true)
        {
            var client = new Client(options.GatewayUrl, options.ApplicationId, options.UserId, options.DeviceId, null, options.ClientRoles.ToArray())
            {
                UserAlias = options.UserAlias,
                DeviceAlias = options.DeviceAlias,
                Tag = options.ClientTag
            };

            if (logState)
            {
                client.OnStateChange += (c) =>
                {
                    Console.Error.WriteLine($"Client '{client.Id}' state is {client.State.ToString().ToLower()}.");
                };
            }

            return client;
        }
    }
}
