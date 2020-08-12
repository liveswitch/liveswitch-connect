using System;
using System.Collections.Generic;
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
            if (Options.AudioPipe == null && Options.VideoPipe == null)
            {
                Console.Error.WriteLine("--audio-pipe and/or --video-pipe must be specified.");
                return Task.FromResult(1);
            }
            if (Options.AudioPipe != null)
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
            if (Options.VideoPipe != null)
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

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.AudioPipe == null || !remoteConnectionInfo.HasAudio)
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
            if (Options.VideoPipe == null || !remoteConnectionInfo.HasVideo)
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
            var tracks = new List<AudioTrack>();
            foreach (var encoding in (AudioEncoding[])Enum.GetValues(typeof(AudioEncoding)))
            {
                tracks.Add(CreateAudioTrack(encoding));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack()
        {
            var tracks = new List<VideoTrack>();
            foreach (var encoding in (VideoEncoding[])Enum.GetValues(typeof(VideoEncoding)))
            {
                if (Options.DisableOpenH264 && encoding == VideoEncoding.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(encoding));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioEncoding encoding)
        {
            var depacketizer = encoding.CreateDepacketizer();
            var decoder = encoding.CreateDecoder();
            var converter = new SoundConverter(new AudioConfig(Options.AudioClockRate, Options.AudioChannelCount));
            var reframer = new SoundReframer(new AudioConfig(Options.AudioClockRate, Options.AudioChannelCount), Options.AudioFrameDuration);
            var sink = new NamedPipeAudioSink(Options.AudioPipe, Options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Audio pipe connected.");
            };
            return new AudioTrack(depacketizer).Next(decoder).Next(converter).Next(reframer).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoEncoding encoding)
        {
            var depacketizer = encoding.CreateDepacketizer();
            var decoder = encoding.CreateDecoder();
            var converter = new Yuv.ImageConverter(Options.VideoFormat.CreateFormat());
            converter.OnProcessFrame += (frame) =>
            {
                var buffer = frame.LastBuffer;
                var scale = (double)Options.VideoWidth / frame.LastBuffer.Width;
                if (scale != converter.TargetScale)
                {
                    converter.TargetScale = scale;
                    Console.Error.WriteLine($"Video scale updated to {scale} (input frame size is {buffer.Width}x{buffer.Height}).");
                }
            };
            var sink = new NamedPipeVideoSink(Options.VideoPipe, Options.Client);
            sink.OnPipeConnected += () =>
            {
                Console.Error.WriteLine("Video pipe connected.");
            };
            return new VideoTrack(depacketizer).Next(decoder).Next(converter).Next(sink);
        }
    }
}
