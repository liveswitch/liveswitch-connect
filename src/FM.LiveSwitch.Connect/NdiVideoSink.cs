
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

        private int requestedWidth = -1;

        private int requestedHeight = -1;

        private VideoBuffer videoBuffer;

        private readonly int frameRateNumerator = 30000;

        private readonly int frameRateDenominator = 1000;

        private IntPtr videoBufferPtr = IntPtr.Zero;

        private bool videoBufferAllocated = false;

        public NdiVideoSink(NDI.Sender ndiSender, int requestedWidth, int requestedHeight, int frameRateNumerator, int frameRateDenominator, VideoFormat format)
            : base(format)
        {
            this.frameRateNumerator = frameRateNumerator;
            this.frameRateDenominator = frameRateDenominator;
            this.requestedHeight = requestedHeight;
            this.requestedWidth = requestedWidth;
            this.NdiSender = ndiSender;
            this.videoBuffer = VideoBuffer.CreateBlack(requestedWidth, requestedHeight, VideoFormat.I420Name);
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

        private void initializeNdiFrame()
        {
            stride = videoBuffer.Stride;
            width = videoBuffer.Width;
            height = videoBuffer.Height;

            int bufferSize = height * stride * 3 / 2;

            if (videoBufferAllocated)
            {
                Marshal.FreeHGlobal(videoBufferPtr);
            }

            videoBufferPtr = Marshal.AllocHGlobal(bufferSize);
            videoBufferAllocated = true;

            NdiVideoFrame = new NDI.VideoFrame(
                videoBufferPtr,
                requestedWidth,
                requestedHeight,
                stride,
                NDIlib.FourCC_type_e.NDIlib_FourCC_video_type_I420,
                (float)width / height,
                frameRateNumerator,
                frameRateDenominator,
                NDIlib.frame_format_type_e.frame_format_type_progressive);
        }

        protected virtual bool WriteFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {

            try
            {

                if (NdiVideoFrame == null)
                {
                    initializeNdiFrame();
                }

                var output = videoBuffer.DataBuffer.Data;
                var stride = inputBuffer.Stride;
                var outputStride = videoBuffer.Stride;

                var yCopyLength = Math.Min(Math.Min(outputStride, stride), inputBuffer.Width);
                var uvCopyLength = Math.Min(Math.Min(outputStride/2, stride/2), inputBuffer.Width/2);

                var inputUOffset = inputBuffer.Height * stride;
                var inputULength = inputBuffer.Height / 2 * stride / 2;
                var inputVOffset = inputUOffset + inputULength;
                var inputVLength = inputULength;

                var outputUOffset = videoBuffer.Height * outputStride;
                var outputULength = videoBuffer.Height / 2 * outputStride / 2;
                var outputVOffset = outputUOffset + outputULength;

                foreach (var dataBuffer in inputBuffer.DataBuffers)
                {
                    var input = dataBuffer.Data;
                    
                    for (var i = 0; i < inputBuffer.Height; i++)
                    {
                        int startPosition = inputBuffer.Stride * i;
                        int outPosition = outputStride * i;

                        Buffer.BlockCopy(input, startPosition, output, outPosition, yCopyLength);
                    }

                    for (var i = 0; i < inputBuffer.Height/2; i++) {
                        int startPosition = inputUOffset + (i * stride/2);
                        int outPosition = outputUOffset + (i * outputStride/2);

                        Buffer.BlockCopy(input, startPosition, output, outPosition, uvCopyLength);
                    }

                    for (var i = 0; i < inputBuffer.Height / 2; i++)
                    {
                        int startPosition = inputVOffset + (i * stride/2);
                        int outPosition = outputVOffset + (i * outputStride/2);

                        Buffer.BlockCopy(input, startPosition, output, outPosition, uvCopyLength);
                    }

                    Marshal.Copy(output, 0, NdiVideoFrame.BufferPtr, output.Length);
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
