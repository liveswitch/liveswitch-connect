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
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
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
                    if (options.NvidiaSupported && (options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.Nvidia))
                    {
                        return new Nvidia.H264.Encoder();
                    }
                    if (options.OpenH264Supported && (options.H264Encoder == H264Encoder.Auto || options.H264Encoder == H264Encoder.OpenH264))
                    {
                        return new OpenH264.Encoder();
                    }
                    throw new NotSupportedException("No H.264 encoders available.");
                case VideoEncoding.H265:
                    if (options.NvidiaSupported)
                    {
                        return new Nvidia.H265.Encoder();
                    }
                    throw new NotSupportedException("No H.265 encoders available.");
                default:
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
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
                    if (options.NvidiaSupported && (options.H264Decoder == H264Decoder.Auto || options.H264Decoder == H264Decoder.Nvidia))
                    {
                        return new Nvidia.H264.Decoder();
                    }
                    if (options.OpenH264Supported && (options.H264Decoder == H264Decoder.Auto || options.H264Decoder == H264Decoder.OpenH264))
                    {
                        return new OpenH264.Decoder();
                    }
                    throw new NotSupportedException("No H.264 decoders available.");
                case VideoEncoding.H265:
                    if (options.NvidiaSupported)
                    {
                        return new Nvidia.H265.Decoder();
                    }
                    throw new NotSupportedException("No H.265 decoders available.");
                default:
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
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
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
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
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
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
                    return new Vp8.Format { IsPacketized = isPacketized };
                case VideoEncoding.VP9:
                    return new Vp9.Format { IsPacketized = isPacketized };
                case VideoEncoding.H264:
                    return new H264.Format(H264.ProfileLevelId.Default, H264.PacketizationMode.Default) { IsPacketized = isPacketized };
                case VideoEncoding.H265:
                    return new H265.Format { IsPacketized = isPacketized };
                default:
                    throw new InvalidOperationException($"Unexpected video encoding '{encoding}'.");
            }
        }
    }
}
