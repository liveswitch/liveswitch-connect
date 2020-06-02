namespace FM.LiveSwitch.Connect
{
    interface ISendOptions : IConnectionOptions, IChannelOptions, IClientOptions
    {
        string MediaId { get; }

        int AudioBitrate { get; }

        int VideoBitrate { get; }
    }
}
