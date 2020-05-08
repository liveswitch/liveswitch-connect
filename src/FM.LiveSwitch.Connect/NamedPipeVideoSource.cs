using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class NamedPipeVideoSource : VideoSource
    {
        public override string Label
        {
            get { return "Named Pipe Video Source"; }
        }

        public string PipeName { get; private set; }

        public bool Server { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public event Action0 OnPipeConnected;

        private NamedPipe _Pipe;

        public NamedPipeVideoSource(string pipeName, int width, int height, VideoFormat outputFormat)
            : this(pipeName, width, height, outputFormat, false)
        { }

        public NamedPipeVideoSource(string pipeName, int width, int height, VideoFormat outputFormat, bool server)
            : base(outputFormat)
        {
            PipeName = pipeName;
            Width = width;
            Height = height;
            Server = server;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            try
            {
                _Pipe = new NamedPipe(PipeName, Server);
                _Pipe.OnConnected += () =>
                {
                    OnPipeConnected?.Invoke();
                };
                _Pipe.OnReadDataBuffer += (dataBuffer) =>
                {
                    RaiseFrame(new VideoFrame(new VideoBuffer(Width, Height, dataBuffer, OutputFormat)));
                };

                var frameLength = VideoBuffer.GetMinimumBufferLength(Width, Height, OutputFormat.Name);

                Task.Run(async () =>
                {
                    try
                    {
                        if (Server)
                        {
                            await _Pipe.WaitForConnectionAsync();
                        }
                        else
                        {
                            await _Pipe.ConnectAsync();
                        }
                        _Pipe.StartReading(frameLength);

                        promise.Resolve(null);
                    }
                    catch (Exception ex)
                    {
                        promise.Reject(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return promise;
        }

        protected override Future<object> DoStop()
        {
            var promise = new Promise<object>();
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _Pipe.StopReading();
                        await _Pipe.DestroyAsync();

                        promise.Resolve(null);
                    }
                    catch (Exception ex)
                    {
                        promise.Reject(ex);
                    }
                });
                promise.Resolve(null);
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return promise;
        }
    }
}
