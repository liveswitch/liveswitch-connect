using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Player : Sender<PlayOptions, MatroskaAudioSource, MatroskaVideoSource>
    {
        public Player(PlayOptions options)
            : base(options)
        { }

        public Task<int> Play()
        {
            if (Options.AudioPath == null)
            {
                Console.Error.WriteLine("Setting --no-audio to true because --audio-path is not specified.");
                Options.NoAudio = true;
            }
            if (Options.VideoPath == null)
            {
                Console.Error.WriteLine("Setting --no-video to true because --video-path is not specified.");
                Options.NoVideo = true;
            }
            return Send();
        }

        protected override MatroskaAudioSource CreateAudioSource()
        {
            return new MatroskaAudioSource(Options.AudioPath);
        }

        protected override MatroskaVideoSource CreateVideoSource()
        {
            return new MatroskaVideoSource(Options.VideoPath, Options);
        }
    }
}
