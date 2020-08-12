using System;

namespace FM.LiveSwitch.Connect
{
    static class AudioCodecExtensions
    {
        public static AudioEncoding ToEncoding(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.Opus:
                    return AudioEncoding.Opus;
                case AudioCodec.G722:
                    return AudioEncoding.G722;
                case AudioCodec.PCMU:
                    return AudioEncoding.PCMU;
                case AudioCodec.PCMA:
                    return AudioEncoding.PCMA;
                case AudioCodec.Any:
                    throw new Exception("Cannot convert 'any' codec to encoding.");
                default:
                    throw new Exception("Unknown audio codec.");
            }
        }
    }
}
