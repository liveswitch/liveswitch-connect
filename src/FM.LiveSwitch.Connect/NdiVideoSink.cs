
using NewTek;
using NDI = NewTek.NDI;
using System;
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

        protected NDI.Sender _NdiSender { get; private set; }
        protected NDI.VideoFrame _NdiVideoFrame { get; private set; }
        protected bool _IsCheckConnectionCount { get; private set; }

        private int _RequestedWidth = -1;
        private int _RequestedHeight = -1;

        private int _Width = -1;
        private int _Height = -1;

        private int _Stride = -1;

        private readonly int _FrameRateNumerator = 30000;
        private readonly int _FrameRateDenominator = 1000;

        private VideoBuffer _VideoBuffer;
        private IntPtr _VideoBufferPtr = IntPtr.Zero;
        private bool _VideoBufferAllocated = false;

        public NdiVideoSink(NDI.Sender ndiSender, int requestedWidth, int requestedHeight, int frameRateNumerator, int frameRateDenominator, VideoFormat format)
            : base(format)
        {
            _FrameRateNumerator = frameRateNumerator;
            _FrameRateDenominator = frameRateDenominator;
            _RequestedHeight = requestedHeight;
            _RequestedWidth = requestedWidth;
            _NdiSender = ndiSender;
            _VideoBuffer = VideoBuffer.CreateBlack(requestedWidth, requestedHeight, VideoFormat.I420Name);
        }

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (_IsCheckConnectionCount && _NdiSender.GetConnections(10000) < 1)
            {
                _Log.Info(Id, "No NDI connections, not rendering audio frame");
                return;
            }
            else
            {
                _IsCheckConnectionCount = false;
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
            _Stride = _VideoBuffer.Stride;
            _Width = _VideoBuffer.Width;
            _Height = _VideoBuffer.Height;

            int bufferSize = _Height * _Stride * 3 / 2;

            if (_VideoBufferAllocated)
            {
                Marshal.FreeHGlobal(_VideoBufferPtr);
            }

            _VideoBufferPtr = Marshal.AllocHGlobal(bufferSize);
            _VideoBufferAllocated = true;

            _NdiVideoFrame = new NDI.VideoFrame(
                _VideoBufferPtr,
                _RequestedWidth,
                _RequestedHeight,
                _Stride,
                NDIlib.FourCC_type_e.NDIlib_FourCC_video_type_I420,
                (float)_Width / _Height,
                _FrameRateNumerator,
                _FrameRateDenominator,
                NDIlib.frame_format_type_e.frame_format_type_progressive);
        }

        protected virtual bool WriteFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {

            try
            {

                if (_NdiVideoFrame == null)
                {
                    initializeNdiFrame();
                }

                var output = _VideoBuffer.DataBuffer.Data;
                var stride = inputBuffer.Stride;
                var outputStride = _VideoBuffer.Stride;

                var yCopyLength = Math.Min(Math.Min(outputStride, stride), inputBuffer.Width);
                var uvCopyLength = Math.Min(Math.Min(outputStride/2, stride/2), inputBuffer.Width/2);

                var inputUOffset = inputBuffer.Height * stride;
                var inputULength = inputBuffer.Height / 2 * stride / 2;
                var inputVOffset = inputUOffset + inputULength;
                var inputVLength = inputULength;

                var outputUOffset = _VideoBuffer.Height * outputStride;
                var outputULength = _VideoBuffer.Height / 2 * outputStride / 2;
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

                    Marshal.Copy(output, 0, _NdiVideoFrame.BufferPtr, output.Length);
                    _NdiSender.Send(_NdiVideoFrame);
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
            if (_VideoBufferAllocated)
            {
                Marshal.FreeHGlobal(_VideoBufferPtr);
                _VideoBufferPtr = IntPtr.Zero;
            }
            if (_NdiVideoFrame != null)
            {
                _NdiVideoFrame.Dispose();
            }            

        }
    }
}
