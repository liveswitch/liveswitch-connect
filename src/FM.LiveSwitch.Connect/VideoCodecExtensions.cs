using System;

namespace FM.LiveSwitch.Connect
{
    static class VideoCodecExtensions
    {
        public static VideoEncoder CreateEncoder(this VideoCodec codec)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Encoder();
                case VideoCodec.VP9:
                    return new Vp9.Encoder();
                case VideoCodec.H264:
                    if (Nvidia.Utility.NvencSupported)
                    {
                        Log.Debug("Using Nvidia Encoder.");
                        return new Nvidia.Encoder(VideoFormat.I420);
                    }
                    else
                    {
                        Log.Debug("Using OpenH264 Encoder.");
                        return new OpenH264.Encoder();
                    }
                default:
                    throw new Exception("Unknown video codec.");
            }
        }

        public static VideoDecoder CreateDecoder(this VideoCodec codec)
        {
            switch (codec)
            {
                case VideoCodec.VP8:
                    return new Vp8.Decoder();
                case VideoCodec.VP9:
                    return new Vp9.Decoder();
                case VideoCodec.H264:
                    if (Nvidia.Utility.NvdecSupported)
                    {
                        Log.Debug("Using Nvidia Decoder.");
                        return new Nvidia.Decoder();
                    }
                    else
                    {
                        Log.Debug("Using OpenH264 Decoder.");
                        return new OpenH264.Decoder();
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
                default:
                    throw new Exception("Unknown video codec.");
            }
        }
    }
}
