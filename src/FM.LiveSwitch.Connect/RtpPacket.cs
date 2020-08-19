namespace FM.LiveSwitch.Connect
{
    class RtpPacket
    {
        public DataBuffer Payload { get; set; }
        public int SequenceNumber { get; set; }
        public long Timestamp { get; set; }
        public bool Marker { get; set; }
        public int PayloadType { get; set; }
        public long SynchronizationSource { get; set; }

        public RtpPacket(DataBuffer payload, int sequenceNumber, long timestamp, bool marker)
        {
            Payload = payload;
            SequenceNumber = sequenceNumber;
            Timestamp = timestamp;
            Marker = marker;
        }
    }
}
