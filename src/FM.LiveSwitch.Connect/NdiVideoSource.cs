
using NDI = NewTek.NDI;
using System;
using System.Runtime.InteropServices;

namespace FM.LiveSwitch.Connect
{

    class NdiVideoSource : VideoSource
    {
        static readonly ILog _Log = Log.GetLogger(typeof(NdiVideoSource));

        public override string Label
        {
            get { return "Ndi Video Source"; }
        }

        protected NDI.Receiver NdiReceiver { get; private set; }

        private VideoBuffer buffer;

        private bool bufferAllocated = false;
        private int allocatedPixels;
        private readonly int fourCC;

        public NdiVideoSource(NDI.Receiver ndiReceiver, ImageFormat format)
            : base(ImageFormatExtensions.CreateFormat(format))
        {
            NdiReceiver = ndiReceiver;
            fourCC = OutputFormat.FourCC;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            if (NdiReceiver != null)
            {
                NdiReceiver.VideoFrameReceived += ProcessFrameReceived;
                promise.Resolve(null);
            }
            else
            {
                promise.Reject(new Exception("NDI.Receiver not initialized."));
            }
            return promise;
        }

        protected override Future<object> DoStop()
        {
            var promise = new Promise<object>();
            if (NdiReceiver != null)
            {
                NdiReceiver.VideoFrameReceived -= ProcessFrameReceived;

                if (bufferAllocated)
                {
                    buffer.Free();
                    bufferAllocated = false;
                }

                promise.Resolve(null);
            }
            else
            {
                promise.Reject(new Exception("NDI.Receiver already gone."));
            }
            return promise;
        }

        protected void AllocateBuffer(int width, int height)
        {
            allocatedPixels = width * height;
            buffer = VideoBuffer.CreateBlack(width, height, OutputFormat.Name);
            bufferAllocated = true;
        }

        protected void ProcessFrameReceived(object sender, NDI.VideoFrameReceivedEventArgs e)
        {
            if (!bufferAllocated)
            {
                if (fourCC != e.Frame.FourCC)
                {
                    string receivedFormatName = OutputFormat.FourCCToFormatName(e.Frame.FourCC);
                    if (receivedFormatName.Length == 0)
                    {
                        receivedFormatName = e.Frame.FourCC.ToString("X8");
                    }
                    _Log.Error($"Specified format {OutputFormat.Name} doesn't match NDI device format {receivedFormatName}");
                }


                _Log.Debug($"Creating video buffer {e.Frame.Width}x{e.Frame.Height} {OutputFormat.Name}");
                AllocateBuffer(e.Frame.Width, e.Frame.Height);
            }
            else if (e.Frame.Width != buffer.Width || e.Frame.Height != buffer.Height)
            {
                _Log.Debug($"Video size changed to {e.Frame.Width}x{e.Frame.Height}");
                if (e.Frame.Width * e.Frame.Height > allocatedPixels)
                {
                    // Only reallocate if pixel count increased
                    buffer.Free();
                    bufferAllocated = false;
                    _Log.Debug($"Re-creating video buffer");
                    AllocateBuffer(e.Frame.Width, e.Frame.Height);
                }
                else
                {
                    buffer.Width = e.Frame.Width;
                    buffer.Height = e.Frame.Height;
                    buffer.Stride = e.Frame.Stride;
                }
            }

            // Populate buffer with NDI frame data
            Marshal.Copy(e.Frame.BufferPtr, buffer.DataBuffer.Data, 0, e.Frame.Height * e.Frame.Stride);

            RaiseFrame(new VideoFrame(buffer));
        }
    }
}
