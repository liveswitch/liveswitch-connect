using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFCapturer : Sender<FFCaptureOptions, FFAudioSource, FFVideoSource>
    {
        private static string ShortId()
        {
            return Guid.NewGuid().ToString().Replace("-","").Substring(0, 8);
        }

        public Task<int> Capture(FFCaptureOptions options)
        {
            if (options.NoAudio && options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return Task.FromResult(1);
            }
            return Send(options);
        }

        protected override FFAudioSource CreateAudioSource(FFCaptureOptions options)
        {
            if (options.NoAudio)
            {
                return null;
            }

            var source = new FFAudioSource($"ffcapture_audio_{ShortId()}");
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return source;
        }

        protected override FFVideoSource CreateVideoSource(FFCaptureOptions options)
        {
            if (options.NoVideo)
            {
                return null;
            }

            var source = new FFVideoSource($"ffcapture_video_{ShortId()}");
            source.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return source;
        }

        private Process FFmpeg;

        protected override void SourcesReady(FFAudioSource audioSource, FFVideoSource videoSource, FFCaptureOptions options)
        {
            base.SourcesReady(audioSource, videoSource, options);

            var args = new List<string>
            {
                "-y",
                options.InputArgs
            };

            if (audioSource != null)
            {
                var config = audioSource.Config;
                args.AddRange(new[]
                {
                    $"-map 0:a:0",
                    $"-f s16le",
                    $"-ar {config.ClockRate}",
                    $"-ac {config.ChannelCount}",
                    NamedPipe.GetOSPipeName(audioSource.PipeName)
                });
            }

            if (videoSource != null)
            {
                args.AddRange(new[]
                {
                    $"-map 0:v:0",
                    $"-f yuv4mpegpipe",
                    $"-pix_fmt yuv420p", //TODO: remove I420 requirement using matroska? // videoSource.OutputFormat...?
                    NamedPipe.GetOSPipeName(videoSource.PipeName)
                });
            }

            FFmpeg = FFUtility.FFmpeg(string.Join(" ", args));
        }

        protected override void SourcesUnready(FFAudioSource audioSource, FFVideoSource videoSource, FFCaptureOptions options)
        {
            if (FFmpeg != null)
            {
                FFmpeg.StandardInput.Write('q');
                FFmpeg.WaitForExit();
            }

            base.SourcesUnready(audioSource, videoSource, options);
        }
    }
}
