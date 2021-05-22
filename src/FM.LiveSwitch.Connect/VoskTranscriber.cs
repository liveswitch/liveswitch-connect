using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Vosk;

namespace FM.LiveSwitch.Connect
{
    class VoskTranscriber : Receiver<VoskTranscribeOptions, VoskTranscribeAudioSink, NullVideoSink>
    {
        private Uri _ModelUri;
        private Model _Model;
        private bool _Download;

        private const string _ModelsPath = "models";

        public VoskTranscriber(VoskTranscribeOptions options)
            : base(options)
        { }

        public Task<int> Transcribe()
        {
            Options.NoVideo = true;

            if (!Uri.TryCreate(Options.ModelUri, UriKind.Absolute, out _ModelUri))
            {
                Console.Error.WriteLine("--model-uri must be a valid URI.");
                return Task.FromResult(1);
            }

            if (_ModelUri.Scheme == Uri.UriSchemeHttp ||
                _ModelUri.Scheme == Uri.UriSchemeHttps)
            {
                _Download = true;
            }
            else if (_ModelUri.Scheme != Uri.UriSchemeFile)
            {
                Console.Error.WriteLine("--model-uri must start with http://, https://, or file://.");
                return Task.FromResult(1);
            }

            return Receive();
        }

        protected override async Task Initialize()
        {
            if (!Directory.Exists(_ModelsPath))
            {
                Directory.CreateDirectory(_ModelsPath);
            }

            var modelName = Path.GetFileNameWithoutExtension(Options.ModelUri);
            var modelPath = Path.Combine(_ModelsPath, modelName);

            if (!Directory.Exists(modelPath))
            {
                if (_Download)
                {
                    var fileName = Path.GetFileName(Options.ModelUri);
                    try
                    {
                        if (!File.Exists(fileName))
                        {
                            using var webClient = new WebClient();
                            Console.Error.WriteLine($"Downloading {_ModelUri}...");
                            webClient.DownloadFile(_ModelUri, fileName);
                        }

                        Console.Error.WriteLine($"Extracting {fileName}...");
                        ZipFile.ExtractToDirectory(fileName, _ModelsPath);
                    }
                    finally
                    {
                        File.Delete(fileName);
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Extracting {_ModelUri.LocalPath}...");
                    ZipFile.ExtractToDirectory(_ModelUri.LocalPath, _ModelsPath);
                }
            }

            _Model = new Model(modelPath);

            await base.Initialize().ConfigureAwait(false);
        }

        protected override VoskTranscribeAudioSink CreateAudioSink()
        {
            var lastPartial = string.Empty;
            var sink = new VoskTranscribeAudioSink(_Model);
            sink.OnResult += (result) =>
            {
                var text = JObject.Parse(result)["text"].Value<string>();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine($">>>>>>>> {text}{Environment.NewLine}");
                    lastPartial = string.Empty;
                }
            };
            sink.OnPartialResult += (partialResult) =>
            {
                var partial = JObject.Parse(partialResult)["partial"].Value<string>();
                if (partial != lastPartial)
                {
                    Console.WriteLine($"partial> {partial}");
                    lastPartial = partial;
                }
            };
            return sink;
        }

        protected override NullVideoSink CreateVideoSink()
        {
            return new NullVideoSink();
        }
    }
}
