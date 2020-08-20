namespace FM.LiveSwitch.Connect
{
    class MatroskaAudioSource : Matroska.AudioSource
    {
        protected IConnectionOptions Options { get; private set; }

        public MatroskaAudioSource(string path, IConnectionOptions options)
            : base(path)
        {
            Options = options;
        }

        protected override AudioDecoder CreateOpusDecoder(AudioConfig config)
        {
            return new Opus.Decoder(config);
        }
    }
}
