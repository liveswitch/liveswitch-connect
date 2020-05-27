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

        public bool StartAsync { get; set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public event Action0 OnPipeConnected;

        protected NamedPipe Pipe { get; private set; }

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
                Pipe = new NamedPipe(PipeName, Server);
                Pipe.OnConnected += () =>
                {
                    OnPipeConnected?.Invoke();
                };
                Pipe.OnReadDataBuffer += (dataBuffer) =>
                {
                    RaiseFrame(new VideoFrame(new VideoBuffer(Width, Height, dataBuffer, OutputFormat)));
                };

                if (StartAsync)
                {
                    Task ready;
                    if (Server)
                    {
                        ready = Pipe.WaitForConnectionAsync();
                    }
                    else
                    {
                        ready = Pipe.ConnectAsync();
                    }

                    Task.Run(async () =>
                    {
                        await ready;

                        ReadStreamHeader();

                        Pipe.StartReading(ReadFrameHeader);
                    });

                    promise.Resolve(null);
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            if (Server)
                            {
                                await Pipe.WaitForConnectionAsync();
                            }
                            else
                            {
                                await Pipe.ConnectAsync();
                            }

                            ReadStreamHeader();

                            Pipe.StartReading(ReadFrameHeader);

                            promise.Resolve(null);
                        }
                        catch (Exception ex)
                        {
                            promise.Reject(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return promise;
        }

        protected virtual void ReadStreamHeader() { }

        protected virtual int ReadFrameHeader()
        {
            return VideoBuffer.GetMinimumBufferLength(Width, Height, OutputFormat.Name);
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
                        await Pipe.StopReading();
                        await Pipe.DestroyAsync();

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
