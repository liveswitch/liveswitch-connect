namespace FM.LiveSwitch.Connect
{
    class FFAudioSink : NamedPipeAudioSink
    {
        public override string Label
        {
            get { return "FFmpeg Audio Sink"; }
        }

        public FFAudioSink(string pipeName)
            : base(pipeName)
        { }
    }
}
