using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class NamedPipeAudioSource : AudioSource
    {
        public override string Label
        {
            get { return "Named Pipe Audio Source"; }
        }

        public string PipeName { get; private set; }

        public bool Server { get; private set; }

        public bool StartAsync { get; set; }

        public event Action0 OnPipeConnected;

        protected NamedPipe Pipe { get; private set; }

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat)
            : this(pipeName, outputFormat, 20)
        { }

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat, int frameDuration)
            : this(pipeName, outputFormat, frameDuration, false)
        { }

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat, bool server)
            : this(pipeName, outputFormat, 20, server)
        { }

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat, int frameDuration, bool server)
            : base(outputFormat, frameDuration)
        {
            PipeName = pipeName;
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
                    RaiseFramePayload(dataBuffer);
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
            return SoundUtility.CalculateDataLength(FrameDuration, Config);
        }

        protected virtual void RaiseFramePayload(DataBuffer dataBuffer)
        {
            RaiseFrame(new AudioFrame(FrameDuration, new AudioBuffer(dataBuffer, OutputFormat)));
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
