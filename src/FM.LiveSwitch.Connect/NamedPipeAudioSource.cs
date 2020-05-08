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

        public event Action0 OnPipeConnected;

        private NamedPipe _Pipe;

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat)
            : this(pipeName, outputFormat, 20)
        { }

        public NamedPipeAudioSource(string pipeName, AudioFormat outputFormat, int frameDuration)
            : this(pipeName, outputFormat, frameDuration, false)
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
                var frameLength = SoundUtility.CalculateDataLength(FrameDuration, Config);

                _Pipe = new NamedPipe(PipeName, Server);
                _Pipe.OnConnected += () =>
                {
                    OnPipeConnected?.Invoke();
                };
                _Pipe.OnReadDataBuffer += (dataBuffer) =>
                {
                    RaiseFrame(new AudioFrame(FrameDuration, new AudioBuffer(dataBuffer, OutputFormat)));
                };

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
