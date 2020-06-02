using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoFormatExtensions
    {
        public static VideoCodec CreateCodec(this VideoFormat format)
        {
            if (format.Name == VideoFormat.Vp8Name)
            {
                return VideoCodec.VP8;
            }
            if (format.Name == VideoFormat.Vp9Name)
            {
                return VideoCodec.VP9;
            }
            if (format.Name == VideoFormat.H264Name)
            {
                return VideoCodec.H264;
            }
            throw new Exception("Unknown video format.");
        }
    }
}
