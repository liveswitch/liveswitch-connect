namespace FM.LiveSwitch.Connect
{
    class PcmNamedPipeAudioSink : NamedPipeAudioSink
    {
        public override string Label
        {
            get { return "PCM Named Pipe Audio Sink"; }
        }

        public PcmNamedPipeAudioSink(string pipeName)
            : base(pipeName)
        { }
    }
}
