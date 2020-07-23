using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFRenderer : Receiver<FFRenderOptions, NamedPipeAudioSink, NamedPipeVideoSink>
    {
        public FFRenderer(FFRenderOptions options)
            : base(options)
        { }

        private static string ShortId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        }

        public Task<int> Render()
        {
            return Receive();
        }

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var track = CreateAudioTrack();
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

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.NoVideo || !remoteConnectionInfo.HasVideo)
            {
                return null;
            }

            var track = CreateVideoTrack();
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

        private AudioTrack CreateAudioTrack()
        {
            var sink = new PcmNamedPipeAudioSink($"ffrender_pcm_{ShortId()}");
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };

            var tracks = new List<AudioTrack>();
            foreach (var codec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Any))
            {
                tracks.Add(CreateAudioTrack(codec));
            }
            return new AudioTrack(tracks.ToArray()).Next(sink);
        }

        private VideoTrack CreateVideoTrack()
        {
            var sink = new Yuv4MpegNamedPipeVideoSink($"ffrender_i420_{ShortId()}");
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };

            var tracks = new List<VideoTrack>();
            foreach (var codec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Any))
            {
                if (!Options.IsH264EncoderAvailable() && codec == VideoCodec.H264)
                {
                    continue;
                }
                if (Options.DisableNvidia && codec == VideoCodec.H265)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(codec));
            }
            return new VideoTrack(tracks.ToArray()).Next(sink);
        }

        private AudioTrack CreateAudioTrack(AudioCodec codec)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var converter = new SoundConverter(Opus.Format.DefaultConfig);
            return new AudioTrack(depacketizer).Next(decoder).Next(converter);
        }

        private VideoTrack CreateVideoTrack(VideoCodec codec)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder(Options);
            var converter = new Yuv.ImageConverter(VideoFormat.I420); //TODO: use matroska and remove this?
            return new VideoTrack(depacketizer).Next(decoder).Next(converter);
        }

        private Process FFmpeg;

        protected override void SinksReady(NamedPipeAudioSink[] audioSinks, NamedPipeVideoSink[] videoSinks)
        {
            base.SinksReady(audioSinks, videoSinks);

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

            args.Add(Options.OutputArgs);

            FFmpeg = FFUtility.FFmpeg(string.Join(" ", args));
        }

        protected override void SinksUnready(NamedPipeAudioSink[] audioSinks, NamedPipeVideoSink[] videoSinks)
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

            base.SinksUnready(audioSinks, videoSinks);
        }
    }
}
