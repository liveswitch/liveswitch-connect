using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFRenderer : Receiver<FFRenderOptions, FFAudioSink, FFVideoSink>
    {
        private static string ShortId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        }

        public Task<int> Render(FFRenderOptions options)
        {
            if (options.NoAudio && options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return Task.FromResult(1);
            }
            return Receive(options);
        }

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo, FFRenderOptions options)
        {
            if (options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var track = CreateAudioTrack(options);
            var stream = new AudioStream(null, track);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo, FFRenderOptions options)
        {
            if (options.NoVideo || !remoteConnectionInfo.HasVideo)
            {
                return null;
            }

            var track = CreateVideoTrack(options);
            var stream = new VideoStream(null, track);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        private AudioTrack CreateAudioTrack(FFRenderOptions options)
        {
            var sink = new FFAudioSink($"ffrender_audio_{ShortId()}");
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };

            var tracks = new List<AudioTrack>();
            foreach (var codec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Copy))
            {
                tracks.Add(CreateAudioTrack(codec, options));
            }
            return new AudioTrack(tracks.ToArray()).Next(sink);
        }

        private VideoTrack CreateVideoTrack(FFRenderOptions options)
        {
            var sink = new FFVideoSink($"ffrender_video_{ShortId()}");
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };

            var tracks = new List<VideoTrack>();
            foreach (var codec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Copy))
            {
                if (options.DisableOpenH264 && codec == VideoCodec.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(codec));
            }
            return new VideoTrack(tracks.ToArray()).Next(sink);
        }

        private AudioTrack CreateAudioTrack(AudioCodec codec, FFRenderOptions options)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var converter = new SoundConverter(Opus.Format.DefaultConfig);
            return new AudioTrack(depacketizer).Next(decoder).Next(converter);
        }

        private VideoTrack CreateVideoTrack(VideoCodec codec)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var converter = new Yuv.ImageConverter(VideoFormat.I420); //TODO: use matroska and remove this?
            return new VideoTrack(depacketizer).Next(decoder).Next(converter);
        }

        private Process FFmpeg;

        protected override void SinksReady(FFAudioSink[] audioSinks, FFVideoSink[] videoSinks, FFRenderOptions options)
        {
            base.SinksReady(audioSinks, videoSinks, options);

            var args = new List<string>
            {
                "-y"
            };

            if (audioSinks != null)
            {
                var audioSink = audioSinks[0];
                var config = audioSink.Config;
                args.AddRange(new[]
                {
                    $"-f s16le",
                    $"-ar {config.ClockRate}",
                    $"-ac {config.ChannelCount}",
                    $"-i {NamedPipe.GetOSPipeName(audioSink.PipeName)}",
                });
            }

            if (videoSinks != null)
            {
                var videoSink = videoSinks[0];
                args.AddRange(new[]
                {
                    $"-f yuv4mpegpipe",
                    $"-i {NamedPipe.GetOSPipeName(videoSink.PipeName)}",
                });
            }

            args.Add(options.OutputArgs);

            FFmpeg = FFUtility.FFmpeg(string.Join(" ", args));
        }

        protected override void SinksUnready(FFAudioSink[] audioSinks, FFVideoSink[] videoSinks, FFRenderOptions options)
        {
            if (audioSinks != null)
            {
                var audioSink = audioSinks[0];
                audioSink.Deactivated = true;
            }

            if (videoSinks != null)
            {
                var videoSink = videoSinks[0];
                videoSink.Deactivated = true;
            }

            if (FFmpeg != null)
            {
                FFmpeg.StandardInput.Write('q');
                FFmpeg.WaitForExit();
            }

            base.SinksUnready(audioSinks, videoSinks, options);
        }
    }
}
