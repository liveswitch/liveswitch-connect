using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    static class ClientExtensions
    {
        public static async Task Register(this Client client, IClientOptions options)
        {
            await client.Register(Token.GenerateClientRegisterToken(client, new ChannelClaim[0], options.SharedSecret));
        }

        public static async Task<Channel> Join(this Client client, IChannelOptions options)
        {
            return await client.Join(Token.GenerateClientJoinToken(client, options.ChannelId, options.SharedSecret));
        }

        public static Descriptor[] GetDescriptors(this Client client)
        {
            var descriptors = new List<Descriptor>();
            if (client.GatewayUrl != null)
            {
                descriptors.Add(new Descriptor("Gateway URL", client.GatewayUrl));
            }
            if (client.ApplicationId != null)
            {
                descriptors.Add(new Descriptor("Application ID", client.ApplicationId));
            }
            if (client.UserId != null)
            {
                descriptors.Add(new Descriptor("User ID", client.UserId));
            }
            if (client.UserAlias != null)
            {
                descriptors.Add(new Descriptor("User Alias", client.UserAlias));
            }
            if (client.DeviceId != null)
            {
                descriptors.Add(new Descriptor("Device ID", client.DeviceId));
            }
            if (client.DeviceAlias != null)
            {
                descriptors.Add(new Descriptor("Device Alias", client.DeviceAlias));
            }
            if (client.Id != null)
            {
                descriptors.Add(new Descriptor("ID", client.Id));
            }
            if (client.Tag != null)
            {
                descriptors.Add(new Descriptor("Tag", client.Tag));
            }
            if (client.Roles != null && client.Roles.Length > 0)
            {
                descriptors.Add(new Descriptor("Roles", string.Join(", ", client.Roles)));
            }
            return descriptors.ToArray();
        }
    }
}
