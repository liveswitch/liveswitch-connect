namespace FM.LiveSwitch.Connect
{
    class RtpAudioSource : AudioSource
    {
        public override string Label
        {
            get { return "RTP Audio Source"; }
        }

        public int Port { get { return _Reader.Port; } }

        private readonly RtpReader _Reader;
        private RolloverContext _SequenceNumberRolloverContext;
        private RolloverContext _TimestampRolloverContext;

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
            _SequenceNumberRolloverContext = new RolloverContext(16);
            _TimestampRolloverContext = new RolloverContext(32);

            _Reader.OnPacket += (packet) =>
            {
                packet.Payload.LittleEndian = OutputFormat.LittleEndian;

                RaisePacket(packet);
            };
        }

        private void RaisePacket(RtpPacket packet)
        {
            RaiseFrame(new AudioFrame(-1, new PacketizedAudioBuffer(packet.Payload, OutputFormat, new RtpPacketHeader
            {
                Marker = packet.Marker
            }))
            {
                SequenceNumber = _SequenceNumberRolloverContext.GetIndex(packet.SequenceNumber),
                Timestamp = _TimestampRolloverContext.GetIndex(packet.Timestamp)
            });
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
