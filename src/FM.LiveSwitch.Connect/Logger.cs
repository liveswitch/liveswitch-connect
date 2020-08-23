using System;
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

        protected override NullAudioSink CreateAudioSink()
        {
            var sink = new NullAudioSink(AudioFormat);
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessAudioLog(Options.AudioLog, frame, AudioFormat.ToEncoding(), RemoteConnectionInfo));
            };
            return sink;
        }

        protected override NullVideoSink CreateVideoSink()
        {
            var sink = new NullVideoSink(VideoFormat);
            sink.OnProcessFrame += (frame) =>
            {
                Console.WriteLine(ProcessVideoLog(Options.VideoLog, frame, VideoFormat.ToEncoding(), RemoteConnectionInfo));
            };
            return sink;
        }
    }
}
