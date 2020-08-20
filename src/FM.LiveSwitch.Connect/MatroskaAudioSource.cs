namespace FM.LiveSwitch.Connect
{
    class MatroskaAudioSource : Matroska.AudioSource
    {
        public MatroskaAudioSource(string path)
            : base(path)
        { }

        protected override AudioDecoder CreateOpusDecoder(AudioConfig config)
        {
            return new Opus.Decoder(config);
        }
    }
}
