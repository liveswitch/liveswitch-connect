using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoFormatExtensions
    {
        public static VideoEncoding ToEncoding(this VideoFormat format)
        {
            if (format.Name == VideoFormat.Vp8Name)
            {
                return VideoEncoding.VP8;
            }
            if (format.Name == VideoFormat.Vp9Name)
            {
                return VideoEncoding.VP9;
            }
            if (format.Name == VideoFormat.H264Name)
            {
                return VideoEncoding.H264;
            }
            if (format.Name == VideoFormat.H265Name)
            {
                return VideoEncoding.H265;
            }
            throw new Exception("Unknown video format.");
        }
    }
}
