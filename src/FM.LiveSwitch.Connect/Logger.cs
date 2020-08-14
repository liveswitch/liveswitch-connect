using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Logger : Receiver<LogOptions, NullAudioSink, NullVideoSink>
    {
        public Logger(LogOptions options) 
            : base(options)
        { }

        public Task<int> Log()
        {
            return Receive();
        }

        private string ProcessAudioLog(string log, AudioFrame frame, AudioEncoding encoding, ConnectionInfo remoteConnectionInfo)
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
                .Replace("{encoding}", encoding.ToString())
                .Replace("{applicationId}", Options.ApplicationId)
                .Replace("{channelId}", Options.ChannelId)
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

        private string ProcessVideoLog(string log, VideoFrame frame, VideoEncoding encoding, ConnectionInfo remoteConnectionInfo)
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
                .Replace("{encoding}", encoding.ToString())
                .Replace("{applicationId}", Options.ApplicationId)
                .Replace("{channelId}", Options.ChannelId)
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

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var track = CreateAudioTrack(remoteConnectionInfo);
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

            var track = CreateVideoTrack(remoteConnectionInfo);
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

        private AudioTrack CreateAudioTrack(ConnectionInfo remoteConnectionInfo)
        {
            var tracks = new List<AudioTrack>();
            foreach (var encoding in (AudioEncoding[])Enum.GetValues(typeof(AudioEncoding)))
            {
                tracks.Add(CreateAudioTrack(encoding, remoteConnectionInfo));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(ConnectionInfo remoteConnectionInfo)
        {
            var tracks = new List<VideoTrack>();
            foreach (var encoding in (VideoEncoding[])Enum.GetValues(typeof(VideoEncoding)))
            {
                if (Options.DisableOpenH264 && encoding == VideoEncoding.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(encoding, remoteConnectionInfo));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioEncoding encoding, ConnectionInfo remoteConnectionInfo)
        {
            var depacketizer = encoding.CreateDepacketizer();
            var decoder = encoding.CreateDecoder();
            var sink = new NullAudioSink();
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessAudioLog(Options.AudioLog, frame, encoding, remoteConnectionInfo));
            };
            return new AudioTrack(depacketizer).Next(decoder).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoEncoding encoding, ConnectionInfo remoteConnectionInfo)
        {
            var depacketizer = encoding.CreateDepacketizer();
            var decoder = encoding.CreateDecoder();
            var sink = new NullVideoSink();
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessVideoLog(Options.VideoLog, frame, encoding, remoteConnectionInfo));
            };
            return new VideoTrack(depacketizer).Next(decoder).Next(sink);
        }
    }
}
