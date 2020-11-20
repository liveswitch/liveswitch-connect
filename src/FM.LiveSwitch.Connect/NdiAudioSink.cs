
using NewTek;
using NDI = NewTek.NDI;
using System;
using System.Runtime.InteropServices;

namespace FM.LiveSwitch.Connect
{

    class NdiAudioSink : AudioSink
    {
        static ILog _Log = Log.GetLogger(typeof(NdiAudioSink));

        public override string Label
        {
            get { return "Ndi Audio Sink"; }
        }

        protected NDI.Sender NdiSender { get; private set; }
        protected NDI.AudioFrame16bpp NdiAudioFrame16bpp { get; private set; }
        protected bool IsCheckConnectionCount { get; private set; }

        public NdiAudioSink(NDI.Sender ndiSender, int maxRate, int sampleRate, int channelCount, AudioFormat format)
            : base(format)
        {
            Initialize(ndiSender, maxRate,  sampleRate, channelCount); // 20ms audio samples.
        }

        private void Initialize(NDI.Sender ndiSender, int maxRate, int sampleRate, int channelCount)
        {
            NdiSender = ndiSender;
            NdiAudioFrame16bpp = new NDI.AudioFrame16bpp(maxRate, sampleRate, channelCount);
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            if (IsCheckConnectionCount && NdiSender.GetConnections(10000) < 1)
            {
                _Log.Error(Id, "No NDI connections, not rendering audio frame");
                return;
            } else
            {
                IsCheckConnectionCount = false;
            }

            if (!WriteFrame(frame, inputBuffer))
            {
                if (!Deactivated)
                {
                    _Log.Error(Id, "Could not send ndi audio frame.");
                }
            }
        }

        protected virtual bool WriteFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            foreach (var dataBuffer in inputBuffer.DataBuffers)
            {
                if (!TryWrite(dataBuffer))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual bool TryWrite(DataBuffer buffer)
        {
            try
            {
                // Split across the channels
                var chanLength = buffer.Length / NdiAudioFrame16bpp.NumChannels;
                for (int ch = 0; ch < NdiAudioFrame16bpp.NumChannels; ch++)
                {
                    // Calculate the size of each channel
                    int channelStride = ch * NdiAudioFrame16bpp.NumSamples * sizeof(short);

                    // Set the pointer to the start of this channel.
                    IntPtr destStart = new IntPtr(NdiAudioFrame16bpp.AudioBuffer.ToInt64() + channelStride);

                    // Write the byytes
                    Marshal.Copy(buffer.Data, ch * chanLength, destStart, chanLength);
                }

                // Send the frame
                NdiSender.Send(NdiAudioFrame16bpp);
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("Could not write audio buffer to Ndi Sender", ex);
            }
            return false;
        }

        protected override void DoDestroy()
        {
            NdiAudioFrame16bpp.Dispose();
        }
    }
}
