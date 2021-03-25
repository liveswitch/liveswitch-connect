using FM.LiveSwitch.Matroska;
using System;
using System.Threading.Tasks;
using NewTek;
using NewTek.NDI;

namespace FM.LiveSwitch.Connect
{
    class NdiRenderer : Receiver<NdiRenderOptions, NdiAudioSink, NdiVideoSink>
    {
        static ILog _Log = Log.GetLogger(typeof(NdiRenderer));

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

            switch (Options.VideoFormat)
            {
                case ImageFormat.I420:
                    // Supported
                    break;
                default:
                    Console.Error.WriteLine("--video-format not supported");
                    return Task.FromResult(1);
            }

            return Receive();
        }

        protected Sender _NdiSender;

        protected override NdiAudioSink CreateAudioSink()
        {
            _Log.Info("Ndi Audio Sink Created");
            var maxRate = Options.AudioClockRate / 1000 * Options.AudioFrameDuration; // 1000ms
            var sink = new NdiAudioSink(_NdiSender, maxRate,  Options.AudioClockRate, Options.AudioChannelCount, new Pcm.Format(Options.AudioClockRate, Options.AudioChannelCount));
            
            return sink;
        }

        protected override NdiVideoSink CreateVideoSink()
        {
            _Log.Info("Ndi Video Sink Created");
            var sink = new NdiVideoSink(_NdiSender, Options.VideoWidth, Options.VideoHeight, Options.FrameRateNumerator, Options.FrameRateDenominator, ImageFormatExtensions.CreateFormat(Options.VideoFormat));
            return sink;
        }

        protected override int GetAudioFrameDuration()
        {
            return Options.AudioFrameDuration;
        }

        protected override Task Initialize()
        {
            string failoverName = $"{System.Net.Dns.GetHostName()}-{Options.StreamName}";
            _Log.Info($"Initializing NDI Stream - {Options.StreamName} (Alt: {failoverName})");

            // Faster to put the clock on audio as opposed to video
            _NdiSender = new Sender(Options.StreamName, false, true, null, failoverName);
            
            return base.Initialize();
        }

        protected override Task Ready()
        {
            return base.Ready();
        }

        protected override Task Unready()
        {
            _NdiSender.Dispose();
            return base.Unready();
        }
    }
}
