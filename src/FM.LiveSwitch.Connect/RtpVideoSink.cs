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

        public bool WaitForKeyFrame { get; set; }

        private RtpWriter _Writer = null;
        private int _KeyFrameIntervalCounter;
        private long _LastTimestamp;
        private long _SequenceNumber;
        private volatile bool _Ready;

        public RtpVideoSink(VideoFormat format)
            : base(format)
        {
            _Writer = new RtpWriter(format.ClockRate);
        }

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (frame.Timestamp != _LastTimestamp)
            {
                _KeyFrameIntervalCounter++;
                _LastTimestamp = frame.Timestamp;
            }

            if (_KeyFrameIntervalCounter == KeyFrameInterval)
            {
                Console.Error.WriteLine("Raising keyframe request.");
                RaiseControlFrame(new FirControlFrame(new FirEntry(GetCcmSequenceNumber())));
                _KeyFrameIntervalCounter = 0;
            }

            if (!WaitForKeyFrame)
            {
                _Ready = true;
            }

            if (!_Ready)
            {
                // clone to check keyframe since H.264 keyframe test on 
                // packetized data buffers is destructive in 1.9.x
                var inputBufferClone = inputBuffer.Clone();
                var dataBufferClones = new DataBuffer[inputBuffer.DataBuffers.Length];
                for (var i = 0; i < dataBufferClones.Length; i++)
                {
                    dataBufferClones[i] = DataBuffer.Wrap(inputBuffer.DataBuffers[i].ToArray());
                }
                inputBufferClone.DataBuffers = dataBufferClones;
                if (inputBufferClone.IsKeyFrame)
                {
                    _Ready = true;
                }
            }

            if (_Ready)
            {
                var rtpHeaders = inputBuffer.RtpHeaders;
                var rtpPayloads = inputBuffer.DataBuffers;
                for (var i = 0; i < rtpHeaders.Length; i++)
                {
                    var rtpHeader = rtpHeaders[i];
                    if (rtpHeader == null)
                    {
                        rtpHeader = new RtpPacketHeader();
                    }
                    if (rtpHeader.SequenceNumber == -1)
                    {
                        rtpHeader.SequenceNumber = (int)(_SequenceNumber++ % (ushort.MaxValue + 1));
                    }
                    if (rtpHeader.Timestamp == -1)
                    {
                        rtpHeader.Timestamp = frame.Timestamp % ((long)uint.MaxValue + 1);
                    }
                    _Writer.Write(new RtpPacket(rtpPayloads[i], rtpHeader.SequenceNumber, rtpHeader.Timestamp, rtpHeader.Marker)
                    {
                        PayloadType = PayloadType,
                        SynchronizationSource = SynchronizationSource
                    });
                }
            }
        }

        protected override void DoDestroy()
        {
            _Writer.Destroy();
        }
    }
}
