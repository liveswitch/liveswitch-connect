
using NewTek;
using NDI = NewTek.NDI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FM.LiveSwitch.Connect
{

    class NdiVideoSink : VideoSink
    {
        static ILog _Log = Log.GetLogger(typeof(NdiVideoSink));

        public override string Label
        {
            get { return "Ndi Video Sink"; }
        }

        protected NDI.Sender NdiSender { get; private set; }
        protected NDI.VideoFrame NdiVideoFrame { get; private set; }
        protected bool IsCheckConnectionCount { get; private set; }

        private int ndiBufferSizeBytes = 0;

        public NdiVideoSink(NDI.Sender ndiSender, int videoWidth, int videoHeight, int frameRateNumerator, int frameRateDenominator, VideoFormat format)
            : base(format)
        {
            Initialize(ndiSender, videoWidth, videoHeight, frameRateNumerator, frameRateDenominator);
        }

        private void Initialize(NDI.Sender ndiSender, int videoWidth, int videoHeight, int frameRateNumerator, int frameRateDenominator)
        {
            
            NdiSender = ndiSender;
            NdiVideoFrame = new NDI.VideoFrame(videoWidth, videoHeight, (float)videoWidth / videoHeight, frameRateNumerator, frameRateDenominator);
            this.ndiBufferSizeBytes = NdiVideoFrame.Width * NdiVideoFrame.Height * 4;
        }

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (IsCheckConnectionCount && NdiSender.GetConnections(10000) < 1)
            {
                _Log.Info(Id, "No NDI connections, not rendering audio frame");
                return;
            }
            else
            {
                IsCheckConnectionCount = false;
            }

            if (!WriteFrame(frame, inputBuffer))
            {
                if (!Deactivated)
                {
                    _Log.Error(Id, "Could not send ndi video frame.");
                }
            }
        }

        protected virtual bool WriteFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            foreach (var dataBuffer in inputBuffer.DataBuffers)
            {
                if (!TryWrite(dataBuffer, frame.LastBuffer.Width, frame.LastBuffer.Height))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual bool TryWrite(DataBuffer buffer, int width, int height)
        {
            try
            {
                int offset = Math.Max(0, (buffer.Data.Length - buffer.Length) / 8); // 8 bits per channel
                Marshal.Copy(buffer.Data, offset, NdiVideoFrame.BufferPtr, Math.Min(buffer.Length, ndiBufferSizeBytes));

                NdiSender.Send(NdiVideoFrame);
                return true;
            } 
            catch (Exception ex)
            {
                _Log.Error("Could not write Video buffer to Ndi Sender", ex);
            }
            return false;
         }


        protected override void DoDestroy()
        {
            NdiVideoFrame.Dispose();
        }
    }
}
