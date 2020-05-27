namespace FM.LiveSwitch.Connect
{
    class RtpAudioSource : AudioSource
    {
        public override string Label
        {
            get { return "RTP Audio Source"; }
        }

        public int Port { get { return _Reader.Port; } }

        private RtpReader _Reader = null;

        public RtpAudioSource(AudioFormat format)
            : base(format)
        {
            _Reader = new RtpReader(format.ClockRate);

            Initialize();
        }

        public RtpAudioSource(AudioFormat format, int port)
            : base(format)
        {
            _Reader = new RtpReader(format.ClockRate, port);

            Initialize();
        }

        private void Initialize()
        {
            _Reader.OnPacket += (payload, sequenceNumber, timestamp, marker) =>
            {
                RaiseFrame(new AudioFrame(-1, new PacketizedAudioBuffer(payload, OutputFormat, new RtpPacketHeader()))
                {
                    SequenceNumber = sequenceNumber,
                    Timestamp = timestamp
                });
            };
        }

        protected override Future<object> DoStart()
        {
            return _Reader.Start();
        }

        protected override Future<object> DoStop()
        {
            return _Reader.Stop();
        }

        protected override void DoDestroy()
        {
            _Reader.Destroy();

            base.DoDestroy();
        }
    }
}
