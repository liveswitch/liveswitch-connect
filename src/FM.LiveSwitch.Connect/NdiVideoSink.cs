
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

        public NdiVideoSink(NDI.Sender ndiSender, int videoWidth, int videoHeight, VideoFormat format)
            : base(format)
        {
            Initialize(ndiSender, videoWidth, videoHeight);
        }

        private void Initialize(NDI.Sender ndiSender, int videoWidth, int videoHeight)
        {
            NdiSender = ndiSender;
            NdiVideoFrame = new NDI.VideoFrame(videoWidth, videoHeight, (float)videoWidth / videoHeight, 30000, 1001);
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
                Marshal.Copy(buffer.Data, 0, NdiVideoFrame.BufferPtr, buffer.Length);
           
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
