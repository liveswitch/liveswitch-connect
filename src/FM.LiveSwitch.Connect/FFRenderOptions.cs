using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("ffrender", HelpText = "Renders remote media to FFmpeg.")]
    class FFRenderOptions : ReceiveOptions
    {
        [Option("output-args", Required = true, HelpText = "The FFmpeg output arguments.")]
        public string OutputArgs { get; set; }

        [Option("audio-mode", Required = false, Default = FFRenderMode.LSDecode, HelpText = "Where audio is decoded.")]
        public FFRenderMode AudioMode { get; set; }

        [Option("video-mode", Required = false, Default = FFRenderMode.LSDecode, HelpText = "Where video is decoded.")]
        public FFRenderMode VideoMode { get; set; }

        [Option("audio-encoding", Required = false, HelpText = "The audio encoding of the output stream, if different from audio-codec. Enables transcoding if audio-mode is nodecode or ffdecode.")]
        public AudioEncoding? AudioEncoding { get; set; }

        [Option("video-encoding", Required = false, HelpText = "The video encoding of the output stream, if different from video-codec. Enables transcoding if video-mode is nodecode or ffdecode.")]
        public VideoEncoding? VideoEncoding { get; set; }

        [Option("keyframe-interval", Required = false, Default = 60, HelpText = "The keyframe interval for video, in frames. Only used if video-mode is nodecode.")]
        public int KeyFrameInterval { get; set; }
    }
}
