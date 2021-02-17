
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
                if (stride != inputBuffer.Stride || NdiVideoFrame == null)
                {
                    stride = inputBuffer.Stride;

                    int width = inputBuffer.Stride; //This has to be stride! - The buffer may be returned with a stride that does not match up with the width.
                                                    // For example - when returning video at 1918x924 the stride ends up being 1984
                                                    // If 1918x924 is passed to ndi with a stride of 1984 it will expect a different buffer size. 

                    int height = inputBuffer.Height;
                    int bufferSize = (width * height) + 2 * ((width / 2) * (height / 2));

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
