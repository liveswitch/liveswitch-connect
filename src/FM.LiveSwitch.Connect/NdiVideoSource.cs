
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

        protected NDI.Receiver _NdiReceiver { get; private set; }

        private VideoBuffer _Buffer;

        private bool _BufferAllocated = false;
        private int _AllocatedPixels;
        private readonly int _FourCC;

        public NdiVideoSource(NDI.Receiver ndiReceiver, ImageFormat format)
            : base(ImageFormatExtensions.CreateFormat(format))
        {
            _NdiReceiver = ndiReceiver;
            _FourCC = OutputFormat.FourCC;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            if (_NdiReceiver != null)
            {
                _NdiReceiver.VideoFrameReceived += ProcessFrameReceived;
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
            if (_NdiReceiver != null)
            {
                _NdiReceiver.VideoFrameReceived -= ProcessFrameReceived;

                if (_BufferAllocated)
                {
                    _Buffer.Free();
                    _BufferAllocated = false;
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
            _AllocatedPixels = width * height;
            _Buffer = VideoBuffer.CreateBlack(width, height, OutputFormat.Name);
            _BufferAllocated = true;
        }

        protected void ProcessFrameReceived(object sender, NDI.VideoFrameReceivedEventArgs e)
        {
            if (!_BufferAllocated)
            {
                if (_FourCC != e.Frame.FourCC)
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
            else if (e.Frame.Width != _Buffer.Width || e.Frame.Height != _Buffer.Height)
            {
                _Log.Debug($"Video size changed to {e.Frame.Width}x{e.Frame.Height}");
                if (e.Frame.Width * e.Frame.Height > _AllocatedPixels)
                {
                    // Only reallocate if pixel count increased
                    _Buffer.Free();
                    _BufferAllocated = false;
                    _Log.Debug($"Re-creating video buffer");
                    AllocateBuffer(e.Frame.Width, e.Frame.Height);
                }
                else
                {
                    _Buffer.Width = e.Frame.Width;
                    _Buffer.Height = e.Frame.Height;
                    _Buffer.Stride = e.Frame.Stride;
                }
            }

            // Populate buffer with NDI frame data
            Marshal.Copy(e.Frame.BufferPtr, _Buffer.DataBuffer.Data, 0, e.Frame.Height * e.Frame.Stride);

            RaiseFrame(new VideoFrame(_Buffer));
        }
    }
}
