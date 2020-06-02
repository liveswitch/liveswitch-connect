namespace FM.LiveSwitch.Connect
{
    class RtpVideoSource : VideoSource
    {
        public override string Label
        {
            get { return "RTP Video Source"; }
        }

        public int Port { get { return _Reader.Port; } }

        private RtpReader _Reader = null;

        public RtpVideoSource(VideoFormat format)
            : base(format)
        {
            _Reader = new RtpReader(format.ClockRate);

            Initialize();
        }

        public RtpVideoSource(VideoFormat format, int port)
            : base(format)
        {
            _Reader = new RtpReader(format.ClockRate, port);

            Initialize();
        }

        private void Initialize()
        {
            _Reader.OnPacket += (payload, sequenceNumber, timestamp, marker) =>
            {
                RaiseFrame(new VideoFrame(new PacketizedVideoBuffer(-1, -1, payload, OutputFormat, new RtpPacketHeader
                {
                    Marker = marker
                }))
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
