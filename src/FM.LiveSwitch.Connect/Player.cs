using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Player : Sender<PlayOptions, MatroskaAudioSource, MatroskaVideoSource>
    {
        public Task<int> Play(PlayOptions options)
        {
            if (options.AudioPath == null && options.VideoPath == null)
            {
                Console.Error.WriteLine("--audio-path and/or --video-path must be specified.");
                return Task.FromResult(1);
            }
            return Send(options);
        }

        protected override MatroskaAudioSource CreateAudioSource(PlayOptions options)
        {
            if (options.AudioPath == null)
            {
                return null;
            }
            return new MatroskaAudioSource(options.AudioPath);
        }

        protected override MatroskaVideoSource CreateVideoSource(PlayOptions options)
        {
            if (options.VideoPath == null)
            {
                return null;
            }
            return new MatroskaVideoSource(options.VideoPath);
        }
    }
}
