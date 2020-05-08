using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFAudioSource : AudioSource
    {
        public override string Label
        {
            get { return "FFmpeg Audio Source"; }
        }

        public string PipeName { get; private set; }

        public event Action0 OnPipeConnected;

        private NamedPipe _Pipe;

        public FFAudioSource(string pipeName)
            : base(new Pcm.Format(Opus.Format.DefaultConfig))
        {
            PipeName = pipeName;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            try
            {
                var frameLength = SoundUtility.CalculateDataLength(FrameDuration, Config);

                _Pipe = new NamedPipe(PipeName, true);
                _Pipe.OnConnected += () =>
                {
                    OnPipeConnected?.Invoke();
                };
                _Pipe.OnReadDataBuffer += (dataBuffer) =>
                {
                    RaiseFrame(new AudioFrame(FrameDuration, new AudioBuffer(dataBuffer, OutputFormat)));
                };

                var ready = _Pipe.WaitForConnectionAsync();

                Task.Run(async () =>
                {
                    await ready;

                    _Pipe.StartReading(frameLength);
                });

                promise.Resolve(null);
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
