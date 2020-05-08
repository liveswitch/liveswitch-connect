using System;

namespace FM.LiveSwitch.Connect
{
    static class AudioCodecExtensions
    {
        public static AudioEncoder CreateEncoder(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return new Opus.Encoder();
                case AudioCodec.G722:
                    return new G722.Encoder();
                case AudioCodec.PCMU:
                    return new Pcmu.Encoder();
                case AudioCodec.PCMA:
                    return new Pcma.Encoder();
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }

        public static AudioDecoder CreateDecoder(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return new Opus.Decoder();
                case AudioCodec.G722:
                    return new G722.Decoder();
                case AudioCodec.PCMU:
                    return new Pcmu.Decoder();
                case AudioCodec.PCMA:
                    return new Pcma.Decoder();
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }

        public static AudioPipe CreatePacketizer(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return new Opus.Packetizer();
                case AudioCodec.G722:
                    return new G722.Packetizer();
                case AudioCodec.PCMU:
                    return new Pcmu.Packetizer();
                case AudioCodec.PCMA:
                    return new Pcma.Packetizer();
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }

        public static AudioPipe CreateDepacketizer(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return new Opus.Depacketizer();
                case AudioCodec.G722:
                    return new G722.Depacketizer();
                case AudioCodec.PCMU:
                    return new Pcmu.Depacketizer();
                case AudioCodec.PCMA:
                    return new Pcma.Depacketizer();
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }

        public static NullAudioSink CreateNullSink(this AudioCodec codec, bool isPacketized)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return new NullAudioSink(new Opus.Format() { IsPacketized = isPacketized });
                case AudioCodec.G722:
                    return new NullAudioSink(new G722.Format() { IsPacketized = isPacketized });
                case AudioCodec.PCMU:
                    return new NullAudioSink(new Pcmu.Format() { IsPacketized = isPacketized });
                case AudioCodec.PCMA:
                    return new NullAudioSink(new Pcma.Format() { IsPacketized = isPacketized });
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }
    }
}
