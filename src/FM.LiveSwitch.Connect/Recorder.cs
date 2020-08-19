using System;
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

        protected override Matroska.AudioSink CreateAudioSink()
        {
            var fileIndex = 0;
            var fileExtension = "mka";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(Options.OutputPath, Options.OutputFileName), RemoteConnectionInfo);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, RemoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            return new Matroska.AudioSink(filePath);
        }

        protected override Matroska.VideoSink CreateVideoSink()
        {
            var fileIndex = 0;
            var fileExtension = "mkv";
            var filePathWithoutExtension = ProcessFilePath(Path.Combine(Options.OutputPath, Options.OutputFileName), RemoteConnectionInfo);
            var filePath = $"{filePathWithoutExtension}-{fileIndex}.{fileExtension}";
            while (File.Exists(filePath))
            {
                filePath = $"{filePathWithoutExtension}-{++fileIndex}.{fileExtension}";
            }

            //TODO: should the JSON here follow the media server conventions?
            var jsonPath = filePath + ".json";
            File.WriteAllText(jsonPath, RemoteConnectionInfo.ToJson());
            Console.WriteLine(jsonPath);

            return new Matroska.VideoSink(filePath);
        }
    }
}
