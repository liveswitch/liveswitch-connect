namespace FM.LiveSwitch.Connect
{
    interface IChannelOptions
    {
        string ChannelId { get; }

        string SharedSecret { get; }
    }
}
