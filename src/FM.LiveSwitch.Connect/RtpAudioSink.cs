namespace FM.LiveSwitch.Connect
{
    class RtpAudioSink : AudioSink
    {
        public override string Label
        {
            get { return "RTP Audio Sink"; }
        }

        public string IPAddress
        {
            get { return _Writer.IPAddress; }
            set { _Writer.IPAddress = value; }
        }

        public int Port
        {
            get { return _Writer.Port; }
            set { _Writer.Port = value; }
        }

        public int PayloadType { get; set; }

        public long SynchronizationSource { get; set; }

        private RtpWriter _Writer = null;
        private long _SequenceNumber;

        public RtpAudioSink(AudioFormat format)
            : base(format)
        {
            _Writer = new RtpWriter(format.ClockRate);
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            var rtpHeaders = inputBuffer.RtpHeaders;
            foreach (var rtpHeader in rtpHeaders)
            {
                if (rtpHeader.SequenceNumber == -1)
                {
                    rtpHeader.SequenceNumber = (int)(_SequenceNumber++ % (ushort.MaxValue + 1));
                }
                if (rtpHeader.Timestamp == -1)
                {
                    rtpHeader.Timestamp = frame.Timestamp % ((long)uint.MaxValue + 1);
                }
            }

            var rtpPayloads = inputBuffer.DataBuffers;
            for (var i = 0; i < rtpHeaders.Length; i++)
            {
                _Writer.Write(new RtpPacket(rtpPayloads[i], rtpHeaders[i].SequenceNumber, rtpHeaders[i].Timestamp, rtpHeaders[i].Marker)
                {
                    PayloadType = PayloadType,
                    SynchronizationSource = SynchronizationSource
                });
            }
        }

        protected override void DoDestroy()
        {
            _Writer.Destroy();
        }
    }
}
