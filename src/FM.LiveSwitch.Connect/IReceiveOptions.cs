namespace FM.LiveSwitch.Connect
{
    interface IReceiveOptions : IConnectionOptions, IChannelOptions, IClientOptions
    {
        string ConnectionId { get; set; }
    }
}
