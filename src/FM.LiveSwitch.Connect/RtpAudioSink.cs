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

        public RtpAudioSink(AudioFormat format)
            : base(format)
        {
            _Writer = new RtpWriter(format.ClockRate);
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            var rtpHeaders = inputBuffer.RtpHeaders;
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
