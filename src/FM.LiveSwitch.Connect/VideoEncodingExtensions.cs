using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoEncodingExtensions
    {
        public static VideoCodec ToCodec(this VideoEncoding encoding)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return VideoCodec.VP8;
                case VideoEncoding.VP9:
                    return VideoCodec.VP9;
                case VideoEncoding.H264:
                    return VideoCodec.H264;
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static VideoEncoder CreateEncoder(this VideoEncoding encoding, IConnectionOptions options)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Encoder();
                case VideoEncoding.VP9:
                    return new Vp9.Encoder();
                case VideoEncoding.H264:
                    if ((options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.Nvidia) && !options.DisableNvidia)
                    {
                        return new Nvidia.H264.Encoder();
                    }
                    else if ((options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.OpenH264) && !options.DisableOpenH264)
                    {
                        return new OpenH264.Encoder();
                    }
                    else
                    {
                        throw new Exception("H.264 video codec selected, but no encoders are enabled.");
                    }
                case VideoEncoding.H265:
                    if (!options.DisableNvidia)
                    {
                        return new Nvidia.H265.Encoder();
                    }
                    else
                    {
                        throw new Exception("H.265 video codec selected, Nvidia hardware support is required but not detected.");
                    }
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static VideoDecoder CreateDecoder(this VideoEncoding encoding, IConnectionOptions options)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Decoder();
                case VideoEncoding.VP9:
                    return new Vp9.Decoder();
                case VideoEncoding.H264:
                    if ((options.H264Decoder == H264Decoder.Auto || options.H264Decoder == H264Decoder.Nvidia) && !options.DisableNvidia)
                    {
                        return new Nvidia.H264.Decoder();
                    }
                    else if ((options.H264Decoder == H264Decoder.Auto || options.H264Decoder == H264Decoder.OpenH264) && !options.DisableOpenH264)
                    {
                        return new OpenH264.Decoder();
                    }
                    else
                    {
                        throw new Exception("H.264 video codec selected, but no decoders are enabled");
                    }
                case VideoEncoding.H265:
                    if (!options.DisableNvidia)
                    {
                        return new Nvidia.H265.Decoder();
                    }
                    else
                    {
                        throw new Exception("H.265 video codec selected, Nvidia hardware support is required but not detected.");
                    }
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static VideoPacketizer CreatePacketizer(this VideoEncoding encoding)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Packetizer();
                case VideoEncoding.VP9:
                    return new Vp9.Packetizer();
                case VideoEncoding.H264:
                    return new H264.Packetizer(H264.PacketizationMode.Default);
                case VideoEncoding.H265:
                    return new H265.Packetizer();
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static VideoPipe CreateDepacketizer(this VideoEncoding encoding)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Depacketizer();
                case VideoEncoding.VP9:
                    return new Vp9.Depacketizer();
                case VideoEncoding.H264:
                    return new H264.Depacketizer(H264.PacketizationMode.Default);
                case VideoEncoding.H265:
                    return new H265.Depacketizer();
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static NullVideoSink CreateNullSink(this VideoEncoding encoding, bool isPacketized)
        {
            return new NullVideoSink(CreateFormat(encoding, isPacketized));
        }

        public static VideoFormat CreateFormat(this VideoEncoding encoding, bool isPacketized = false)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Format() { IsPacketized = isPacketized };
                case VideoEncoding.VP9:
                    return new Vp9.Format() { IsPacketized = isPacketized };
                case VideoEncoding.H264:
                    return new H264.Format(H264.ProfileLevelId.Default, H264.PacketizationMode.Default) { IsPacketized = isPacketized };
                case VideoEncoding.H265:
                    return new H265.Format(H265.ProfileLevelId.Default) { IsPacketized = isPacketized };
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }
    }
}
