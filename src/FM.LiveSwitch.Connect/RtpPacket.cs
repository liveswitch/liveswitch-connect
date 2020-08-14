namespace FM.LiveSwitch.Connect
{
    class RtpPacket
    {
        public DataBuffer Payload { get; set; }
        public long SequenceNumber { get; set; }
        public long Timestamp { get; set; }
        public bool Marker { get; set; }

        public RtpPacket(DataBuffer payload, long sequenceNumber, long timestamp, bool marker)
        {
            Payload = payload;
            SequenceNumber = sequenceNumber;
            Timestamp = timestamp;
            Marker = marker;
        }
    }
}
