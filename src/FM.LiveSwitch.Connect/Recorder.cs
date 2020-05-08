using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Recorder : Receiver<RecordOptions, Matroska.AudioSink, Matroska.VideoSink>
    {
        public Task<int> Record(RecordOptions options)
        {
            if (options.NoAudio && options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return Task.FromResult(1);
            }
            if (!options.NoVideo)
            {
                if (options.DisableOpenH264 && options.VideoCodec == VideoCodec.H264)
                {
                    Console.Error.WriteLine("--video-codec cannot be H264. OpenH264 failed to initialize.");
                    return Task.FromResult(1);
                }
            }
            return Receive(options);
        }

        private string ProcessFilePath(string filePath, ConnectionInfo remoteConnectionInfo, RecordOptions options)
        {
            return filePath
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

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo, RecordOptions options)
        {
            if (options.NoAudio || !remoteConnectionInfo.HasAudio)
            {
                return null;
            }

            var fileIndex = 0;
            var fileExtension = "mka";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(options.OutputPath, options.OutputFileName), remoteConnectionInfo, options);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, remoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            var track = CreateAudioTrack(options, filePath);
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

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo, RecordOptions options)
        {
            if (options.NoVideo || !remoteConnectionInfo.HasVideo)
            {
                return null;
            }

            var fileIndex = 0;
            var fileExtension = "mkv";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(options.OutputPath, options.OutputFileName), remoteConnectionInfo, options);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, remoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            var track = CreateVideoTrack(options, filePath);
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

        private AudioTrack CreateAudioTrack(RecordOptions options, string filePath)
        {
            var tracks = new List<AudioTrack>();
            foreach (var inputCodec in ((AudioCodec[])Enum.GetValues(typeof(AudioCodec))).Where(x => x != AudioCodec.Copy))
            {
                var outputCodec = options.AudioCodec == AudioCodec.Copy ? inputCodec : options.AudioCodec;
                tracks.Add(CreateAudioTrack(inputCodec, outputCodec, filePath));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(RecordOptions options, string filePath)
        {
            var tracks = new List<VideoTrack>();
            foreach (var inputCodec in ((VideoCodec[])Enum.GetValues(typeof(VideoCodec))).Where(x => x != VideoCodec.Copy))
            {
                var outputCodec = options.VideoCodec == VideoCodec.Copy ? inputCodec : options.VideoCodec;
                if (options.DisableOpenH264 && (inputCodec == VideoCodec.H264 || outputCodec == VideoCodec.H264))
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(inputCodec, options.VideoCodec == VideoCodec.Copy ? inputCodec : options.VideoCodec, filePath));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioCodec inputCodec, AudioCodec outputCodec, string filePath)
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

        private VideoTrack CreateVideoTrack(VideoCodec inputCodec, VideoCodec outputCodec, string filePath)
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
