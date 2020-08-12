using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoCodecExtensions
    {
        public static VideoEncoding ToEncoding(this VideoCodec codec)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return VideoEncoding.VP8;
                case VideoCodec.VP9:
                    return VideoEncoding.VP9;
                case VideoCodec.H264:
                    return VideoEncoding.H264;
                case VideoCodec.Any:
                    throw new Exception("Cannot convert 'any' codec to encoding.");
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

    }
}
