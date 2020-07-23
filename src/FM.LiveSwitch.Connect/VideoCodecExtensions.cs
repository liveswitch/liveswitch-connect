using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoCodecExtensions
    {
        public static VideoEncoder CreateEncoder(this VideoCodec codec, IConnectionOptions options)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Encoder();
                case VideoCodec.VP9:
                    return new Vp9.Encoder();
                case VideoCodec.H264:
                    if ((options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.NVENC) && !options.DisableNvidia)
                    {
                        return new Nvidia.H264.Encoder();
                    }
                    else if ((options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.OpenH264) && !options.DisableOpenH264)
                    {
                        return new OpenH264.Encoder();
                    }
                    else
                    {
                        throw new Exception("H.264 video codec selected, but no encoders are enabled");
                    }
                case VideoCodec.H265:
                    if (!options.DisableNvidia)
                    {
                        return new Nvidia.H265.Encoder();
                    }
                    else
                    {
                        throw new Exception("H.265 video codec selected, Nvidia hardware support is required but not detected.");
                    }
                default:
                    throw new Exception("Unknown video codec.");
            }
        }

        public static VideoDecoder CreateDecoder(this VideoCodec codec, IConnectionOptions options)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Decoder();
                case VideoCodec.VP9:
                    return new Vp9.Decoder();
                case VideoCodec.H264:
                    if ((options.H264Decoder == H264Decoder.Auto || options.H264Decoder == H264Decoder.NVDEC) && !options.DisableNvidia)
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
                case VideoCodec.H265:
                    if (!options.DisableNvidia)
                    {
                        return new Nvidia.H265.Decoder();
                    }
                    else
                    {
                        throw new Exception("H.265 video codec selected, Nvidia hardware support is required but not detected.");
                    }
                default:
                    throw new Exception("Unknown video codec.");
            }
        }

        public static VideoPacketizer CreatePacketizer(this VideoCodec codec)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Packetizer();
                case VideoCodec.VP9:
                    return new Vp9.Packetizer();
                case VideoCodec.H264:
                    return new H264.Packetizer();
                case VideoCodec.H265:
                    return new H265.Packetizer();
                default:
                    throw new Exception("Unknown video codec.");
            }
        }

        public static VideoDepacketizer<VideoFragment> CreateDepacketizer(this VideoCodec codec)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Depacketizer() as object as VideoDepacketizer<VideoFragment>;
                case VideoCodec.VP9:
                    return new Vp9.Depacketizer() as object as VideoDepacketizer<VideoFragment>;
                case VideoCodec.H264:
                    return new H264.Depacketizer() as object as VideoDepacketizer<VideoFragment>;
                case VideoCodec.H265:
                    return new H265.Depacketizer() as object as VideoDepacketizer<VideoFragment>;
                default:
                    throw new Exception("Unknown video codec.");
            }
        }

        public static NullVideoSink CreateNullSink(this VideoCodec codec, bool isPacketized)
        {
            return new NullVideoSink(CreateFormat(codec, isPacketized));
        }

        public static VideoFormat CreateFormat(this VideoCodec codec, bool isPacketized = false)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Format() { IsPacketized = isPacketized };
                case VideoCodec.VP9:
                    return new Vp9.Format() { IsPacketized = isPacketized };
                case VideoCodec.H264:
                    return new H264.Format(H264.ProfileLevelId.Default, H264.PacketizationMode.Default) { IsPacketized = isPacketized };
                case VideoCodec.H265:
                    return new H265.Format() { IsPacketized = isPacketized };
                default:
                    throw new Exception("Unknown video codec.");
            }
        }
    }
}
