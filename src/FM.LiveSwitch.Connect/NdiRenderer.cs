using FM.LiveSwitch.Matroska;
using System;
using System.Threading.Tasks;
using NewTek;
using NewTek.NDI;

namespace FM.LiveSwitch.Connect
{
    class NdiRenderer : Receiver<NdiRenderOptions, NdiAudioSink, NdiVideoSink>
    {
        public NdiRenderer(NdiRenderOptions options)
            : base(options)
        { }

        public Task<int> Render()
        {

            if (Options.AudioClockRate % 8000 != 0)
            {
                Console.Error.WriteLine("--audio-clock-rate must be a multiple of 8000.");
                return Task.FromResult(1);
            }
            if (Options.AudioClockRate < 8000)
            {
                Console.Error.WriteLine("--audio-clock-rate minimum value is 8000.");
                return Task.FromResult(1);
            }
            if (Options.AudioClockRate > 48000)
            {
                Console.Error.WriteLine("--audio-clock-rate maximum value is 48000.");
                return Task.FromResult(1);
            }
            if (Options.AudioChannelCount < 1)
            {
                Console.Error.WriteLine("--audio-channel-count minimum value is 1.");
                return Task.FromResult(1);
            }
            if (Options.AudioChannelCount > 2)
            {
                Console.Error.WriteLine("--audio-channel-count maximum value is 2.");
                return Task.FromResult(1);
            }
            if (Options.AudioFrameDuration < 5)
            {
                Console.Error.WriteLine("--audio-frame-duration minimum value is 5.");
                return Task.FromResult(1);
            }
            if (Options.AudioFrameDuration > 100)
            {
                Console.Error.WriteLine("--audio-frame-duration maximum value is 100.");
                return Task.FromResult(1);
            }
            


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

        protected Sender NdiSender;

        protected bool IsNdiConnected = false;
        protected bool IsAwaitingFirstFrame = true;

        protected override NdiAudioSink CreateAudioSink()
        {
            Console.WriteLine("Ndi Audio Sink Created");
            var sink = new NdiAudioSink(NdiSender, Options.AudioClockRate, Options.AudioChannelCount, new Pcm.Format(Options.AudioClockRate, Options.AudioChannelCount));
            return sink;
        }

        protected override NdiVideoSink CreateVideoSink()
        {
            Console.WriteLine("Ndi Video Sink Created");
            var sink = new NdiVideoSink(NdiSender, Options.VideoWidth, Options.VideoHeight, VideoFormat.Bgra);
            return sink;
        }

        protected override int GetAudioFrameDuration()
        {
            return Options.AudioFrameDuration;
        }

        protected override Task Initialize()
        {
            string failoverName = $"{System.Net.Dns.GetHostName()}-{Options.StreamName}";
            NdiSender = new Sender(Options.StreamName, true, false, null, failoverName);
            
            return base.Initialize();
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

        protected override Task Unready()
        {
            NdiSender.Dispose();
            return base.Unready();
        }
    }
}
