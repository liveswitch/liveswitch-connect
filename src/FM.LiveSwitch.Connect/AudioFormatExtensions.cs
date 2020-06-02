using System;

namespace FM.LiveSwitch.Connect
{
    static class AudioFormatExtensions
    {
        public static AudioCodec CreateCodec(this AudioFormat format)
        {
            if (format.Name == AudioFormat.OpusName)
            {
                return AudioCodec.Opus;
            }
            if (format.Name == AudioFormat.G722Name)
            {
                return AudioCodec.G722;
            }
            if (format.Name == AudioFormat.PcmuName)
            {
                return AudioCodec.PCMU;
            }
            if (format.Name == AudioFormat.PcmaName)
            {
                return AudioCodec.PCMA;
            }
            throw new Exception("Unknown audio format.");
        }
    }
}
