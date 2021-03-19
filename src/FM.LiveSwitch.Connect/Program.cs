using CommandLine;
using CommandLine.Text;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            using var parser = new Parser((settings) =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null;
            });

            var result = parser.ParseArguments<
                ShellOptions,
                CaptureOptions, 
                FFCaptureOptions,
                NdiCaptureOptions,
                FakeOptions,
                PlayOptions, 
                RenderOptions,
                FFRenderOptions,
                NdiRenderOptions,
                LogOptions,
                RecordOptions,
                InterceptOptions,
                NdiFindOptions
            >(args);

            result.MapResult(
                (ShellOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new ShellRunner(options).Run();
                    }).GetAwaiter().GetResult();
                },
                (CaptureOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Capturer(options).Capture();
                    }).GetAwaiter().GetResult();
                },
                (NdiCaptureOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new NdiCapturer(options).Capture();
                    }).GetAwaiter().GetResult();
                },
                (FFCaptureOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new FFCapturer(options).Capture();
                    }).GetAwaiter().GetResult();
                },
                (FakeOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Faker(options).Fake();
                    }).GetAwaiter().GetResult();
                },
                (PlayOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Player(options).Play();
                    }).GetAwaiter().GetResult();
                },
                (RenderOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Renderer(options).Render();
                    }).GetAwaiter().GetResult();
                },
                (NdiRenderOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new NdiRenderer(options).Render();
                    }).GetAwaiter().GetResult();
                },
                (FFRenderOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new FFRenderer(options).Render();
                    }).GetAwaiter().GetResult();
                },
                (LogOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Logger(options).Log();
                    }).GetAwaiter().GetResult();
                },
                (RecordOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Recorder(options).Record();
                    }).GetAwaiter().GetResult();
                },
                (InterceptOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        Initialize(options);
                        return await new Interceptor(options).Intercept();
                    }).GetAwaiter().GetResult();
                },
                (NdiFindOptions options) =>
                {
                    return Task.Run(async () =>
                    {
                        return await new NdiFinder(options).Run();
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

        private static void Initialize(Options options)
        {
            Console.Error.WriteLine("Checking for OpenH264...");
            options.DisableOpenH264 = true;
            try
            {
                options.DisableOpenH264 = !OpenH264.Utility.Initialize();
            }
            catch
            {
                // Do nothing. An exception indicates that the default
                // behaviour (disabling OpenH264) is correct.
            }

            if (options.DisableOpenH264)
            {
                OpenH264.Utility.DownloadOpenH264(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).GetAwaiter().GetResult();
                try
                {
                    options.DisableOpenH264 = !OpenH264.Utility.Initialize();
                    if (options.DisableOpenH264)
                    {
                        Console.Error.WriteLine("OpenH264 failed to initialize.");
                    }
                    else
                    {
                        Console.Error.WriteLine("OpenH264 initialized.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"OpenH264 failed to initialize. {ex}");
                }
            }

            Log.AddProvider(new ErrorLogProvider(options.LogLevel));
        }
    }
}
