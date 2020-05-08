namespace FM.LiveSwitch.Connect
{
    class MatroskaVideoSource : Matroska.VideoSource
    {
        public MatroskaVideoSource(string path)
            : base(path)
        { }

        protected override VideoDecoder CreateVp8Decoder()
        {
            return new Vp8.Decoder();
        }

        protected override VideoDecoder CreateVp9Decoder()
        {
            return new Vp9.Decoder();
        }

        protected override VideoDecoder CreateH264Decoder()
        {
            return new OpenH264.Decoder();
        }
    }
}
