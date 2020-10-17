using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect.Shell
{
    class ShellManager
    {
        public ShellOptions Options { get; private set; }

        public int Depth { get; private set; }

        public string CurrentLabel
        {
            get
            {
                var parts = new List<string> { "lsconnect" };

                if (_Client != null)
                {
                    parts.Add(_Client.Id);
                }

                if (_Channel != null)
                {
                    parts.Add(_Channel.Id);
                }

                return string.Join("> ", parts);
            }
        }

        private Client _Client;

        private Channel _Channel;

        public ShellManager(ShellOptions options)
        {
            Options = options;
        }

        public async Task<int> Register(RegisterOptions options)
        {
            options.GatewayUrl = Options.GatewayUrl;
            options.ApplicationId = Options.ApplicationId;
            options.SharedSecret = Options.SharedSecret;

            var client = options.CreateClient(false);

            await client.Register(options);

            _Client = client;

            Depth++;

            return 0;
        }

        public async Task<int> Unregister(UnregisterOptions options)
        {
            await _Client.Unregister();

            _Client = null;

            Depth--;

            return 0;
        }

        public async Task<int> Join(JoinOptions options)
        {
            options.SharedSecret = Options.SharedSecret;

            var channel = await _Client.Join(options);

            _Channel = channel;

            Depth++;

            return 0;
        }

        public async Task<int> Leave(LeaveOptions options)
        {
            var channelId = _Channel.Id;

            await _Client.Leave(channelId);

            _Channel = null;

            Depth--;

            return 0;
        }

        public async Task<int> Clients(ClientsOptions options)
        {
            if (options.Listen)
            {
                Console.Error.WriteLine("Press Q to stop listening.");
            }
            foreach (var remoteClientInfo in _Channel.RemoteClientInfos)
            {
                if (options.Ids)
                {
                    Console.WriteLine(remoteClientInfo.Id);
                }
                else
                {
                    Console.WriteLine($"Remote client:{Environment.NewLine}{Descriptor.Format(remoteClientInfo.GetDescriptors())}");
                }
            }
            if (options.Listen)
            {
                var onRemoteClientJoin = new Action1<ClientInfo>((remoteClientInfo) =>
                {
                    if (options.Ids)
                    {
                        Console.WriteLine($"+ {remoteClientInfo.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Remote client joined:{Environment.NewLine}{Descriptor.Format(remoteClientInfo.GetDescriptors())}");
                    }
                });

                var onRemoteClientLeave = new Action1<ClientInfo>((remoteClientInfo) =>
                {
                    if (options.Ids)
                    {
                        Console.WriteLine($"- {remoteClientInfo.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Remote client left:{Environment.NewLine}{Descriptor.Format(remoteClientInfo.GetDescriptors())}");
                    }
                });

                _Channel.OnRemoteClientJoin += onRemoteClientJoin;
                _Channel.OnRemoteClientLeave += onRemoteClientLeave;

                await Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            break;
                        }
                    }
                }).ConfigureAwait(false);

                _Channel.OnRemoteClientJoin -= onRemoteClientJoin;
                _Channel.OnRemoteClientLeave -= onRemoteClientLeave;
            }
            return 0;
        }

        public async Task<int> Connections(ConnectionsOptions options)
        {
            if (options.Listen)
            {
                Console.Error.WriteLine("Press Q to stop listening.");
            }
            foreach (var remoteUpstreamConnectionInfo in _Channel.RemoteUpstreamConnectionInfos)
            {
                if (options.Ids)
                {
                    Console.WriteLine(remoteUpstreamConnectionInfo.Id);
                }
                else
                {
                    Console.WriteLine($"Remote upstream connection:{Environment.NewLine}{Descriptor.Format(remoteUpstreamConnectionInfo.GetDescriptors(true))}");
                }
            }
            if (options.Listen)
            {
                var onRemoteUpstreamConnectionOpen = new Action1<ConnectionInfo>((remoteUpstreamConnectionInfo) =>
                {
                    if (options.Ids)
                    {
                        Console.WriteLine($"+ {remoteUpstreamConnectionInfo.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Remote upstream connection opened:{Environment.NewLine}{Descriptor.Format(remoteUpstreamConnectionInfo.GetDescriptors(true))}");
                    }
                });

                var onRemoteUpstreamConnectionClose = new Action1<ConnectionInfo>((remoteUpstreamConnectionInfo) =>
                {
                    if (options.Ids)
                    {
                        Console.WriteLine($"- {remoteUpstreamConnectionInfo.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Remote upstream connection closed:{Environment.NewLine}{Descriptor.Format(remoteUpstreamConnectionInfo.GetDescriptors(true))}");
                    }
                });

                _Channel.OnRemoteUpstreamConnectionOpen += onRemoteUpstreamConnectionOpen;
                _Channel.OnRemoteUpstreamConnectionClose += onRemoteUpstreamConnectionClose;

                await Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            break;
                        }
                    }
                });

                _Channel.OnRemoteUpstreamConnectionOpen -= onRemoteUpstreamConnectionOpen;
                _Channel.OnRemoteUpstreamConnectionClose -= onRemoteUpstreamConnectionClose;
            }
            return 0;
        }

        public async Task<int> Exit(ExitOptions options)
        {
            if (_Channel != null)
            {
                await _Client.Leave(_Channel.Id);

                _Channel = null;
            }

            if (_Client != null)
            {
                await _Client.Unregister();

                _Client = null;
            }

            Depth = -1;

            return 0;
        }
    }
}
