using FM.LiveSwitch;
using FM.LiveSwitch.Yuv;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    abstract class Sender<TOptions, TAudioSource, TVideoSource>
        where TOptions : ISendOptions
        where TAudioSource : AudioSource
        where TVideoSource : VideoSource
    {
        public TOptions Options { get; private set; }

        private long AudioSynchronizationSource;
        private long VideoSynchronizationSource;

        protected AudioStream AudioStream { get; private set; }
        protected VideoStream VideoStream { get; private set; }
        protected DataStream DataStream { get; private set; }
        protected DataChannel DataChannel { get; private set; }

        protected AudioFormat AudioFormat { get; private set; }
        protected VideoFormat VideoFormat { get; private set; }

        protected TAudioSource AudioSource { get; private set; }
        protected TVideoSource VideoSource { get; private set; }
        protected DataSource DataSource { get; private set; }

        protected AudioDepacketizer AudioDepacketizer { get; private set; }
        protected VideoPipe VideoDepacketizer { get; private set; }

        protected AudioDecoder AudioDecoder { get; private set; }
        protected VideoDecoder VideoDecoder { get; private set; }

        protected ResetAudioPipe ResetAudioPipe { get; private set; }
        protected ResetVideoPipe ResetVideoPipe { get; private set; }

        protected SoundConverter AudioConverter { get; private set; }
        protected ImageConverter VideoConverter { get; private set; }

        protected AudioEncoder AudioEncoder { get; private set; }
        protected VideoEncoder VideoEncoder { get; private set; }

        protected AudioPacketizer AudioPacketizer { get; private set; }
        protected VideoPacketizer VideoPacketizer { get; private set; }

        protected Client Client { get; private set; }
        protected Channel Channel { get; private set; }
        protected ManagedConnection Connection { get; private set; }

        private AudioPipe[] AudioPipes;
        private VideoPipe[] VideoPipes;

        private Task Disconnected;

        public Sender(TOptions options)
        {
            Options = options;
        }

        protected async Task<int> Send()
        {
            if (Options.NoAudio && Options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return 1;
            }

            try
            {
                var exit = false;
                while (!exit)
                {
                    Client = Options.CreateClient();

                    Console.Error.WriteLine($"{GetType().Name} client created:{Environment.NewLine}{Descriptor.Format(Client.GetDescriptors())}");

                    await Client.Register(Options);

                    Console.Error.WriteLine($"{GetType().Name} client registered: {Options.ApplicationId}");

                    try
                    {
                        Channel = await Client.Join(Options);

                        Console.Error.WriteLine($"{GetType().Name} client joined: {Channel.Id}");

                        try
                        {
                            var connected = false;
                            while (!connected)
                            {
                                InitializeAudioStream();
                                InitializeVideoStream();
                                InitializeDataStream();

                                Console.Error.WriteLine($"{GetType().Name} streams initialized.");

                                Connection = Options.CreateConnection(Channel, AudioStream, VideoStream, DataStream);

                                Console.Error.WriteLine($"{GetType().Name} connection created:{Environment.NewLine}{Descriptor.Format(Connection.GetDescriptors())}");

                                try
                                {
                                    Disconnected = await Connection.Connect();
                                    connected = true;
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine($"{GetType().Name} connection failed. {ex}");

                                    DestroyAudioStream();
                                    DestroyVideoStream();
                                    DestroyDataStream();
                                }
                            }

                            Console.Error.WriteLine($"{GetType().Name} connection connected.");

                            await Task.WhenAll(
                                StartAudioStream(),
                                StartVideoStream(),
                                StartDataStream());

                            Console.Error.WriteLine($"{GetType().Name} streams started.");

                            await Ready();

                            Console.Error.WriteLine($"{GetType().Name} is ready and waiting for exit signal or disconnect...");

                            await Task.WhenAny(ExitSignal(), Disconnected);

                            if (Connection.State == ConnectionState.Failed)
                            {
                                Console.Error.WriteLine($"{GetType().Name} connection failed. {Client.UnregisterException}");
                            }
                            else if (Client.UnregisterException != null)
                            {
                                Console.Error.WriteLine($"{GetType().Name} client failed. {Client.UnregisterException}");
                            }
                            else
                            {
                                if (Disconnected.IsCompletedSuccessfully)
                                {
                                    Console.Error.WriteLine($"{GetType().Name} connection was closed. {Client.UnregisterException}");
                                }
                                else
                                {
                                    Console.Error.WriteLine($"{GetType().Name} received exit signal.");
                                }
                                exit = true;
                            }

                            await Unready();

                            Console.Error.WriteLine($"{GetType().Name} is not ready.");

                            await Task.WhenAll(
                                StopAudioStream(),
                                StopVideoStream(),
                                StopDataStream());

                            Console.Error.WriteLine($"{GetType().Name} streams stopped.");

                            if (Connection.State == ConnectionState.Connected)
                            {
                                await Connection.Disconnect();

                                Console.Error.WriteLine($"{GetType().Name} connection disconnected.");
                            }

                            DestroyAudioStream();
                            DestroyVideoStream();
                            DestroyDataStream();

                            Console.Error.WriteLine($"{GetType().Name} streams destroyed.");

                        }
                        finally
                        {
                            if (Client.State == ClientState.Registered)
                            {
                                try
                                {
                                    await Client.Leave(Channel.Id);

                                    Console.Error.WriteLine($"{GetType().Name} client left.");
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine($"Could not leave gracefully. {ex}");
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (Client.State == ClientState.Registered)
                        {
                            try
                            {
                                await Client.Unregister();

                                Console.Error.WriteLine($"{GetType().Name} client unregistered.");
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Could not unregister gracefully. {ex}");
                            }
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        private Task ExitSignal()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                taskCompletionSource.TrySetResult(true);
            };

            return taskCompletionSource.Task;
        }

        #region Audio

        private bool InitializeAudioStream()
        {
            if (Options.NoAudio)
            {
                return false;
            }

            AudioSynchronizationSource = Utility.GenerateSynchronizationSource();
            AudioPipes = Options.GetAudioEncodings().Select(audioEncoding =>
            {
                var identityAudioPipe = new IdentityAudioPipe(audioEncoding.CreateFormat(true));
                if (Options.AudioBitrate.HasValue)
                {
                    identityAudioPipe.UpdateTargetOutputBitrate(Options.AudioBitrate.Value);
                    identityAudioPipe.UpdateMaxOutputBitrate(Options.AudioBitrate.Value);
                }
                return identityAudioPipe;
            }).ToArray();

            foreach (var pipe in AudioPipes)
            {
                pipe.SynchronizationSource = AudioSynchronizationSource;
            }

            AudioStream = new AudioStream(AudioPipes, null);
            DoInitializeAudioStream();
            return true;
        }

        private async Task StartAudioStream()
        {
            if (AudioStream != null)
            {
                var inputFormat = AudioStream.InputFormat;
                if (inputFormat == null)
                {
                    throw new Exception("Could not negotiate an audio codec with the server.");
                }
                AudioFormat = inputFormat.Clone();

                AudioSource = CreateAudioSource();
                AudioSource.SynchronizationSource = AudioSynchronizationSource;

                var currentOutput = (IAudioOutput)AudioSource;

                if (Options.AudioTranscode)
                {
                    if (currentOutput.OutputFormat.IsPacketized)
                    {
                        AudioDepacketizer = currentOutput.OutputFormat.ToEncoding().CreateDepacketizer();

                        currentOutput.AddOutput(AudioDepacketizer);
                        currentOutput = AudioDepacketizer;
                    }

                    if (currentOutput.OutputFormat.IsCompressed)
                    {
                        AudioDecoder = currentOutput.OutputFormat.ToEncoding().CreateDecoder();

                        currentOutput.AddOutput(AudioDecoder);
                        currentOutput = AudioDecoder;
                    }

                    ResetAudioPipe = new ResetAudioPipe(currentOutput.OutputFormat);
                    currentOutput.AddOutput(ResetAudioPipe);
                    currentOutput = ResetAudioPipe;
                }

                if (!currentOutput.OutputFormat.IsCompressed)
                {
                    AudioEncoder = AudioFormat.ToEncoding().CreateEncoder();

                    AudioConverter = new SoundConverter(currentOutput.Config, AudioEncoder.InputConfig);

                    currentOutput.AddOutput(AudioConverter);
                    currentOutput = AudioConverter;

                    currentOutput.AddOutput(AudioEncoder);
                    currentOutput = AudioEncoder;
                }

                if (!currentOutput.OutputFormat.IsPacketized)
                {
                    AudioPacketizer = AudioFormat.ToEncoding().CreatePacketizer();

                    currentOutput.AddOutput(AudioPacketizer);
                    currentOutput = AudioPacketizer;
                }

                var streamInput = null as AudioPipe;
                foreach (var input in AudioStream.Inputs)
                {
                    if (input.OutputFormat.IsEquivalent(AudioFormat, true))
                    {
                        streamInput = input as AudioPipe;
                    }
                }

                currentOutput.AddOutput(streamInput);

                if (AudioEncoder != null && !AudioEncoder.OutputFormat.IsFixedBitrate && Options.AudioBitrate.HasValue)
                {
                    AudioEncoder.TargetBitrate = Options.AudioBitrate.Value;
                }

                await AudioSource.Start();
                DoStartAudioStream();
            }
        }

        private async Task StopAudioStream()
        {
            if (AudioSource != null)
            {
                DoStopAudioStream();
                await AudioSource.Stop();
                AudioSource.Destroy();
                AudioSource = null;
            }
            if (AudioDepacketizer != null)
            {
                AudioDepacketizer.Destroy();
                AudioDepacketizer = null;
            }
            if (AudioDecoder != null)
            {
                AudioDecoder.Destroy();
                AudioDecoder = null;
            }
            if (ResetAudioPipe != null)
            {
                ResetAudioPipe.Destroy();
                ResetAudioPipe = null;
            }
            if (AudioConverter != null)
            {
                AudioConverter.Destroy();
                AudioConverter = null;
            }
            if (AudioEncoder != null)
            {
                AudioEncoder.Destroy();
                AudioEncoder = null;
            }
            if (AudioPacketizer != null)
            {
                AudioPacketizer.Destroy();
                AudioPacketizer = null;
            }
        }

        private void DestroyAudioStream()
        {
            if (AudioStream != null)
            {
                DoDestroyAudioStream();
                AudioStream = null;
            }
            if (AudioPipes != null)
            {
                foreach (var pipe in AudioPipes)
                {
                    pipe.Destroy();
                }
                AudioPipes = null;
            }
        }

        protected virtual void DoInitializeAudioStream() { }
        protected virtual void DoStartAudioStream() { }
        protected virtual void DoStopAudioStream() { }
        protected virtual void DoDestroyAudioStream() { }

        #endregion

        #region Video

        private bool InitializeVideoStream()
        {
            if (Options.NoVideo)
            {
                return false;
            }

            VideoSynchronizationSource = Utility.GenerateSynchronizationSource();
            VideoPipes = Options.GetVideoEncodings().Select(videoEncoding =>
            {
                var identityVideoPipe = new IdentityVideoPipe(videoEncoding.CreateFormat(true));
                if (Options.VideoBitrate.HasValue)
                {
                    identityVideoPipe.UpdateTargetOutputBitrate(Options.VideoBitrate.Value);
                    identityVideoPipe.UpdateMaxOutputBitrate(Options.VideoBitrate.Value);
                }
                if (Options.VideoWidth.HasValue && Options.VideoHeight.HasValue)
                {
                    identityVideoPipe.UpdateTargetOutputSize(new Size(Options.VideoWidth.Value, Options.VideoHeight.Value));
                    identityVideoPipe.UpdateMaxOutputSize(new Size(Options.VideoWidth.Value, Options.VideoHeight.Value));
                }
                if (Options.VideoFrameRate.HasValue)
                {
                    identityVideoPipe.UpdateTargetOutputFrameRate(Options.VideoFrameRate.Value);
                    identityVideoPipe.UpdateMaxOutputFrameRate(Options.VideoFrameRate.Value);
                }
                return identityVideoPipe;
            }).ToArray();

            foreach (var pipe in VideoPipes)
            {
                pipe.SynchronizationSource = VideoSynchronizationSource;
            }

            VideoStream = new VideoStream(VideoPipes, null);
            DoInitializeVideoStream();
            return true;
        }

        private async Task StartVideoStream()
        {
            if (VideoStream != null)
            {
                var inputFormat = VideoStream.InputFormat;
                if (inputFormat == null)
                {
                    throw new Exception("Could not negotiate a video codec with the server.");
                }
                VideoFormat = inputFormat.Clone();

                VideoSource = CreateVideoSource();
                VideoSource.SynchronizationSource = VideoSynchronizationSource;

                var currentOutput = (IVideoOutput)VideoSource;

                if (Options.VideoTranscode)
                {
                    if (currentOutput.OutputFormat.IsPacketized)
                    {
                        VideoDepacketizer = currentOutput.OutputFormat.ToEncoding().CreateDepacketizer();

                        currentOutput.AddOutput(VideoDepacketizer);
                        currentOutput = VideoDepacketizer;
                    }

                    if (currentOutput.OutputFormat.IsCompressed)
                    {
                        VideoDecoder = currentOutput.OutputFormat.ToEncoding().CreateDecoder();

                        currentOutput.AddOutput(VideoDecoder);
                        currentOutput = VideoDecoder;
                    }

                    ResetVideoPipe = new ResetVideoPipe(currentOutput.OutputFormat);
                    currentOutput.AddOutput(ResetVideoPipe);
                    currentOutput = ResetVideoPipe;
                }

                if (!currentOutput.OutputFormat.IsCompressed)
                {
                    VideoEncoder = VideoFormat.ToEncoding().CreateEncoder();

                    VideoConverter = new ImageConverter(currentOutput.OutputFormat, VideoEncoder.InputFormat);

                    currentOutput.AddOutput(VideoConverter);
                    currentOutput = VideoConverter;

                    currentOutput.AddOutput(VideoEncoder);
                    currentOutput = VideoEncoder;
                }

                if (!currentOutput.OutputFormat.IsPacketized)
                {
                    VideoPacketizer = VideoFormat.ToEncoding().CreatePacketizer();

                    currentOutput.AddOutput(VideoPacketizer);
                    currentOutput = VideoPacketizer;
                }

                var streamInput = null as VideoPipe;
                foreach (var input in VideoStream.Inputs)
                {
                    if (input.OutputFormat.IsEquivalent(VideoFormat, true))
                    {
                        streamInput = input as VideoPipe;
                    }
                }

                currentOutput.AddOutput(streamInput);

                if (VideoEncoder != null && !VideoEncoder.OutputFormat.IsFixedBitrate && Options.VideoBitrate.HasValue)
                {
                    VideoEncoder.TargetBitrate = Options.VideoBitrate.Value;
                }

                await VideoSource.Start();
                DoStartVideoStream();
            }
        }

        private async Task StopVideoStream()
        {
            if (VideoSource != null)
            {
                DoStopVideoStream();
                await VideoSource.Stop();
                VideoSource.Destroy();
                VideoSource = null;
            }
            if (VideoDepacketizer != null)
            {
                VideoDepacketizer.Destroy();
                VideoDepacketizer = null;
            }
            if (VideoDecoder != null)
            {
                VideoDecoder.Destroy();
                VideoDecoder = null;
            }
            if (ResetVideoPipe != null)
            {
                ResetVideoPipe.Destroy();
                ResetVideoPipe = null;
            }
            if (VideoConverter != null)
            {
                VideoConverter.Destroy();
                VideoConverter = null;
            }
            if (VideoEncoder != null)
            {
                VideoEncoder.Destroy();
                VideoEncoder = null;
            }
            if (VideoPacketizer != null)
            {
                VideoPacketizer.Destroy();
                VideoPacketizer = null;
            }
        }

        private void DestroyVideoStream()
        {
            if (VideoStream != null)
            {
                DoDestroyVideoStream();
                VideoStream = null;
            }
            if (VideoPipes != null)
            {
                foreach (var pipe in VideoPipes)
                {
                    pipe.Destroy();
                }
                VideoPipes = null;
            }
        }

        protected virtual void DoInitializeVideoStream() { }
        protected virtual void DoStartVideoStream() { }
        protected virtual void DoStopVideoStream() { }
        protected virtual void DoDestroyVideoStream() { }

        #endregion

        #region Data

        private bool InitializeDataStream()
        {
            if (Options.DataChannelLabel == null)
            {
                return false;
            }

            DataChannel = new DataChannel(Options.DataChannelLabel);
            DataStream = new DataStream(DataChannel);
            DoInitializeDataStream();
            return true;
        }

        private async Task StartDataStream()
        {
            if (DataStream != null)
            {
                DataSource = CreateDataSource();
                DataSource.OnMessage += (sender, message) =>
                {
                    DataChannel.SendDataString(message);
                };
                await DataSource.Start();
                DoStartDataStream();
            }
        }

        private async Task StopDataStream()
        {
            if (DataStream != null)
            {
                DoStopDataStream();
                await DataSource.Stop();
                DataSource = null;
            }
        }

        private void DestroyDataStream()
        {
            if (DataStream != null)
            {
                DoDestroyDataStream();
                DataStream = null;
                DataChannel = null;
            }
        }

        protected virtual void DoInitializeDataStream() { }
        protected virtual void DoStartDataStream() { }
        protected virtual void DoStopDataStream() { }
        protected virtual void DoDestroyDataStream() { }

        #endregion

        protected abstract TAudioSource CreateAudioSource();

        protected abstract TVideoSource CreateVideoSource();

        protected virtual DataSource CreateDataSource()
        {
            return new DataSource();
        }

        protected virtual Task Ready()
        {
            return Task.CompletedTask;
        }

        protected virtual Task Unready()
        {
            return Task.CompletedTask;
        }
    }
}
