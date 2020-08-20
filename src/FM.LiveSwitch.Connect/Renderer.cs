using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Renderer : Receiver<RenderOptions, NamedPipeAudioSink, NamedPipeVideoSink>
    {
        public Renderer(RenderOptions options)
            : base(options)
        { }

        public Task<int> Render()
        {
            if (Options.AudioPipe == null)
            {
                Console.Error.WriteLine("Setting --no-audio to true because --audio-pipe is not specified.");
                Options.NoAudio = true;
            }
            else
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
            if (Options.VideoPipe == null)
            {
                Console.Error.WriteLine("Setting --no-video to true because --video-pipe is not specified.");
                Options.NoVideo = true;
            }
            else
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
            return Receive();
        }

        protected override NamedPipeAudioSink CreateAudioSink()
        {
            var sink = new NamedPipeAudioSink(Options.AudioPipe, Options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return sink;
        }

        protected override NamedPipeVideoSink CreateVideoSink()
        {
            var sink = new NamedPipeVideoSink(Options.VideoPipe, Options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return sink;
        }

        protected override int GetAudioFrameDuration()
        {
            return Options.AudioFrameDuration;
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
