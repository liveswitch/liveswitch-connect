using FM.LiveSwitch;
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
                var client = Options.CreateClient();

                Console.Error.WriteLine($"{GetType().Name} client '{client.Id}' created:{Environment.NewLine}{Descriptor.Format(client.GetDescriptors())}");

                await client.Register(Options);
                try
                {
                    var channel = await client.Join(Options);
                    try
                    {
                        // wait up to 15 seconds to connect
                        var timeout = Task.Delay(15000);
                        var audioSinks = (TAudioSink[])null;
                        var videoSinks = (TVideoSink[])null;
                        var connection = (ManagedConnection)null;
                        var connectedSource = new TaskCompletionSource<bool>();
                        var connected = connectedSource.Task;
                        var disconnected = (Task)null;

                        var connect = new Func<ConnectionInfo, Task>(async (remoteConnectionInfo) =>
                        {
                            var audioStream = CreateAudioStream(remoteConnectionInfo);
                            var videoStream = CreateVideoStream(remoteConnectionInfo);
                            var dataStream = CreateDataStream(remoteConnectionInfo);
                            connection = Options.CreateConnection(channel, remoteConnectionInfo, audioStream, videoStream, dataStream);

                            Console.Error.WriteLine($"{GetType().Name} connection '{connection.Id}' created:{Environment.NewLine}{Descriptor.Format(connection.GetDescriptors())}");

                            audioSinks = audioStream?.RemoteTrack.Sinks.Select(x => x as TAudioSink).ToArray();
                            videoSinks = videoStream?.RemoteTrack.Sinks.Select(x => x as TVideoSink).ToArray();

                            SinksReady(audioSinks, videoSinks);

                            disconnected = await connection.Connect();

                            connectedSource.TrySetResult(true);
                        });

                        if (Options.ConnectionId == "mcu")
                        {
                            _ = Task.Run(async () =>
                            {
                                await connect(GetMcuConnectionInfo());
                            });
                        }
                        else
                        {
                            var notifiedSource = new TaskCompletionSource<bool>();
                            var notified = notifiedSource.Task;
                            channel.OnRemoteUpstreamConnectionOpen += async (remoteConnectionInfo) =>
                            {
                                if (remoteConnectionInfo.Id == Options.ConnectionId)
                                {
                                    notifiedSource.TrySetResult(true);

                                    var remoteClientInfo = channel.GetRemoteClientInfo(remoteConnectionInfo.ClientId);
                                    if (remoteClientInfo != null)
                                    {
                                        Console.Error.WriteLine($"Remote client '{remoteConnectionInfo.ClientId}' found:{Environment.NewLine}{Descriptor.Format(remoteClientInfo.GetDescriptors())}");
                                    }

                                    Console.Error.WriteLine($"Remote connection '{Options.ConnectionId}' found:{Environment.NewLine}{Descriptor.Format(remoteConnectionInfo.GetDescriptors())}");

                                    await connect(remoteConnectionInfo);
                                }
                            };
                            channel.OnRemoteUpstreamConnectionClose += (remoteConnectionInfo) =>
                            {
                                if (remoteConnectionInfo.RemoteConnectionId == Options.ConnectionId)
                                {
                                    Console.Error.WriteLine("Remote disconnected.");
                                }
                            };

                            Console.Error.WriteLine($"Waiting for remote connection...");

                            if (await Task.WhenAny(timeout, notified) == timeout)
                            {
                                Console.Error.WriteLine($"Remote connection '{Options.ConnectionId}' does not exist.");
                                return 1;
                            }
                        }

                        Console.Error.WriteLine($"Connecting...");

                        if (await Task.WhenAny(timeout, connected) == timeout)
                        {
                            Console.Error.WriteLine($"Timeout connecting to '{Options.ConnectionId}'.");
                            return 1;
                        }

                        // watch for exit signal
                        var exitSignalledSource = new TaskCompletionSource<bool>();
                        var exitSignalled = exitSignalledSource.Task;
                        Console.CancelKeyPress += async (sender, e) =>
                        {
                            Console.Error.WriteLine("Exit signalled.");

                            e.Cancel = true;

                            SinksUnready(audioSinks, videoSinks);

                            // close connections gracefully
                            await connection.Disconnect();

                            exitSignalledSource.TrySetResult(true); // exit handling
                        };

                        // wait for exit signal
                        Console.Error.WriteLine("Waiting for exit signal or remote disconnect...");
                        await Task.WhenAny(exitSignalled, disconnected);
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

        protected abstract AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo);

        protected abstract VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo);

        protected DataStream CreateDataStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.DataChannelLabel == null || !remoteConnectionInfo.HasData)
            {
                return null;
            }

            return new DataStream(new DataChannel(Options.DataChannelLabel)
            {
                OnReceive = (args) =>
                {
                    if (args.DataString != null)
                    {
                        Console.WriteLine(args.DataString);
                    }
                    else
                    {
                        Console.Error.WriteLine($"Cannot write binary message from data channel: {args.DataBytes.ToHexString()}");
                    }
                }
            });
        }

        protected virtual void SinksReady(TAudioSink[] audioSinks, TVideoSink[] videoSinks) { }

        protected virtual void SinksUnready(TAudioSink[] audioSinks, TVideoSink[] videoSinks) { }
    }
}
