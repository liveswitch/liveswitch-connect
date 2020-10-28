
using NewTek;
using NDI = NewTek.NDI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Dynamic;

namespace FM.LiveSwitch.Connect
{

    class NdiAudioSink : AudioSink
    {
        static ILog _Log = Log.GetLogger(typeof(NdiVideoSink));

        public override string Label
        {
            get { return "Ndi Video Sink"; }
        }

        protected NDI.Sender NdiSender { get; private set; }
        protected NDI.AudioFrame NdiAudioFrame { get; private set; }

        protected bool IsCheckConnectionCount { get; private set; }

        public NdiAudioSink(NDI.Sender ndiSender, int sampleRate, int channelCount, AudioFormat format)
            : base(format)
        {
            Initialize(ndiSender, sampleRate, channelCount);
        }

        private void Initialize(NDI.Sender ndiSender,int sampleRate, int channelCount)
        {
            NdiSender = ndiSender;
            NdiAudioFrame = new NDI.AudioFrame(1700, sampleRate, channelCount);
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
                IntPtr destStart = new IntPtr(NdiAudioFrame.AudioBuffer.ToInt64());
                Marshal.Copy(buffer.Data, 0, destStart, buffer.Length);
                NdiSender.Send(NdiAudioFrame);
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
            NdiAudioFrame.Dispose();
        }
    }
}
