
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

        private int stride = -1;

        private int width = -1;

        private int height = -1;

        private readonly int frameRateNumerator = 30000;

        private readonly int frameRateDenominator = 1000;

        private IntPtr videoBufferPtr = IntPtr.Zero;

        private bool videoBufferAllocated = false;

        public NdiVideoSink(NDI.Sender ndiSender, int frameRateNumerator, int frameRateDenominator, VideoFormat format)
            : base(format)
        {
            this.frameRateNumerator = frameRateNumerator;
            this.frameRateDenominator = frameRateDenominator;
            this.NdiSender = ndiSender;
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

            try
            {
                if (stride != inputBuffer.Stride || width != inputBuffer.Width || height != inputBuffer.Height || NdiVideoFrame == null)
                {
                    stride = inputBuffer.Stride;
                    width = inputBuffer.Width; 
                    height = inputBuffer.Height;

                    int bufferSize = height * stride * 3/2;

                    if (videoBufferAllocated)
                    {
                        Marshal.FreeHGlobal(videoBufferPtr);
                    }

                    videoBufferPtr = Marshal.AllocHGlobal(bufferSize);
                    videoBufferAllocated = true;

                    NdiVideoFrame = new NDI.VideoFrame(
                        videoBufferPtr, 
                        width, 
                        height, 
                        stride, 
                        NDIlib.FourCC_type_e.NDIlib_FourCC_video_type_I420, 
                        (float)width / height, 
                        frameRateNumerator, 
                        frameRateDenominator,
                        NDIlib.frame_format_type_e.frame_format_type_progressive);
                }

                foreach(var dataBuffer in inputBuffer.DataBuffers)
                {
                    Marshal.Copy(dataBuffer.Data, 0, NdiVideoFrame.BufferPtr, dataBuffer.Data.Length);
                    NdiSender.Send(NdiVideoFrame);
                }
                
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
            if (videoBufferAllocated)
            {
                Marshal.FreeHGlobal(videoBufferPtr);
                videoBufferPtr = IntPtr.Zero;
            }
            NdiVideoFrame.Dispose();
            

        }
    }
}
