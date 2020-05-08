using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Logger : Receiver<LogOptions, NullAudioSink, NullVideoSink>
    {
        public Task<int> Log(LogOptions options)
        {
            if (options.NoAudio && options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return Task.FromResult(1);
            }
            return Receive(options);
        }

        private string ProcessAudioLog(string log, AudioFrame frame, AudioCodec codec, ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var pcmBuffer = GetPcmAudioBuffer(frame);
            var compressedBuffer = GetCompressedAudioBuffer(frame);
            return log
                .Replace("{footprint}", compressedBuffer.Footprint.ToString())
                .Replace("{duration}", frame.Duration.ToString())
                .Replace("{clockRate}", pcmBuffer.Format.ClockRate.ToString())
                .Replace("{channelCount}", pcmBuffer.Format.ChannelCount.ToString())
                .Replace("{mediaStreamId}", frame.Mid)
                .Replace("{rtpStreamId}", frame.RtpStreamId)
                .Replace("{sequenceNumber}", frame.SequenceNumber.ToString())
                .Replace("{synchronizationSource}", frame.SynchronizationSource.ToString())
                .Replace("{systemTimestamp}", frame.SystemTimestamp.ToString())
                .Replace("{timestamp}", frame.Timestamp.ToString())
                .Replace("{codec}", codec.ToString())
                .Replace("{applicationId}", options.ApplicationId)
                .Replace("{channelId}", options.ChannelId)
                .Replace("{userId}", remoteConnectionInfo.UserId)
                .Replace("{userAlias}", remoteConnectionInfo.UserAlias)
                .Replace("{deviceId}", remoteConnectionInfo.DeviceId)
                .Replace("{deviceAlias}", remoteConnectionInfo.DeviceAlias)
                .Replace("{clientId}", remoteConnectionInfo.ClientId)
                .Replace("{clientTag}", remoteConnectionInfo.ClientTag)
                .Replace("{connectionId}", remoteConnectionInfo.Id)
                .Replace("{connectionTag}", remoteConnectionInfo.Tag)
                .Replace("{mediaId}", remoteConnectionInfo.MediaId);
        }

        private string ProcessVideoLog(string log, VideoFrame frame, VideoCodec codec, ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var rawBuffer = GetRawVideoBuffer(frame);
            var compressedBuffer = GetCompressedVideoBuffer(frame);
            return log
                .Replace("{footprint}", compressedBuffer.Footprint.ToString())
                .Replace("{width}", rawBuffer.Width.ToString())
                .Replace("{height}", rawBuffer.Height.ToString())
                .Replace("{mediaStreamId}", frame.Mid)
                .Replace("{rtpStreamId}", frame.RtpStreamId)
                .Replace("{sequenceNumber}", frame.SequenceNumber.ToString())
                .Replace("{synchronizationSource}", frame.SynchronizationSource.ToString())
                .Replace("{systemTimestamp}", frame.SystemTimestamp.ToString())
                .Replace("{timestamp}", frame.Timestamp.ToString())
                .Replace("{codec}", codec.ToString())
                .Replace("{applicationId}", options.ApplicationId)
                .Replace("{channelId}", options.ChannelId)
                .Replace("{userId}", remoteConnectionInfo.UserId)
                .Replace("{userAlias}", remoteConnectionInfo.UserAlias)
                .Replace("{deviceId}", remoteConnectionInfo.DeviceId)
                .Replace("{deviceAlias}", remoteConnectionInfo.DeviceAlias)
                .Replace("{clientId}", remoteConnectionInfo.ClientId)
                .Replace("{clientTag}", remoteConnectionInfo.ClientTag)
                .Replace("{connectionId}", remoteConnectionInfo.Id)
                .Replace("{connectionTag}", remoteConnectionInfo.Tag)
                .Replace("{mediaId}", remoteConnectionInfo.MediaId);
        }

        private AudioBuffer GetPcmAudioBuffer(AudioFrame frame)
        {
            var buffers = frame.Buffers;
            for (var i = buffers.Length - 1; i >= 0; i--)
            {
                if (buffers[i].Format.IsPcm)
                {
                    return buffers[i];
                }
            }
            return null;
        }

        private AudioBuffer GetCompressedAudioBuffer(AudioFrame frame)
        {
            var buffers = frame.Buffers;
            for (var i = buffers.Length - 1; i >= 0; i--)
            {
                if (buffers[i].Format.IsCompressed)
                {
                    return buffers[i];
                }
            }
            return null;
        }

        private VideoBuffer GetRawVideoBuffer(VideoFrame frame)
        {
            var buffers = frame.Buffers;
            for (var i = buffers.Length - 1; i >= 0; i--)
            {
                if (buffers[i].Format.IsRaw)
                {
                    return buffers[i];
                }
            }
            return null;
        }

        private VideoBuffer GetCompressedVideoBuffer(VideoFrame frame)
        {
            var buffers = frame.Buffers;
            for (var i = buffers.Length - 1; i >= 0; i--)
            {
                if (buffers[i].Format.IsCompressed)
                {
                    return buffers[i];
                }
            }
            return null;
        }

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            if (options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var track = CreateAudioTrack(remoteConnectionInfo, options);
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

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            if (options.NoVideo || !remoteConnectionInfo.HasVideo)
            {
                return null;
            }

            var track = CreateVideoTrack(remoteConnectionInfo, options);
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

        private AudioTrack CreateAudioTrack(ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var tracks = new List<AudioTrack>();
            foreach (var codec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Copy))
            {
                tracks.Add(CreateAudioTrack(codec, remoteConnectionInfo, options));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var tracks = new List<VideoTrack>();
            foreach (var codec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Copy))
            {
                if (options.DisableOpenH264 && codec == VideoCodec.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(codec, remoteConnectionInfo, options));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioCodec codec, ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var sink = new NullAudioSink();
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessAudioLog(options.AudioLog, frame, codec, remoteConnectionInfo, options));
            };
            return new AudioTrack(depacketizer).Next(decoder).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoCodec codec, ConnectionInfo remoteConnectionInfo, LogOptions options)
        {
            var depacketizer = codec.CreateDepacketizer();
            var decoder = codec.CreateDecoder();
            var sink = new NullVideoSink();
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessVideoLog(options.VideoLog, frame, codec, remoteConnectionInfo, options));
            };
            return new VideoTrack(depacketizer).Next(decoder).Next(sink);
        }
    }
}
