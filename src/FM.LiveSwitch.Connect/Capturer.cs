using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Capturer : Sender<CaptureOptions, NamedPipeAudioSource, NamedPipeVideoSource>
    {
        public Capturer(CaptureOptions options)
            : base(options)
        { }

        public Task<int> Capture()
        {
            if (Options.AudioPipe == null && Options.VideoPipe == null)
            {
                Console.Error.WriteLine("--audio-pipe and/or --video-pipe must be specified.");
                return Task.FromResult(1);
            }
            if (Options.AudioPipe != null)
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
            }
            if (Options.VideoPipe != null)
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
            }
            return Send();
        }

        protected override NamedPipeAudioSource CreateAudioSource()
        {
            var source = new NamedPipeAudioSource(Options.AudioPipe, new Pcm.Format(Options.AudioClockRate, Options.AudioChannelCount), Options.AudioFrameDuration, Options.Server);
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return source;
        }

        protected override NamedPipeVideoSource CreateVideoSource()
        {
            var source = new NamedPipeVideoSource(Options.VideoPipe, Options.VideoWidth, Options.VideoHeight, Options.VideoFormat.CreateFormat(), Options.Server);
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return source;
        }
    }
}
