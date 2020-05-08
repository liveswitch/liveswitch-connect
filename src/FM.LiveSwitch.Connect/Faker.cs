using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Faker : Sender<FakeOptions, FakeAudioSource, FakeVideoSource>
    {
        public Task<int> Fake(FakeOptions options)
        {
            if (options.NoAudio && options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return Task.FromResult(1);
            }
            if (!options.NoAudio)
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
                if (options.AudioClockRate > 96000)
                {
                    Console.Error.WriteLine("--audio-clock-rate maximum value is 96000.");
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
                if (options.AudioFrequency < 20)
                {
                    Console.Error.WriteLine("--audio-frequency minimum value is 20.");
                    return Task.FromResult(1);
                }
                if (options.AudioFrequency > 20000)
                {
                    Console.Error.WriteLine("--audio-frequency maximum value is 20000.");
                    return Task.FromResult(1);
                }
                if (options.AudioFrequency < 1)
                {
                    Console.Error.WriteLine("--audio-amplitude minimum value is 1.");
                    return Task.FromResult(1);
                }
                if (options.AudioFrequency > 32767)
                {
                    Console.Error.WriteLine("--audio-amplitude maximum value is 32767.");
                    return Task.FromResult(1);
                }
            }
            if (!options.NoVideo)
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
                if (options.VideoFrameRate < 1)
                {
                    Console.Error.WriteLine("--video-frame-rate minimum value is 1.");
                    return Task.FromResult(1);
                }
                if (options.VideoFrameRate > 120)
                {
                    Console.Error.WriteLine("--video-frame-rate maximum value is 120.");
                    return Task.FromResult(1);
                }
            }
            return Send(options);
        }

        protected override FakeAudioSource CreateAudioSource(FakeOptions options)
        {
            if (options.NoAudio)
            {
                return null;
            }
            return new FakeAudioSource(new AudioConfig(options.AudioClockRate, options.AudioChannelCount), options.AudioFrequency, options.AudioAmplitude);
        }

        protected override FakeVideoSource CreateVideoSource(FakeOptions options)
        {
            if (options.NoVideo)
            {
                return null;
            }
            return new FakeVideoSource(new VideoConfig(options.VideoWidth, options.VideoHeight, options.VideoFrameRate), options.VideoFormat.CreateFormat());
        }
    }
}
