namespace FM.LiveSwitch.Connect
{
    class FFAudioSink : AudioSink
    {
        static ILog _Log = Log.GetLogger(typeof(FFAudioSink));

        public override string Label
        {
            get { return "FFmpeg Audio Sink"; }
        }

        public string PipeName { get; private set; }

        public event Action0 OnPipeConnected;

        private readonly NamedPipe _Pipe;

        public FFAudioSink(string pipeName)
            : base()
        {
            PipeName = pipeName;

            Deactivated = true;

            _Pipe = new NamedPipe(pipeName, true);
            _Pipe.OnConnected += () =>
            {
                Deactivated = false;
                OnPipeConnected?.Invoke();
            };

            _ = _Pipe.TryAccept();
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            if (_Pipe.IsConnected)
            {
                foreach (var dataBuffer in inputBuffer.DataBuffers)
                {
                    if (!_Pipe.TryWrite(dataBuffer))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error("Could not send audio frame.");
                        }
                    }
                }
            }
        }

        protected override void DoDestroy()
        {
            _Pipe.Destroy();
        }
    }
}
