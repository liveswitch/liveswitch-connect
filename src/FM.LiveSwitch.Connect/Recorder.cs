using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Recorder : Receiver<RecordOptions, LiveSwitch.Matroska.AudioSink, LiveSwitch.Matroska.VideoSink>
    {
        public Recorder(RecordOptions options)
            : base(options)
        { }

        public Task<int> Record()
        {
            if (!Options.NoVideo)
            {
                if (!Options.IsH264EncoderAvailable() && Options.VideoCodec == VideoCodec.H264)
                {
                    Console.Error.WriteLine("--video-codec cannot be H264. No H.264 encoder is available.");
                    return Task.FromResult(1);
                }
                if (Options.DisableNvidia && Options.VideoCodec == VideoCodec.H265)
                {
                    Console.Error.WriteLine("--video-codec cannot be H265. Nvidia hardware support is unavailable.");
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
            foreach (var inputCodec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Any))
            {
                var outputCodec = Options.AudioCodec == AudioCodec.Any ? inputCodec : Options.AudioCodec;
                tracks.Add(CreateAudioTrack(inputCodec, outputCodec, filePath));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(string filePath)
        {
            var tracks = new List<VideoTrack>();
            foreach (var inputCodec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Any))
            {
                var outputCodec = Options.VideoCodec == VideoCodec.Any ? inputCodec : Options.VideoCodec;
                if (Options.DisableOpenH264 && (inputCodec == VideoCodec.H264 || outputCodec == VideoCodec.H264))
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(inputCodec, Options.VideoCodec == VideoCodec.Any ? inputCodec : Options.VideoCodec, filePath));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioCodec inputCodec, AudioCodec outputCodec, string filePath)
        {
            var depacketizer = inputCodec.CreateDepacketizer();
            var sink = new LiveSwitch.Matroska.AudioSink(filePath);
            if (inputCodec == outputCodec)
            {
                return new AudioTrack(depacketizer).Next(sink);
            }

            var decoder = inputCodec.CreateDecoder();
            var encoder = outputCodec.CreateEncoder();
            return new AudioTrack(depacketizer).Next(decoder).Next(new SoundConverter(encoder.InputConfig)).Next(encoder).Next(sink);
        }

        private VideoTrack CreateVideoTrack(VideoCodec inputCodec, VideoCodec outputCodec, string filePath)
        {
            var depacketizer = inputCodec.CreateDepacketizer();
            var sink = new LiveSwitch.Matroska.VideoSink(filePath);
            if (inputCodec == outputCodec)
            {
                return new VideoTrack(depacketizer).Next(sink);
            }

            var decoder = inputCodec.CreateDecoder(Options);
            var encoder = outputCodec.CreateEncoder(Options);
            return new VideoTrack(depacketizer).Next(decoder).Next(new Yuv.ImageConverter(encoder.InputFormat)).Next(encoder).Next(sink);
        }
    }
}
