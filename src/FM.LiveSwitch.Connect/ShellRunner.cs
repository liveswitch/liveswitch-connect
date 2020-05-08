using CommandLine;
using CommandLine.Text;
using FM.LiveSwitch.Connect.Shell;
using System;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class ShellRunner
    {
        public async Task<int> Run(ShellOptions options)
        {
            try
            {
                var shellManager = new ShellManager(options);

                // watch for exit signal
                var exitSignalledSource = new TaskCompletionSource<bool>();
                var exitSignalled = exitSignalledSource.Task;
                Console.CancelKeyPress += (sender, e) =>
                {
                    Console.Error.WriteLine("Exit signalled.");

                    e.Cancel = true;

                    exitSignalledSource.TrySetResult(true); // exit handling
                };

                while (!exitSignalled.IsCompletedSuccessfully && shellManager.Depth >= 0)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            Console.Write($"{shellManager.CurrentLabel}> ");
                            var command = Console.ReadLine();
                            ProcessArgs(command.Split(" "), shellManager);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Exception: {0}", ex);
                        }
                    });
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        private void ProcessArgs(string[] args, ShellManager shellManager)
        {
            using var parser = new Parser((settings) =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null;
            });

            if (shellManager.Depth == 0)
            {
                var result = parser.ParseArguments<RegisterOptions, ExitOptions>(args);

                result.MapResult(
                    (RegisterOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Register(options);
                        }).GetAwaiter().GetResult();
                    },
                    (ExitOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Exit(options);
                        }).GetAwaiter().GetResult();
                    },
                    errors =>
                    {
                        var helpText = HelpText.AutoBuild(result);
                        helpText.Copyright = "Copyright (C) 2020 Frozen Mountain Software Ltd.";
                        helpText.AddEnumValuesToHelpText = true;
                        Console.Error.Write(helpText);
                        return 1;
                    });
            }
            else if (shellManager.Depth == 1)
            {
                var result = parser.ParseArguments<UnregisterOptions, JoinOptions, ExitOptions>(args);

                result.MapResult(
                    (UnregisterOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Unregister(options);
                        }).GetAwaiter().GetResult();
                    },
                    (JoinOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Join(options);
                        }).GetAwaiter().GetResult();
                    },
                    (ExitOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Exit(options);
                        }).GetAwaiter().GetResult();
                    },
                    errors =>
                    {
                        var helpText = HelpText.AutoBuild(result);
                        helpText.Copyright = "Copyright (C) 2020 Frozen Mountain Software Ltd.";
                        helpText.AddEnumValuesToHelpText = true;
                        Console.Error.Write(helpText);
                        return 1;
                    });
            }
            else if (shellManager.Depth == 2)
            {
                var result = parser.ParseArguments<LeaveOptions, ClientsOptions, ConnectionsOptions, ExitOptions>(args);

                result.MapResult(
                    (LeaveOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Leave(options);
                        }).GetAwaiter().GetResult();
                    },
                    (ClientsOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Clients(options);
                        }).GetAwaiter().GetResult();
                    },
                    (ConnectionsOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Connections(options);
                        }).GetAwaiter().GetResult();
                    },
                    (ExitOptions options) =>
                    {
                        return Task.Run(async () =>
                        {
                            return await shellManager.Exit(options);
                        }).GetAwaiter().GetResult();
                    },
                    errors =>
                    {
                        var helpText = HelpText.AutoBuild(result);
                        helpText.Copyright = "Copyright (C) 2020 Frozen Mountain Software Ltd.";
                        helpText.AddEnumValuesToHelpText = true;
                        Console.Error.Write(helpText);
                        return 1;
                    });
            }
        }
    }
}
