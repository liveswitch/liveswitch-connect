using System;

namespace FM.LiveSwitch.Connect
{
    static class ImageFormatExtensions
    {
        public static VideoFormat CreateFormat(this ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.Rgb:
                    return VideoFormat.Rgb;
                case ImageFormat.Bgr:
                    return VideoFormat.Bgr;
                case ImageFormat.Rgba:
                    return VideoFormat.Rgba;
                case ImageFormat.Bgra:
                    return VideoFormat.Bgra;
                case ImageFormat.Argb:
                    return VideoFormat.Argb;
                case ImageFormat.Abgr:
                    return VideoFormat.Abgr;
                case ImageFormat.I420:
                    return VideoFormat.I420;
                case ImageFormat.Yv12:
                    return VideoFormat.Yv12;
                case ImageFormat.Nv12:
                    return VideoFormat.Nv12;
                case ImageFormat.Nv21:
                    return VideoFormat.Nv21;
                default:
                    throw new Exception("Unknown video format.");
            }
        }
    }
}
