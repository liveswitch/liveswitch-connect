using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class SpouttRenderer : Receiver<SpoutRenderOptions, NullAudioSink, SpoutVideoSink>
    {
        public SpouttRenderer(SpoutRenderOptions options)
            : base(options)
        { }

        public Task<int> Render()
        {
            if (Options.VideoWidth == 0)
            {
                Console.Error.WriteLine("--video-width must be a specified if --video-pipe is specified.");
                return Task.FromResult(1);
            }
            if (Options.VideoHeight == 0)
            {
                Console.Error.WriteLine("--video-height must be a specified if --video-pipe is specified.");
                return Task.FromResult(1);
            }
            if (Options.VideoWidth % 2 != 0)
            {
                Console.Error.WriteLine("--video-width must be a multiple of 2.");
                return Task.FromResult(1);
            }
            if (Options.VideoHeight % 2 != 0)
            {
                Console.Error.WriteLine("--video-height must be a multiple of 2.");
                return Task.FromResult(1);
            }
            
            return Receive();
        }

        protected override NullAudioSink CreateAudioSink()
        {
            Console.WriteLine("Audio discarded");
            var sink = AudioFormat.ToEncoding().CreateNullSink(true);
            return sink;
        }

        protected override SpoutVideoSink CreateVideoSink()
        {
            Console.WriteLine("Spout Video Sink Created");
            var sink = new SpoutVideoSink(Options.NdiName, Options.VideoWidth, Options.VideoHeight, VideoFormat.Bgra);
            return sink;
        }

        protected override int GetAudioFrameDuration()
        {
            return 0;
        }

        protected override Task Ready()
        {
            var videoConverter = VideoConverter;
            if (videoConverter != null)
            {
                videoConverter.OnProcessFrame += (frame) =>
                {
                    var buffer = frame.LastBuffer;
                    var scale = (double)Options.VideoWidth / frame.LastBuffer.Width;
                    if (scale != videoConverter.TargetScale)
                    {
                        videoConverter.TargetScale = scale;
                        Console.Error.WriteLine($"Video scale updated to {scale} (input frame size is {buffer.Width}x{buffer.Height}).");
                    }
                };
            }

            return base.Ready();
        }
    }
}
