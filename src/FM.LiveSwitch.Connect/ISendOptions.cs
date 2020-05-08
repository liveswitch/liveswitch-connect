using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    interface ISendOptions : IConnectionOptions, IChannelOptions, IClientOptions
    {
        string MediaId { get; }

        public IEnumerable<AudioCodec> AudioCodecs { get; }

        public IEnumerable<VideoCodec> VideoCodecs { get; }
    }
}
