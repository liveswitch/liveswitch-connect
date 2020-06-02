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

        private AudioPipe[] AudioPipes;
        private VideoPipe[] VideoPipes;

        protected AudioStream AudioStream { get; private set; }
        protected VideoStream VideoStream { get; private set; }
        protected DataStream DataStream { get; private set; }
        protected DataChannel DataChannel { get; private set; }

        protected AudioFormat AudioFormat { get; private set; }
        protected VideoFormat VideoFormat { get; private set; }

        protected TAudioSource AudioSource { get; private set; }
        protected TVideoSource VideoSource { get; private set; }
        protected DataSource DataSource { get; private set; }

        protected SoundConverter AudioConverter { get; private set; }
        protected ImageConverter VideoConverter { get; private set; }

        protected AudioEncoder AudioEncoder { get; private set; }
        protected VideoEncoder VideoEncoder { get; private set; }

        protected AudioPacketizer AudioPacketizer { get; private set; }
        protected VideoPacketizer VideoPacketizer { get; private set; }

        protected ManagedConnection Connection { get; private set; }

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
                var client = Options.CreateClient();

                Console.Error.WriteLine($"{GetType().Name} client '{client.Id}' created:{Environment.NewLine}{Descriptor.Format(client.GetDescriptors())}");

                await client.Register(Options);
                try
                {
                    var channel = await client.Join(Options);
                    try
                    {
                        InitializeAudioStream();
                        InitializeVideoStream();
                        InitializeDataStream();

                        Connection = Options.CreateConnection(channel, AudioStream, VideoStream, DataStream);

                        Console.Error.WriteLine($"{GetType().Name} connection '{Connection.Id}' created:{Environment.NewLine}{Descriptor.Format(Connection.GetDescriptors())}");

                        await Connection.Connect();

                        await Task.WhenAll(
                            StartAudioStream(),
                            StartVideoStream(),
                            StartDataStream());

                        await Ready();

                        // watch for exit signal
                        var exitSignalledSource = new TaskCompletionSource<bool>();
                        var exitSignalled = exitSignalledSource.Task;
                        Console.CancelKeyPress += async (sender, e) =>
                        {
                            Console.Error.WriteLine("Exit signalled.");

                            e.Cancel = true;

                            await Unready();

                            await Task.WhenAll(
                                StopAudioStream(),
                                StopVideoStream(),
                                StopDataStream());

                            // close connection gracefully
                            await Connection.Disconnect();

                            DestroyAudioStream();
                            DestroyVideoStream();
                            DestroyDataStream();

                            exitSignalledSource.TrySetResult(true); // exit handling
                        };

                        // wait for exit signal
                        Console.Error.WriteLine("Waiting for exit signal...");
                        await exitSignalled;
                    }
                    finally
                    {
                        await client.Leave(channel.Id);
                    }
                }
                finally
                {
                    await client.Unregister();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        #region Audio

        private bool InitializeAudioStream()
        {
            if (Options.NoAudio)
            {
                return false;
            }

            AudioSynchronizationSource = Utility.GenerateSynchronizationSource();
            AudioPipes = Options.GetAudioCodecs().Select(x => new IdentityAudioPipe(x.CreateFormat(true))).ToArray();
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

                var pipe = null as AudioPipe;
                foreach (var input in AudioStream.Inputs)
                {
                    if (input.OutputFormat.IsEquivalent(AudioFormat, true))
                    {
                        pipe = input as AudioPipe;
                    }
                }

                AudioSource = CreateAudioSource();
                AudioSource.SynchronizationSource = AudioSynchronizationSource;
                if (AudioSource.OutputFormat.IsPacketized)
                {
                    AudioSource.AddOutput(pipe);
                }
                else
                {
                    AudioPacketizer = AudioFormat.CreateCodec().CreatePacketizer();
                    AudioPacketizer.AddOutput(pipe);

                    if (AudioSource.OutputFormat.IsCompressed)
                    {
                        AudioSource.AddOutput(AudioPacketizer);
                    }
                    else
                    {
                        AudioEncoder = AudioFormat.CreateCodec().CreateEncoder();
                        AudioEncoder.AddOutput(AudioPacketizer);

                        AudioConverter = new SoundConverter(AudioSource.Config, AudioEncoder.InputConfig);
                        AudioConverter.AddOutput(AudioEncoder);

                        AudioSource.AddOutput(AudioConverter);

                        if (!AudioEncoder.OutputFormat.IsFixedBitrate)
                        {
                            AudioEncoder.TargetBitrate = Options.AudioBitrate;
                        }
                    }
                }
                await AudioSource.Start();
                DoStartAudioStream();
            }
        }

        private async Task StopAudioStream()
        {
            if (AudioStream != null)
            {
                if (AudioSource != null)
                {
                    DoStopAudioStream();
                    await AudioSource.Stop();
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
                if (AudioSource != null)
                {
                    AudioSource.Destroy();
                    AudioSource = null;
                }
            }
        }

        private void DestroyAudioStream()
        {
            if (AudioStream != null)
            {
                DoDestroyAudioStream();
                if (AudioPipes != null)
                {
                    foreach (var pipe in AudioPipes)
                    {
                        pipe.Destroy();
                    }
                    AudioPipes = null;
                }
                AudioStream = null;
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
            VideoPipes = Options.GetVideoCodecs().Select(x => new IdentityVideoPipe(x.CreateFormat(true))).ToArray();
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

                var pipe = null as VideoPipe;
                foreach (var input in VideoStream.Inputs)
                {
                    if (input.OutputFormat.IsEquivalent(VideoFormat, true))
                    {
                        pipe = input as VideoPipe;
                    }
                }

                VideoSource = CreateVideoSource();
                VideoSource.SynchronizationSource = VideoSynchronizationSource;
                if (VideoSource.OutputFormat.IsPacketized)
                {
                    VideoSource.AddOutput(pipe);
                }
                else
                {
                    VideoPacketizer = VideoFormat.CreateCodec().CreatePacketizer();
                    VideoPacketizer.AddOutput(pipe);

                    if (VideoSource.OutputFormat.IsCompressed)
                    {
                        VideoSource.AddOutput(VideoPacketizer);
                    }
                    else
                    {
                        VideoEncoder = VideoFormat.CreateCodec().CreateEncoder();
                        VideoEncoder.AddOutput(VideoPacketizer);

                        VideoConverter = new ImageConverter(VideoSource.OutputFormat, VideoEncoder.InputFormat);
                        VideoConverter.AddOutput(VideoEncoder);

                        VideoSource.AddOutput(VideoConverter);

                        if (!VideoEncoder.OutputFormat.IsFixedBitrate)
                        {
                            VideoEncoder.TargetBitrate = Options.VideoBitrate;
                        }
                    }
                }
                await VideoSource.Start();
                DoStartVideoStream();
            }
        }

        private async Task StopVideoStream()
        {
            if (VideoStream != null)
            {
                if (VideoSource != null)
                {
                    DoStopVideoStream();
                    await VideoSource.Stop();
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
                if (VideoSource != null)
                {
                    VideoSource.Destroy();
                    VideoSource = null;
                }
            }
        }

        private void DestroyVideoStream()
        {
            if (VideoStream != null)
            {
                DoDestroyVideoStream();
                if (VideoPipes != null)
                {
                    foreach (var pipe in VideoPipes)
                    {
                        pipe.Destroy();
                    }
                    VideoPipes = null;
                }
                VideoStream = null;
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
