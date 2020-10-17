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
                    throw new InvalidOperationException($"Cannot convert audio codec '{codec}' to encoding.");
                default:
                    throw new InvalidOperationException($"Unexpected audio codec '{codec}'.");
            }
        }
    }
}
