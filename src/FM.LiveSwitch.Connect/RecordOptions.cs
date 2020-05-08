using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("record", HelpText = "Records remote media to a local file.")]
    class RecordOptions : ReceiveOptions
    {
        [Option("output-path", Required = false, Default = ".", HelpText = "The output path for the recordings. Uses curly-brace syntax. Valid variables: applicationId, channelId, userId, userAlias, deviceId, deviceAlias, clientId, clientTag, connectionId, connectionTag, mediaId")]
        public string OutputPath { get; set; }

        [Option("output-file-name", Required = false, Default = "{connectionId}", HelpText = "The output file name template. Uses curly-brace syntax. Valid variables: applicationId, channelId, userId, userAlias, deviceId, deviceAlias, clientId, clientTag, connectionId, connectionTag, mediaId")]
        public string OutputFileName { get; set; }

        [Option("audio-codec", Required = false, Default = AudioCodec.Copy, HelpText = "The output audio codec.")]
        public AudioCodec AudioCodec { get; set; }

        [Option("video-codec", Required = false, Default = VideoCodec.Copy, HelpText = "The output video codec.")]
        public VideoCodec VideoCodec { get; set; }

        [Option("no-audio", Required = false, HelpText = "Do not record audio.")]
        public bool NoAudio { get; set; }

        [Option("no-video", Required = false, HelpText = "Do not record video.")]
        public bool NoVideo { get; set; }
    }
}
