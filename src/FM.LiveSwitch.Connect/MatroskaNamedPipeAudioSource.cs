using System;

namespace FM.LiveSwitch.Connect
{
    class MatroskaNamedPipeAudioSource : NamedPipeAudioSource
    {
        public override string Label
        {
            get { return "Matroska Named Pipe Audio Source"; }
        }

        private MatroskaReader _Reader;

        public MatroskaNamedPipeAudioSource(string pipeName, AudioFormat format)
            : base(pipeName, format, true)
        {
            StartAsync = true;
        }

        protected override void ReadStreamHeader()
        {
            _Reader = new MatroskaReader(Pipe.Stream);
            _Reader.ReadStreamHeader();
        }

        protected override int ReadFrameHeader()
        {
            return _Reader.ReadFrameHeader();
        }

        protected override void RaiseFramePayload(DataBuffer dataBuffer)
        {
            Console.Error.WriteLine($"Raising frame of size {dataBuffer.Length}");
            RaiseFrame(new AudioFrame(FrameDuration, new AudioBuffer(dataBuffer, OutputFormat))
            {
                Timestamp = _Reader.WaitForFrameTimestamp(OutputFormat.ClockRate)
            });
        }
    }
}
