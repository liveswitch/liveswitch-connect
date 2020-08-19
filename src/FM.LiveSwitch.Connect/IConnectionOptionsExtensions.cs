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

        public static bool IsH264EncoderAvailable(this IConnectionOptions options)
        {
            switch (options.H264Encoder)
            {
                case H264Encoder.Auto:
                    return !options.DisableOpenH264 || !options.DisableNvidia;
                case H264Encoder.OpenH264:
                    return !options.DisableOpenH264;
                case H264Encoder.NVENC:
                    return !options.DisableNvidia;
                default:
                    return false;
            }
        }

        public static bool IsH264DecoderAvailable(this IConnectionOptions options)
        {
            switch (options.H264Decoder)
            {
                case H264Decoder.Auto:
                    return !options.DisableOpenH264 || !options.DisableNvidia;
                case H264Decoder.OpenH264:
                    return !options.DisableOpenH264;
                case H264Decoder.NVDEC:
                    return !options.DisableNvidia;
                default:
                    return false;
            }
        }
    }
}
