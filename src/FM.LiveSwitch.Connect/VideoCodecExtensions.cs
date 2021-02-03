﻿using System;

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
                case VideoCodec.H265:
                    return VideoEncoding.H265;
                case VideoCodec.Any:
                    throw new InvalidOperationException($"Cannot convert video codec '{codec}' to encoding.");
                default:
                    throw new InvalidOperationException($"Unexpected video codec '{codec}'.");
            }
        }
    }
}
