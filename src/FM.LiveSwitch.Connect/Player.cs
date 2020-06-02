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
            if (Options.AudioPath == null && Options.VideoPath == null)
            {
                Console.Error.WriteLine("--audio-path and/or --video-path must be specified.");
                return Task.FromResult(1);
            }
            return Send();
        }

        protected override MatroskaAudioSource CreateAudioSource()
        {
            return new MatroskaAudioSource(Options.AudioPath);
        }

        protected override MatroskaVideoSource CreateVideoSource()
        {
            return new MatroskaVideoSource(Options.VideoPath);
        }
    }
}
