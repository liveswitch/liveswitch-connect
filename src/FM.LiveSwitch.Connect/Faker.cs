using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Faker : Sender<FakeOptions, FakeAudioSource, FakeVideoSource>
    {
        public Faker(FakeOptions options)
            : base(options)
        { }

        public Task<int> Fake()
        {
            if (!Options.NoAudio)
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
                if (Options.AudioClockRate > 96000)
                {
                    Console.Error.WriteLine("--audio-clock-rate maximum value is 96000.");
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
                if (Options.AudioFrequency < 20)
                {
                    Console.Error.WriteLine("--audio-frequency minimum value is 20.");
                    return Task.FromResult(1);
                }
                if (Options.AudioFrequency > 20000)
                {
                    Console.Error.WriteLine("--audio-frequency maximum value is 20000.");
                    return Task.FromResult(1);
                }
                if (Options.AudioFrequency < 1)
                {
                    Console.Error.WriteLine("--audio-amplitude minimum value is 1.");
                    return Task.FromResult(1);
                }
                if (Options.AudioFrequency > 32767)
                {
                    Console.Error.WriteLine("--audio-amplitude maximum value is 32767.");
                    return Task.FromResult(1);
                }
            }
            if (!Options.NoVideo)
            {
                if (!Options.VideoWidth.HasValue || Options.VideoWidth == 0)
                {
                    Console.Error.WriteLine("--video-width must be a specified if --video-pipe is specified.");
                    return Task.FromResult(1);
                }
                if (!Options.VideoHeight.HasValue || Options.VideoHeight == 0)
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
                if (!Options.VideoFrameRate.HasValue || Options.VideoFrameRate < 1)
                {
                    Console.Error.WriteLine("--video-frame-rate minimum value is 1.");
                    return Task.FromResult(1);
                }
                if (Options.VideoFrameRate > 120)
                {
                    Console.Error.WriteLine("--video-frame-rate maximum value is 120.");
                    return Task.FromResult(1);
                }
            }
            return Send();
        }

        protected override FakeAudioSource CreateAudioSource()
        {
            return new FakeAudioSource(new AudioConfig(Options.AudioClockRate, Options.AudioChannelCount), Options.AudioFrequency, Options.AudioAmplitude);
        }

        protected override FakeVideoSource CreateVideoSource()
        {
            return new FakeVideoSource(new VideoConfig(Options.VideoWidth.Value, Options.VideoHeight.Value, Options.VideoFrameRate.Value), Options.VideoFormat.CreateFormat());
        }
    }
}
