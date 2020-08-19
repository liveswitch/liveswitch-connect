using System;

namespace FM.LiveSwitch.Connect
{
    class RtpVideoSink : VideoSink
    {
        public override string Label
        {
            get { return "RTP Video Sink"; }
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

        public int KeyFrameInterval { get; set; }

        private RtpWriter _Writer = null;

        public RtpVideoSink(VideoFormat format)
            : base(format)
        {
            _Writer = new RtpWriter(format.ClockRate);
        }

        private int KeyFrameIntervalCounter;
        private long LastTimestamp;

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (frame.Timestamp != LastTimestamp)
            {
                KeyFrameIntervalCounter++;
                LastTimestamp = frame.Timestamp;
            }

            if (KeyFrameIntervalCounter == KeyFrameInterval)
            {
                Console.Error.WriteLine("Raising keyframe request.");
                RaiseControlFrame(new FirControlFrame(new FirEntry(GetCcmSequenceNumber())));
                KeyFrameIntervalCounter = 0;
            }

            var rtpHeaders = inputBuffer.RtpHeaders;
            var rtpPayloads = inputBuffer.DataBuffers;
            for (var i = 0; i < rtpHeaders.Length; i++)
            {
                _Writer.Write(new RtpPacket(rtpPayloads[i], frame.SequenceNumber + i, frame.Timestamp, rtpHeaders[i].Marker)
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
