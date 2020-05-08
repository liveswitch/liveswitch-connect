using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Capturer : Sender<CaptureOptions, NamedPipeAudioSource, NamedPipeVideoSource>
    {
        public Task<int> Capture(CaptureOptions options)
        {
            if (options.AudioPipe == null && options.VideoPipe == null)
            {
                Console.Error.WriteLine("--audio-pipe and/or --video-pipe must be specified.");
                return Task.FromResult(1);
            }
            if (options.AudioPipe != null)
            {
                if (options.AudioClockRate % 8000 != 0)
                {
                    Console.Error.WriteLine("--audio-clock-rate must be a multiple of 8000.");
                    return Task.FromResult(1);
                }
                if (options.AudioClockRate < 8000)
                {
                    Console.Error.WriteLine("--audio-clock-rate minimum value is 8000.");
                    return Task.FromResult(1);
                }
                if (options.AudioClockRate > 48000)
                {
                    Console.Error.WriteLine("--audio-clock-rate maximum value is 48000.");
                    return Task.FromResult(1);
                }
                if (options.AudioChannelCount < 1)
                {
                    Console.Error.WriteLine("--audio-channel-count minimum value is 1.");
                    return Task.FromResult(1);
                }
                if (options.AudioChannelCount > 2)
                {
                    Console.Error.WriteLine("--audio-channel-count maximum value is 2.");
                    return Task.FromResult(1);
                }
                if (options.AudioFrameDuration < 5)
                {
                    Console.Error.WriteLine("--audio-frame-duration minimum value is 5.");
                    return Task.FromResult(1);
                }
                if (options.AudioFrameDuration > 100)
                {
                    Console.Error.WriteLine("--audio-frame-duration maximum value is 100.");
                    return Task.FromResult(1);
                }
            }
            if (options.VideoPipe != null)
            {
                if (options.VideoWidth == 0)
                {
                    Console.Error.WriteLine("--video-width must be a specified if --video-pipe is specified.");
                    return Task.FromResult(1);
                }
                if (options.VideoHeight == 0)
                {
                    Console.Error.WriteLine("--video-height must be a specified if --video-pipe is specified.");
                    return Task.FromResult(1);
                }
                if (options.VideoWidth % 2 != 0)
                {
                    Console.Error.WriteLine("--video-width must be a multiple of 2.");
                    return Task.FromResult(1);
                }
                if (options.VideoHeight % 2 != 0)
                {
                    Console.Error.WriteLine("--video-height must be a multiple of 2.");
                    return Task.FromResult(1);
                }
            }
            return Send(options);
        }

        protected override NamedPipeAudioSource CreateAudioSource(CaptureOptions options)
        {
            if (options.AudioPipe == null)
            {
                return null;
            }
            var source = new NamedPipeAudioSource(options.AudioPipe, new Pcm.Format(options.AudioClockRate, options.AudioChannelCount), options.AudioFrameDuration, options.Server);
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return source;
        }

        protected override NamedPipeVideoSource CreateVideoSource(CaptureOptions options)
        {
            if (options.VideoPipe == null)
            {
                return null;
            }
            var source = new NamedPipeVideoSource(options.VideoPipe, options.VideoWidth, options.VideoHeight, options.VideoFormat.CreateFormat(), options.Server);
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return source;
        }
    }
}
