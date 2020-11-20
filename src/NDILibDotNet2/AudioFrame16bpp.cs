using System;
using System.Runtime.InteropServices;

namespace NewTek.NDI
{
    public class AudioFrame16bpp: IDisposable
    {
        public AudioFrame16bpp(int maxSamples, int sampleRate, int numChannels)
        {
            // we have to know to free it later
            _memoryOwned = true;

            IntPtr audioBufferPtr = Marshal.AllocHGlobal(numChannels * maxSamples * sizeof(short));

            _ndiAudioFrame = new NDIlib.audio_frame_interleaved_16s_t()
            {
                sample_rate = sampleRate,
                no_channels = numChannels,
                no_samples = maxSamples,
                timecode = NDIlib.send_timecode_synthesize,
                p_data = audioBufferPtr,
                reference_level = 0
            };
        }

        public IntPtr AudioBuffer
        {
            get
            {
                return _ndiAudioFrame.p_data;
            }
        }

        public int NumSamples
        {
            get
            {
                return _ndiAudioFrame.no_samples;
            }

            set
            {
                _ndiAudioFrame.no_samples = value;
            }
        }

        public int NumChannels
        {
            get
            {
                return _ndiAudioFrame.no_channels;
            }

            set
            {
                _ndiAudioFrame.no_channels = value;
            }
        }

        public int SampleRate
        {
            get
            {
                return _ndiAudioFrame.sample_rate;
            }

            set
            {
                _ndiAudioFrame.sample_rate = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AudioFrame16bpp()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_memoryOwned && _ndiAudioFrame.p_data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_ndiAudioFrame.p_data);
                    _ndiAudioFrame.p_data = IntPtr.Zero;
                }

                NDIlib.destroy();
            }
        }

        internal NDIlib.audio_frame_interleaved_16s_t _ndiAudioFrame;
        bool _memoryOwned = false;
    }
}
