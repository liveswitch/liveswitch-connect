namespace FM.LiveSwitch.Connect
{
    class MatroskaVideoSource : Matroska.VideoSource
    {
        protected IConnectionOptions Options { get; private set; }

        public MatroskaVideoSource(string path, IConnectionOptions options)
            : base(path)
        {
            Options = options;
        }

        protected override VideoDecoder CreateVp8Decoder()
        {
            return VideoEncoding.VP8.CreateDecoder(Options);
        }

        protected override VideoDecoder CreateVp9Decoder()
        {
            return VideoEncoding.VP9.CreateDecoder(Options);
        }

        protected override VideoDecoder CreateH264Decoder()
        {
            return VideoEncoding.H264.CreateDecoder(Options);
        }

        protected override VideoDecoder CreateH265Decoder()
        {
            return VideoEncoding.H265.CreateDecoder(Options);
        }
    }
}
