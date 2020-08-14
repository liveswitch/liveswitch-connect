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

        public static VideoEncoder CreateEncoder(this VideoEncoding encoding)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Encoder();
                case VideoEncoding.VP9:
                    return new Vp9.Encoder();
                case VideoEncoding.H264:
                    return new OpenH264.Encoder();
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }

        public static VideoDecoder CreateDecoder(this VideoEncoding encoding)
        {
            switch (encoding)
            {
                case VideoEncoding.VP8:
                    return new Vp8.Decoder();
                case VideoEncoding.VP9:
                    return new Vp9.Decoder();
                case VideoEncoding.H264:
                    return new OpenH264.Decoder();
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
                default:
                    throw new Exception("Unknown video encoding.");
            }
        }
    }
}
