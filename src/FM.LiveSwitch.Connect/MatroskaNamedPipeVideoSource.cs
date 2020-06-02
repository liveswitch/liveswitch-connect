namespace FM.LiveSwitch.Connect
{
    class MatroskaNamedPipeVideoSource : NamedPipeVideoSource
    {
        public override string Label
        {
            get { return "Matroska Named Pipe Video Source"; }
        }

        private MatroskaReader _Reader;

        public MatroskaNamedPipeVideoSource(string pipeName, VideoFormat format)
            : base(pipeName, 0, 0, format, true)
        {
            StartAsync = true;
        }

        protected override void ReadStreamHeader()
        {
            Width = -1;
            Height = -1;

            _Reader = new MatroskaReader(Pipe.Stream);
            _Reader.ReadStreamHeader();
        }

        protected override int ReadFrameHeader()
        {
            return _Reader.ReadFrameHeader();
        }

        protected override void RaiseFramePayload(DataBuffer dataBuffer)
        {
            RaiseFrame(new VideoFrame(new VideoBuffer(Width, Height, dataBuffer, OutputFormat))
            {
                Timestamp = _Reader.WaitForFrameTimestamp(OutputFormat.ClockRate)
            });
        }
    }
}
