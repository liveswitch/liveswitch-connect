using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Renderer : Receiver<RenderOptions, NamedPipeAudioSink, NamedPipeVideoSink>
    {
        public Task<int> Render(RenderOptions options)
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
            return Receive(options);
        }

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo, RenderOptions options)
        {
            if (options.AudioPipe == null || !remoteConnectionInfo.HasAudio)
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

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo, RenderOptions options)
        {
            if (options.VideoPipe == null || !remoteConnectionInfo.HasVideo)
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

        private AudioTrack CreateAudioTrack(RenderOptions options)
        {
            var tracks = new List<AudioTrack>();
            foreach (var codec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Copy))
            {
                tracks.Add(CreateAudioTrack(codec, options));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(RenderOptions options)
        {
            var tracks = new List<VideoTrack>();
            foreach (var codec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Copy))
            {
                if (options.DisableOpenH264 && codec == VideoCodec.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(codec, options));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioCodec codec, RenderOptions options)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var converter = new SoundConverter(new AudioConfig(options.AudioClockRate, options.AudioChannelCount));
            var reframer = new SoundReframer(new AudioConfig(options.AudioClockRate, options.AudioChannelCount), options.AudioFrameDuration);
            var sink = new NamedPipeAudioSink(options.AudioPipe, options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return new AudioTrack(depacketizer).Next(decoder).Next(converter).Next(reframer).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoCodec codec, RenderOptions options)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var converter = new Yuv.ImageConverter(options.VideoFormat.CreateFormat());
            converter.OnProcessFrame += (frame) =>
            {
                var buffer = frame.LastBuffer;
                var scale = (double)options.VideoWidth / frame.LastBuffer.Width;
                if (scale != converter.TargetScale)
                {
                    converter.TargetScale = scale;
                    Console.Error.WriteLine($"Video scale updated to {scale} (input frame size is {buffer.Width}x{buffer.Height}).");
                }
            };
            var sink = new NamedPipeVideoSink(options.VideoPipe, options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return new VideoTrack(depacketizer).Next(decoder).Next(converter).Next(sink);
        }
    }
}
