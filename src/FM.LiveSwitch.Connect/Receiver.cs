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
        protected async Task<int> Receive(TOptions options)
        {
            try
            {
                var client = options.CreateClient();

                Console.Error.WriteLine($"{GetType().Name} client '{client.Id}' created:{Environment.NewLine}{Descriptor.Format(client.GetDescriptors())}");

                await client.Register(options);
                try
                {
                    var channel = await client.Join(options);
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
                            var audioStream = CreateAudioStream(remoteConnectionInfo, options);
                            var videoStream = CreateVideoStream(remoteConnectionInfo, options);
                            var dataStream = CreateDataStream(remoteConnectionInfo, options);
                            connection = options.CreateConnection(channel, remoteConnectionInfo, audioStream, videoStream, dataStream);

                            Console.Error.WriteLine($"{GetType().Name} connection '{connection.Id}' created:{Environment.NewLine}{Descriptor.Format(connection.GetDescriptors())}");

                            audioSinks = audioStream?.RemoteTrack.Sinks.Select(x => x as TAudioSink).ToArray();
                            videoSinks = videoStream?.RemoteTrack.Sinks.Select(x => x as TVideoSink).ToArray();

                            SinksReady(audioSinks, videoSinks, options);

                            disconnected = await connection.Connect();

                            connectedSource.TrySetResult(true);
                        });

                        if (options.ConnectionId == "mcu")
                        {
                            _ = Task.Run(async () =>
                            {
                                await connect(GetMcuConnectionInfo(options));
                            });
                        }
                        else
                        {
                            var notifiedSource = new TaskCompletionSource<bool>();
                            var notified = notifiedSource.Task;
                            channel.OnRemoteUpstreamConnectionOpen += async (remoteConnectionInfo) =>
                            {
                                if (remoteConnectionInfo.Id == options.ConnectionId)
                                {
                                    notifiedSource.TrySetResult(true);

                                    var remoteClientInfo = channel.GetRemoteClientInfo(remoteConnectionInfo.ClientId);
                                    if (remoteClientInfo != null)
                                    {
                                        Console.Error.WriteLine($"Remote client '{remoteConnectionInfo.ClientId}' found:{Environment.NewLine}{Descriptor.Format(remoteClientInfo.GetDescriptors())}");
                                    }

                                    Console.Error.WriteLine($"Remote connection '{options.ConnectionId}' found:{Environment.NewLine}{Descriptor.Format(remoteConnectionInfo.GetDescriptors())}");

                                    await connect(remoteConnectionInfo);
                                }
                            };
                            channel.OnRemoteUpstreamConnectionClose += (remoteConnectionInfo) =>
                            {
                                if (remoteConnectionInfo.RemoteConnectionId == options.ConnectionId)
                                {
                                    Console.Error.WriteLine("Remote disconnected.");
                                }
                            };

                            Console.Error.WriteLine($"Waiting for remote connection...");

                            if (await Task.WhenAny(timeout, notified) == timeout)
                            {
                                Console.Error.WriteLine($"Remote connection '{options.ConnectionId}' does not exist.");
                                return 1;
                            }
                        }

                        Console.Error.WriteLine($"Connecting...");

                        if (await Task.WhenAny(timeout, connected) == timeout)
                        {
                            Console.Error.WriteLine($"Timeout connecting to '{options.ConnectionId}'.");
                            return 1;
                        }

                        // watch for exit signal
                        var exitSignalledSource = new TaskCompletionSource<bool>();
                        var exitSignalled = exitSignalledSource.Task;
                        Console.CancelKeyPress += async (sender, e) =>
                        {
                            Console.Error.WriteLine("Exit signalled.");

                            e.Cancel = true;

                            SinksUnready(audioSinks, videoSinks, options);

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

        private ConnectionInfo GetMcuConnectionInfo(TOptions options)
        {
            return new ConnectionInfo
            {
                ApplicationId = options.ApplicationId,
                ChannelId = options.ChannelId,
                Id = options.ConnectionId,
                Type = ConnectionType.Mcu,
                DataStream = new DataStreamInfo(),
                AudioStream = new MediaStreamInfo { Direction = StreamDirectionHelper.DirectionToString(StreamDirection.SendReceive) },
                VideoStream = new MediaStreamInfo { Direction = StreamDirectionHelper.DirectionToString(StreamDirection.SendReceive) }
            };
        }

        protected abstract AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo, TOptions options);

        protected abstract VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo, TOptions options);

        protected DataStream CreateDataStream(ConnectionInfo remoteConnectionInfo, TOptions options)
        {
            if (options.DataChannelLabel == null || !remoteConnectionInfo.HasData)
            {
                return null;
            }

            return new DataStream(new DataChannel(options.DataChannelLabel)
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

        protected virtual void SinksReady(TAudioSink[] audioSinks, TVideoSink[] videoSinks, TOptions options) { }

        protected virtual void SinksUnready(TAudioSink[] audioSinks, TVideoSink[] videoSinks, TOptions options) { }
    }
}
