using System;

namespace FM.LiveSwitch.Connect
{
    static class AudioFormatExtensions
    {
        public static AudioEncoding ToEncoding(this AudioFormat format)
        {
            if (format.Name == AudioFormat.OpusName)
            {
                return AudioEncoding.Opus;
            }
            if (format.Name == AudioFormat.G722Name)
            {
                return AudioEncoding.G722;
            }
            if (format.Name == AudioFormat.PcmuName)
            {
                return AudioEncoding.PCMU;
            }
            if (format.Name == AudioFormat.PcmaName)
            {
                return AudioEncoding.PCMA;
            }
            throw new Exception("Unknown audio format.");
        }
    }
}
