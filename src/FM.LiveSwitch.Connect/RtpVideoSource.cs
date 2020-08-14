using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    class RtpVideoSource : VideoSource
    {
        public override string Label
        {
            get { return "RTP Video Source"; }
        }

        public int Port { get { return _Reader.Port; } }

        public bool NeedsParameterSets { get; set; }

        public DataBuffer[] ParameterSets { get; set; }

        private RtpReader _Reader = null;
        private List<RtpPacket> _Queue;

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
            _Queue = new List<RtpPacket>();

            _Reader.OnPacket += (packet) =>
            {
                if (NeedsParameterSets)
                {
                    _Queue.Add(packet);

                    if (ParameterSets != null)
                    {
                        for (var i = ParameterSets.Length - 1; i >= 0; i--)
                        {
                            _Queue.Insert(0, new RtpPacket(ParameterSets[i], _Queue[0].SequenceNumber - 1, _Queue[0].Timestamp, false));
                        }

                        NeedsParameterSets = false;
                        ParameterSets = null;

                        _Queue.ForEach(RaisePacket);
                        _Queue.Clear();
                    }
                }
                else
                {
                    RaisePacket(packet);
                }
            };
        }

        private void RaisePacket(RtpPacket packet)
        {
            RaiseFrame(new VideoFrame(new PacketizedVideoBuffer(-1, -1, packet.Payload, OutputFormat, new RtpPacketHeader
            {
                Marker = packet.Marker
            }))
            {
                SequenceNumber = packet.SequenceNumber,
                Timestamp = packet.Timestamp
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
