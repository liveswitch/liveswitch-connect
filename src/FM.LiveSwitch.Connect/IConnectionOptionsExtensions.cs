using System;
using System.Linq;

namespace FM.LiveSwitch.Connect
{
    static class IConnectionOptionsExtensions
    {
        public static AudioCodec[] GetAudioCodecs(this IConnectionOptions options)
        {
            if (options.AudioCodec == AudioCodec.Any)
            {
                return ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Any).ToArray();
            }
            return new[] { options.AudioCodec };
        }

        public static VideoCodec[] GetVideoCodecs(this IConnectionOptions options)
        {
            if (options.VideoCodec == VideoCodec.Any)
            {
                return ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Any).ToArray();
            }
            return new[] { options.VideoCodec };
        }
    }
}
