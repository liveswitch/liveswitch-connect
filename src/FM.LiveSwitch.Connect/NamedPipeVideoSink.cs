namespace FM.LiveSwitch.Connect
{
    class NamedPipeVideoSink : VideoSink
    {
        public override string Label
        {
            get { return "Named Pipe Video Sink"; }
        }

        public string PipeName { get; private set; }

        public bool Client { get; private set; }

        public event Action0 OnPipeConnected;

        private readonly NamedPipe _Pipe;

        public NamedPipeVideoSink(string pipeName)
            : this(pipeName, false)
        { }

        public NamedPipeVideoSink(string pipeName, bool client)
            : base()
        {
            PipeName = pipeName;
            Client = client;

            Deactivated = true;

            _Pipe = new NamedPipe(pipeName, !client);
            _Pipe.OnConnected += () =>
            {
                Deactivated = false;
                OnPipeConnected?.Invoke();
            };

            if (client)
            {
                _Pipe.Connect();
            }
            else
            {
                _ = _Pipe.TryAccept();
            }
        }

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (_Pipe.IsConnected)
            {
                foreach (var dataBuffer in inputBuffer.DataBuffers)
                {
                    _Pipe.TryWrite(dataBuffer);
                }
            }
        }

        protected override void DoDestroy()
        {
            _Pipe.Destroy();
        }
    }
}
