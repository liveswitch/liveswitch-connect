using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    static class ClientInfoExtensions
    {
        public static Descriptor[] GetDescriptors(this ClientInfo clientInfo)
        {
            var descriptors = new List<Descriptor>();
            if (clientInfo.Region != null)
            {
                descriptors.Add(new Descriptor("Region", clientInfo.Region));
            }
            if (clientInfo.UserId != null)
            {
                descriptors.Add(new Descriptor("User ID", clientInfo.UserId));
            }
            if (clientInfo.UserAlias != null)
            {
                descriptors.Add(new Descriptor("User Alias", clientInfo.UserAlias));
            }
            if (clientInfo.DeviceId != null)
            {
                descriptors.Add(new Descriptor("Device ID", clientInfo.DeviceId));
            }
            if (clientInfo.DeviceAlias != null)
            {
                descriptors.Add(new Descriptor("Device Alias", clientInfo.DeviceAlias));
            }
            if (clientInfo.Id != null)
            {
                descriptors.Add(new Descriptor("Client ID", clientInfo.Id));
            }
            if (clientInfo.Tag != null)
            {
                descriptors.Add(new Descriptor("Client Tag", clientInfo.Tag));
            }
            if (clientInfo.Roles != null && clientInfo.Roles.Length > 0)
            {
                descriptors.Add(new Descriptor("Client Roles", string.Join(", ", clientInfo.Roles)));
            }
            return descriptors.ToArray();
        }
    }
}
