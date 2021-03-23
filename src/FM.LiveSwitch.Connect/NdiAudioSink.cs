
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

        protected NDI.Sender _NdiSender { get; private set; }
        protected NDI.AudioFrame16bpp _NdiAudioFrame16bpp { get; private set; }
        protected bool _IsCheckConnectionCount { get; private set; }

        public NdiAudioSink(NDI.Sender ndiSender, int maxRate, int sampleRate, int channelCount, AudioFormat format)
            : base(format)
        {
            Initialize(ndiSender, maxRate,  sampleRate, channelCount); // 20ms audio samples.
        }

        private void Initialize(NDI.Sender ndiSender, int maxRate, int sampleRate, int channelCount)
        {
            _NdiSender = ndiSender;
            _NdiAudioFrame16bpp = new NDI.AudioFrame16bpp(maxRate, sampleRate, channelCount);
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            if (_IsCheckConnectionCount && _NdiSender.GetConnections(10000) < 1)
            {
                _Log.Error(Id, "No NDI connections, not rendering audio frame");
                return;
            } else
            {
                _IsCheckConnectionCount = false;
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
                var chanLength = buffer.Length / _NdiAudioFrame16bpp.NumChannels;
                for (int ch = 0; ch < _NdiAudioFrame16bpp.NumChannels; ch++)
                {
                    // Calculate the size of each channel
                    int channelStride = ch * _NdiAudioFrame16bpp.NumSamples * sizeof(short);

                    // Set the pointer to the start of this channel.
                    IntPtr destStart = new IntPtr(_NdiAudioFrame16bpp.AudioBuffer.ToInt64() + channelStride);

                    // Write the byytes
                    Marshal.Copy(buffer.Data, ch * chanLength, destStart, chanLength);
                }

                // Send the frame
                _NdiSender.Send(_NdiAudioFrame16bpp);
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
            if (_NdiAudioFrame16bpp != null) {
                _NdiAudioFrame16bpp.Dispose();
            }
        }
    }
}
