namespace FM.LiveSwitch.Connect
{
    class FFAudioSource : NamedPipeAudioSource
    {
        public override string Label
        {
            get { return "FFmpeg Audio Source"; }
        }

        public FFAudioSource(string pipeName)
            : base(pipeName, new Pcm.Format(Opus.Format.DefaultConfig), 20, true)
        {
            StartAsync = true;
        }
    }
}
