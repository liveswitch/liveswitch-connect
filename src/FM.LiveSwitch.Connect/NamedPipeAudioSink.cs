namespace FM.LiveSwitch.Connect
{
    class NamedPipeAudioSink : AudioSink
    {
        static ILog _Log = Log.GetLogger(typeof(NamedPipeAudioSink));

        public override string Label
        {
            get { return "Named Pipe Audio Sink"; }
        }

        public string PipeName { get; private set; }

        public bool Client { get; private set; }

        public event Action0 OnPipeConnected;

        protected NamedPipe Pipe { get; private set; }

        private volatile bool _StreamHeaderSent = false;

        public NamedPipeAudioSink(string pipeName)
            : this(pipeName, false)
        { }

        public NamedPipeAudioSink(string pipeName, bool client)
            : base()
        {
            Initialize(pipeName, client);
        }

        public NamedPipeAudioSink(string pipeName, bool client, AudioFormat format)
            : base(format)
        {
            Initialize(pipeName, client);
        }

        private void Initialize(string pipeName, bool client)
        {
            PipeName = pipeName;
            Client = client;

            Deactivated = true;

            Pipe = new NamedPipe(pipeName, !client);
            Pipe.OnConnected += () =>
            {
                Deactivated = false;
                OnPipeConnected?.Invoke();
            };

            if (client)
            {
                Pipe.Connect();
            }
            else
            {
                _ = Pipe.TryAccept();
            }
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            if (Pipe.IsConnected)
            {
                if (!_StreamHeaderSent)
                {
                    if (!(_StreamHeaderSent = WriteStreamHeader(frame, inputBuffer)))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send audio stream header.");
                        }
                    }
                }

                if (_StreamHeaderSent)
                {
                    if (!WriteFrameHeader(frame, inputBuffer))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send audio frame header.");
                        }
                    }
                    else
                    {
                        if (!WriteFrame(frame, inputBuffer))
                        {
                            if (!Deactivated)
                            {
                                _Log.Error(Id, "Could not send audio frame.");
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool WriteStreamHeader(AudioFrame frame, AudioBuffer inputBuffer)
        {
            return true;
        }

        protected virtual bool WriteFrameHeader(AudioFrame frame, AudioBuffer inputBuffer)
        {
            return true;
        }

        protected virtual bool WriteFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            foreach (var dataBuffer in inputBuffer.DataBuffers)
            {
                if (!Pipe.TryWrite(dataBuffer))
                {
                    return false;
                }
            }
            return true;
        }

        protected override void DoDestroy()
        {
            Pipe.Destroy();
        }
    }
}
