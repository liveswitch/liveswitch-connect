using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Recorder : Receiver<RecordOptions, Matroska.AudioSink, Matroska.VideoSink>
    {
        public Recorder(RecordOptions options)
            : base(options)
        { }

        public Task<int> Record()
        {
            if (!Options.NoVideo)
            {
                if (Options.DisableOpenH264 && Options.VideoCodec == VideoCodec.H264)
                {
                    Console.Error.WriteLine("--video-codec cannot be H264. OpenH264 failed to initialize.");
                    return Task.FromResult(1);
                }
            }
            return Receive();
        }

        private string ProcessFilePath(string filePath, ConnectionInfo remoteConnectionInfo)
        {
            return filePath
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

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var fileIndex = 0;
            var fileExtension = "mka";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(Options.OutputPath, Options.OutputFileName), remoteConnectionInfo);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, remoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            var track = CreateAudioTrack(filePath);
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

            var fileIndex = 0;
            var fileExtension = "mkv";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(Options.OutputPath, Options.OutputFileName), remoteConnectionInfo);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, remoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            var track = CreateVideoTrack(filePath);
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

        private AudioTrack CreateAudioTrack(string filePath)
        {
            var tracks = new List<AudioTrack>();
            foreach (var inputEncoding in (AudioEncoding[])Enum.GetValues(typeof(AudioEncoding)))
            {
                var outputEncoding = Options.AudioCodec == AudioCodec.Any ? inputEncoding : Options.AudioCodec.ToEncoding();
                tracks.Add(CreateAudioTrack(inputEncoding, outputEncoding, filePath));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(string filePath)
        {
            var tracks = new List<VideoTrack>();
            foreach (var inputEncoding in (VideoEncoding[])Enum.GetValues(typeof(VideoEncoding)))
            {
                var outputEncoding = Options.VideoCodec == VideoCodec.Any ? inputEncoding : Options.VideoCodec.ToEncoding();
                if (Options.DisableOpenH264 && (inputEncoding == VideoEncoding.H264 || outputEncoding == VideoEncoding.H264))
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(inputEncoding, outputEncoding, filePath));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioEncoding inputCodec, AudioEncoding outputCodec, string filePath)
        {
            var depacketizer = inputCodec.CreateDepacketizer();
            var sink = new Matroska.AudioSink(filePath);
            if (inputCodec == outputCodec)
            {
                return new AudioTrack(depacketizer).Next(sink);
            }

            var decoder = inputCodec.CreateDecoder();
            var encoder = outputCodec.CreateEncoder();
            return new AudioTrack(depacketizer).Next(decoder).Next(new SoundConverter(encoder.InputConfig)).Next(encoder).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoEncoding inputCodec, VideoEncoding outputCodec, string filePath)
        {
            var depacketizer = inputCodec.CreateDepacketizer();
            var sink = new Matroska.VideoSink(filePath);
            if (inputCodec == outputCodec)
            {
                return new VideoTrack(depacketizer).Next(sink);
            }

            var decoder = inputCodec.CreateDecoder();
            var encoder = outputCodec.CreateEncoder();
            return new VideoTrack(depacketizer).Next(decoder).Next(new Yuv.ImageConverter(encoder.InputFormat)).Next(encoder).Next(sink);
        }
    }
}
