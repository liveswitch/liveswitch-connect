using FM.LiveSwitch;
using FM.LiveSwitch.Yuv;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    abstract class Receiver<TOptions, TAudioSink, TVideoSink>
        where TOptions : IReceiveOptions
        where TAudioSink : AudioSink
        where TVideoSink : VideoSink
    {
        public TOptions Options { get; private set; }

        protected AudioStream AudioStream { get; private set; }
        protected VideoStream VideoStream { get; private set; }
        protected DataStream DataStream { get; private set; }
        protected DataChannel DataChannel { get; private set; }

        protected AudioFormat AudioFormat { get; private set; }
        protected VideoFormat VideoFormat { get; private set; }

        protected TAudioSink AudioSink { get; private set; }
        protected TVideoSink VideoSink { get; private set; }
        protected DataSink DataSink { get; private set; }

        protected AudioPacketizer AudioPacketizer { get; private set; }
        protected VideoPacketizer VideoPacketizer { get; private set; }

        protected AudioEncoder AudioEncoder { get; private set; }
        protected VideoEncoder VideoEncoder { get; private set; }

        protected ResetAudioPipe ResetAudioPipe { get; private set; }
        protected ResetVideoPipe ResetVideoPipe { get; private set; }

        protected SoundConverter AudioConverter { get; private set; }
        protected ImageConverter VideoConverter { get; private set; }

        protected SoundReframer AudioReframer { get; private set; }

        protected AudioDecoder AudioDecoder { get; private set; }
        protected VideoDecoder VideoDecoder { get; private set; }

        protected AudioDepacketizer AudioDepacketizer { get; private set; }
        protected VideoPipe VideoDepacketizer { get; private set; }

        protected Client Client { get; private set; }
        protected Channel Channel { get; private set; }
        protected ManagedConnection Connection { get; private set; }

        protected ConnectionInfo RemoteConnectionInfo { get; private set; }

        private AudioPipe[] AudioPipes;
        private VideoPipe[] VideoPipes;

        private Task Disconnected;

        public Receiver(TOptions options)
        {
            Options = options;
        }

        protected async Task<int> Receive()
        {
            if (Options.NoAudio && Options.NoVideo)
            {
                Console.Error.WriteLine("--no-audio and --no-video cannot both be set.");
                return 1;
            }

            try
            {
                Client = Options.CreateClient();

                Console.Error.WriteLine($"{GetType().Name} client created:{Environment.NewLine}{Descriptor.Format(Client.GetDescriptors())}");

                await Client.Register(Options);
                try
                {
                    Channel = await Client.Join(Options);
                    try
                    {
                        Console.Error.WriteLine($"{GetType().Name} is waiting for connection '{Options.ConnectionId}'.");

                        RemoteConnectionInfo = await GetRemoteConnectionInfo();

                        InitializeAudioStream();
                        InitializeVideoStream();
                        InitializeDataStream();

                        Connection = Options.CreateConnection(Channel, RemoteConnectionInfo, AudioStream, VideoStream, DataStream);

                        Console.Error.WriteLine($"{GetType().Name} connection created:{Environment.NewLine}{Descriptor.Format(Connection.GetDescriptors())}");

                        Disconnected = await Connection.Connect();

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
                        Console.Error.WriteLine("Waiting for exit signal or disconnect...");
                        await Task.WhenAny(exitSignalled, Disconnected);
                    }
                    finally
                    {
                        await Client.Leave(Channel.Id);
                    }
                }
                finally
                {
                    await Client.Unregister();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        private Task<ConnectionInfo> GetRemoteConnectionInfo()
        {
            var taskCompletionSource = new TaskCompletionSource<ConnectionInfo>();

            if (Options.ConnectionId == "mcu")
            {
                taskCompletionSource.TrySetResult(GetMcuConnectionInfo());
            }
            else
            {
                Channel.OnRemoteUpstreamConnectionOpen += (remoteConnectionInfo) =>
                {
                    if (remoteConnectionInfo.Id == Options.ConnectionId)
                    {
                        taskCompletionSource.TrySetResult(remoteConnectionInfo);
                    }
                };
                foreach (var remoteConnectionInfo in Channel.RemoteUpstreamConnectionInfos)
                {
                    if (remoteConnectionInfo.Id == Options.ConnectionId)
                    {
                        taskCompletionSource.TrySetResult(remoteConnectionInfo);
                    }
                }
            }

            return taskCompletionSource.Task;
        }

        private ConnectionInfo GetMcuConnectionInfo()
        {
            return new ConnectionInfo
            {
                ApplicationId = Options.ApplicationId,
                ChannelId = Options.ChannelId,
                Id = Options.ConnectionId,
                Type = ConnectionType.Mcu,
                DataStream = new DataStreamInfo(),
                AudioStream = new MediaStreamInfo { Direction = StreamDirectionHelper.DirectionToString(StreamDirection.SendReceive) },
                VideoStream = new MediaStreamInfo { Direction = StreamDirectionHelper.DirectionToString(StreamDirection.SendReceive) }
            };
        }

        #region Audio

        protected virtual int GetAudioFrameDuration()
        {
            return -1;
        }

        private bool InitializeAudioStream()
        {
            if (Options.NoAudio || !RemoteConnectionInfo.HasAudio)
            {
                return false;
            }

            AudioPipes = Options.GetAudioEncodings().Select(audioEncoding =>
            {
                return new IdentityAudioPipe(audioEncoding.CreateFormat(true));
            }).ToArray();

            AudioStream = new AudioStream(null, AudioPipes);
            
            if (Options.AudioBitrate.HasValue)
            {
                AudioStream.LocalBandwidth = Options.AudioBitrate.Value;
            }
            
            DoInitializeAudioStream();
            return true;
        }

        private Task StartAudioStream()
        {
            if (AudioStream != null)
            {
                var outputFormat = AudioStream.OutputFormat;
                if (outputFormat == null)
                {
                    throw new Exception("Could not negotiate an audio codec with the server.");
                }
                AudioFormat = outputFormat.Clone();

                AudioSink = CreateAudioSink();

                var currentInput = (IAudioInput)AudioSink;

                if (Options.AudioTranscode)
                {
                    if (currentInput.InputFormat.IsPacketized)
                    {
                        AudioPacketizer = currentInput.InputFormat.ToEncoding().CreatePacketizer();

                        currentInput.AddInput(AudioPacketizer);
                        currentInput = AudioPacketizer;
                    }

                    if (currentInput.InputFormat.IsCompressed)
                    {
                        AudioEncoder = currentInput.InputFormat.ToEncoding().CreateEncoder();

                        currentInput.AddInput(AudioEncoder);
                        currentInput = AudioEncoder;
                    }

                    ResetAudioPipe = new ResetAudioPipe(currentInput.InputFormat);
                    currentInput.AddInput(ResetAudioPipe);
                    currentInput = ResetAudioPipe;
                }

                if (!currentInput.InputFormat.IsCompressed)
                {
                    AudioDecoder = AudioFormat.ToEncoding().CreateDecoder();

                    AudioConverter = new SoundConverter(AudioDecoder.OutputConfig, currentInput.Config);

                    var frameDuration = GetAudioFrameDuration();
                    if (frameDuration != -1)
                    {
                        AudioReframer = new SoundReframer(currentInput.Config, frameDuration);
                        currentInput.AddInput(AudioReframer);
                        currentInput = AudioReframer;
                    }

                    currentInput.AddInput(AudioConverter);
                    currentInput = AudioConverter;

                    currentInput.AddInput(AudioDecoder);
                    currentInput = AudioDecoder;
                }

                if (!currentInput.InputFormat.IsPacketized)
                {
                    AudioDepacketizer = AudioFormat.ToEncoding().CreateDepacketizer();

                    currentInput.AddInput(AudioDepacketizer);
                    currentInput = AudioDepacketizer;
                }

                var streamOutput = null as AudioPipe;
                foreach (var output in AudioStream.Outputs)
                {
                    if (output.InputFormat.IsEquivalent(AudioFormat, true))
                    {
                        streamOutput = output as AudioPipe;
                    }
                }

                currentInput.AddInput(streamOutput);

                if (AudioEncoder != null && !AudioEncoder.OutputFormat.IsFixedBitrate && Options.AudioBitrate.HasValue)
                {
                    AudioEncoder.TargetBitrate = Options.AudioBitrate.Value;
                }
            }
            return Task.CompletedTask;
        }

        private Task StopAudioStream()
        {
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
            if (AudioConverter != null)
            {
                AudioConverter.Destroy();
                AudioConverter = null;
            }
            if (ResetAudioPipe != null)
            {
                ResetAudioPipe.Destroy();
                ResetAudioPipe = null;
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
            if (AudioSink != null)
            {
                AudioSink.Destroy();
                AudioSink = null;
            }
            return Task.CompletedTask;
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
        protected virtual void DoDestroyAudioStream() { }

        #endregion

        #region Video

        private bool InitializeVideoStream()
        {
            if (Options.NoVideo || !RemoteConnectionInfo.HasVideo)
            {
                return false;
            }

            VideoPipes = Options.GetVideoEncodings().Select(videoEncoding =>
            {
                return new IdentityVideoPipe(videoEncoding.CreateFormat(true));
            }).ToArray();

            VideoStream = new VideoStream(null, VideoPipes);

            if (Options.VideoBitrate.HasValue)
            {
                VideoStream.LocalBandwidth = Options.VideoBitrate.Value;
            }

            DoInitializeVideoStream();
            return true;
        }

        private Task StartVideoStream()
        {
            if (VideoStream != null)
            {
                var outputFormat = VideoStream.OutputFormat;
                if (outputFormat == null)
                {
                    throw new Exception("Could not negotiate an video codec with the server.");
                }
                VideoFormat = outputFormat.Clone();

                VideoSink = CreateVideoSink();

                var currentInput = (IVideoInput)VideoSink;

                if (Options.VideoTranscode)
                {
                    if (currentInput.InputFormat.IsPacketized)
                    {
                        VideoPacketizer = currentInput.InputFormat.ToEncoding().CreatePacketizer();

                        currentInput.AddInput(VideoPacketizer);
                        currentInput = VideoPacketizer;
                    }

                    if (currentInput.InputFormat.IsCompressed)
                    {
                        VideoEncoder = currentInput.InputFormat.ToEncoding().CreateEncoder();

                        currentInput.AddInput(VideoEncoder);
                        currentInput = VideoEncoder;
                    }

                    ResetVideoPipe = new ResetVideoPipe(currentInput.InputFormat);
                    currentInput.AddInput(ResetVideoPipe);
                    currentInput = ResetVideoPipe;
                }

                if (!currentInput.InputFormat.IsCompressed)
                {
                    VideoDecoder = VideoFormat.ToEncoding().CreateDecoder();

                    VideoConverter = new ImageConverter(VideoDecoder.OutputFormat, currentInput.InputFormat);

                    currentInput.AddInput(VideoConverter);
                    currentInput = VideoConverter;

                    currentInput.AddInput(VideoDecoder);
                    currentInput = VideoDecoder;
                }

                if (!currentInput.InputFormat.IsPacketized)
                {
                    VideoDepacketizer = VideoFormat.ToEncoding().CreateDepacketizer();

                    currentInput.AddInput(VideoDepacketizer);
                    currentInput = VideoDepacketizer;
                }

                var streamOutput = null as VideoPipe;
                foreach (var output in VideoStream.Outputs)
                {
                    if (output.InputFormat.IsEquivalent(VideoFormat, true))
                    {
                        streamOutput = output as VideoPipe;
                    }
                }

                currentInput.AddInput(streamOutput);

                if (VideoEncoder != null && !VideoEncoder.OutputFormat.IsFixedBitrate && Options.VideoBitrate.HasValue)
                {
                    VideoEncoder.TargetBitrate = Options.VideoBitrate.Value;
                }
            }
            return Task.CompletedTask;
        }

        private Task StopVideoStream()
        {
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
            if (VideoConverter != null)
            {
                VideoConverter.Destroy();
                VideoConverter = null;
            }
            if (ResetVideoPipe != null)
            {
                ResetVideoPipe.Destroy();
                ResetVideoPipe = null;
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
            if (VideoSink != null)
            {
                VideoSink.Destroy();
                VideoSink = null;
            }
            return Task.CompletedTask;
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
        protected virtual void DoDestroyVideoStream() { }

        #endregion

        #region Data

        private bool InitializeDataStream()
        {
            if (Options.DataChannelLabel == null || !RemoteConnectionInfo.HasData)
            {
                return false;
            }

            DataChannel = new DataChannel(Options.DataChannelLabel);
            DataStream = new DataStream(DataChannel);
            DoInitializeDataStream();
            return true;
        }

        private Task StartDataStream()
        {
            if (DataStream != null)
            {
                DataSink = CreateDataSink();
                DataChannel.OnReceive = DataSink.ProcessReceive;
            }
            return Task.CompletedTask;
        }

        private Task StopDataStream()
        {
            if (DataStream != null)
            {
                DataSink = null;
            }
            return Task.CompletedTask;
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
        protected virtual void DoDestroyDataStream() { }

        #endregion

        protected abstract TAudioSink CreateAudioSink();

        protected abstract TVideoSink CreateVideoSink();

        protected virtual DataSink CreateDataSink()
        {
            return new DataSink();
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
