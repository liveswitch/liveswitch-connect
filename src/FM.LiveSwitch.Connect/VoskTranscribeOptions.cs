using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("vosktranscribe", HelpText = "Transcribes speech to text using Vosk.")]
    class VoskTranscribeOptions : ReceiveOptions
    {
        [Option("model-uri", Required = false, Default = "https://alphacephei.com/kaldi/models/vosk-model-small-en-us-0.15.zip", HelpText = "The model URI. Must start with http://, https://, or file://.")]
        public string ModelUri { get; set; }
    }
}
