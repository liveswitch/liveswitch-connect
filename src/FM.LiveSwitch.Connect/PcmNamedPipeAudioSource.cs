namespace FM.LiveSwitch.Connect
{
    class PcmNamedPipeAudioSource : NamedPipeAudioSource
    {
        public override string Label
        {
            get { return "PCM Named Pipe Audio Source"; }
        }

        public PcmNamedPipeAudioSource(string pipeName)
            : base(pipeName, new Pcm.Format(Opus.Format.DefaultConfig), true)
        {
            StartAsync = true;
        }
    }
}
