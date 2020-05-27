namespace FM.LiveSwitch.Connect
{
    class NamedPipeVideoSink : VideoSink
    {
        static ILog _Log = Log.GetLogger(typeof(NamedPipeVideoSink));

        public override string Label
        {
            get { return "Named Pipe Video Sink"; }
        }

        public string PipeName { get; private set; }

        public bool Client { get; private set; }

        public event Action0 OnPipeConnected;

        protected NamedPipe Pipe { get; private set; }

        private volatile bool _StreamHeaderSent = false;

        public NamedPipeVideoSink(string pipeName)
            : this(pipeName, false)
        { }

        public NamedPipeVideoSink(string pipeName, bool client)
            : base()
        {
            Initialize(pipeName, client);
        }

        public NamedPipeVideoSink(string pipeName, bool client, VideoFormat format)
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

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (Pipe.IsConnected)
            {
                if (!_StreamHeaderSent)
                {
                    if (!(_StreamHeaderSent = WriteStreamHeader(frame, inputBuffer)))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send video stream header.");
                        }
                    }
                }

                if (_StreamHeaderSent)
                {
                    if (!WriteFrameHeader(frame, inputBuffer))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send video frame header.");
                        }
                    }
                    else
                    {
                        if (!WriteFrame(frame, inputBuffer))
                        {
                            if (!Deactivated)
                            {
                                _Log.Error(Id, "Could not send video frame.");
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool WriteStreamHeader(VideoFrame frame, VideoBuffer inputBuffer)
        {
            return true;
        }

        protected virtual bool WriteFrameHeader(VideoFrame frame, VideoBuffer inputBuffer)
        {
            return true;
        }

        protected virtual bool WriteFrame(VideoFrame frame, VideoBuffer inputBuffer)
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
