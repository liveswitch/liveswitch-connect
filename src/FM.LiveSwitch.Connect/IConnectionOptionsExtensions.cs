using System;
using System.Linq;

namespace FM.LiveSwitch.Connect
{
    static class IConnectionOptionsExtensions
    {
        public static AudioEncoding[] GetAudioEncodings(this IConnectionOptions options)
        {
            if (options.AudioCodec == AudioCodec.Any)
            {
                return ((AudioEncoding[])Enum.GetValues(typeof(AudioEncoding))).ToArray();
            }
            return new[] { options.AudioCodec.ToEncoding() };
        }

        public static VideoEncoding[] GetVideoEncodings(this IConnectionOptions options)
        {
            if (options.VideoCodec == VideoCodec.Any)
            {
                return ((VideoEncoding[])Enum.GetValues(typeof(VideoEncoding))).ToArray();
            }
            return new[] { options.VideoCodec.ToEncoding() };
        }
    }
}
