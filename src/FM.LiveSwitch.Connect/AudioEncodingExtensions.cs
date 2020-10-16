using System;

namespace FM.LiveSwitch.Connect
{
    static class AudioEncodingExtensions
    {
        public static AudioCodec ToCodec(this AudioEncoding encoding)
        {
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    return AudioCodec.Opus;
                case AudioEncoding.G722:
                    return AudioCodec.G722;
                case AudioEncoding.PCMU:
                    return AudioCodec.PCMU;
                case AudioEncoding.PCMA:
                    return AudioCodec.PCMA;
                default:
                    throw new Exception("Unknown audio encoding.");
            }
        }

        public static AudioEncoder CreateEncoder(this AudioEncoding encoding, int? targetBitrate = null)
        {
            AudioEncoder encoder = null;
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    encoder = new Opus.Encoder();
                    break;
                case AudioEncoding.G722:
                    encoder = new G722.Encoder();
                    break;
                case AudioEncoding.PCMU:
                   encoder = new Pcmu.Encoder();
                    break;
                case AudioEncoding.PCMA:
                    encoder = new Pcma.Encoder();
                    break;
                default:
                    throw new Exception("Unknown audio encoding.");
            }
            if (targetBitrate.HasValue)
            {
                encoder.TargetBitrate = targetBitrate.Value;
            }
            return encoder;
        }

        public static AudioDecoder CreateDecoder(this AudioEncoding encoding)
        {
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    return new Opus.Decoder();
                case AudioEncoding.G722:
                    return new G722.Decoder();
                case AudioEncoding.PCMU:
                    return new Pcmu.Decoder();
                case AudioEncoding.PCMA:
                    return new Pcma.Decoder();
                default:
                    throw new Exception("Unknown audio encoding.");
            }
        }

        public static AudioPacketizer CreatePacketizer(this AudioEncoding encoding)
        {
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    return new Opus.Packetizer();
                case AudioEncoding.G722:
                    return new G722.Packetizer();
                case AudioEncoding.PCMU:
                    return new Pcmu.Packetizer();
                case AudioEncoding.PCMA:
                    return new Pcma.Packetizer();
                default:
                    throw new Exception("Unknown audio encoding.");
            }
        }

        public static AudioDepacketizer CreateDepacketizer(this AudioEncoding encoding)
        {
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    return new Opus.Depacketizer();
                case AudioEncoding.G722:
                    return new G722.Depacketizer();
                case AudioEncoding.PCMU:
                    return new Pcmu.Depacketizer();
                case AudioEncoding.PCMA:
                    return new Pcma.Depacketizer();
                default:
                    throw new Exception("Unknown audio encoding.");
            }
        }

        public static NullAudioSink CreateNullSink(this AudioEncoding encoding, bool isPacketized)
        {
            return new NullAudioSink(CreateFormat(encoding, isPacketized));
        }

        public static AudioFormat CreateFormat(this AudioEncoding encoding, bool isPacketized = false)
        {
            switch (encoding)
            {
                case AudioEncoding.Opus:
                    return new Opus.Format() { IsPacketized = isPacketized };
                case AudioEncoding.G722:
                    return new G722.Format() { IsPacketized = isPacketized, ClockRate = isPacketized ? 8000 : 16000 };
                case AudioEncoding.PCMU:
                    return new Pcmu.Format() { IsPacketized = isPacketized };
                case AudioEncoding.PCMA:
                    return new Pcma.Format() { IsPacketized = isPacketized };
                default:
                    throw new Exception("Unknown audio encoding.");
            }
        }
    }
}
