namespace FM.LiveSwitch.Connect
{
    class MatroskaVideoSource : Matroska.VideoSource
    {
        private PlayOptions Options;

        public MatroskaVideoSource(PlayOptions options)
            : base(options.VideoPath)
        {
            Options = options;
        }

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
            if ((Options.H264Decoder == H264Decoder.Auto || Options.H264Decoder == H264Decoder.NVDEC) && !Options.DisableNvidia)
            {
                return new Nvidia.H264.Decoder();
            }
            else if ((Options.H264Decoder == H264Decoder.Auto || Options.H264Decoder == H264Decoder.OpenH264) && !Options.DisableOpenH264)
            {
                return new OpenH264.Decoder();
            }
            else
            {
                return null;
            }
        }

        protected override VideoDecoder CreateH265Decoder()
        {
            if (!Options.DisableNvidia)
            {
                return new Nvidia.H265.Decoder();
            }
            return null;
        }
    }
}
