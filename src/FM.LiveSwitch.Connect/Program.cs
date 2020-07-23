using CommandLine;
using CommandLine.Text;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Log.AddProvider(new ErrorLogProvider(LogLevel.Error));

            Console.Error.WriteLine("Checking for OpenH264...");
            OpenH264.Utility.DownloadOpenH264(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).GetAwaiter().GetResult();
            var disableOpenH264 = true;
            try
            {
                disableOpenH264 = !OpenH264.Utility.Initialize();
                if (disableOpenH264)
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
                Console.Error.WriteLine("OpenH264 failed to initialize.", ex);
            }

            Console.Error.WriteLine("Checking for Nvidia hardware support...");
            var disableNvidia = !Nvidia.Utility.NvencSupported;
            if (disableNvidia)
            {
                Console.Error.WriteLine("Nvidia hardware encoder/decoder support not detected.");
            }
            else
            {
                Console.Error.WriteLine("Nvidia hardware encoder/decoder is supported.");
            }

            using var parser = new Parser((settings) =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null;
            });

            var result = parser.ParseArguments<
                ShellOptions,
                CaptureOptions, 
                FFCaptureOptions,
                FakeOptions,
                PlayOptions, 
                RenderOptions,
                FFRenderOptions,
                LogOptions,
                RecordOptions,
                InterceptOptions
            >(args);

            result.MapResult(
                (ShellOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new ShellRunner(options).Run();
                    }).GetAwaiter().GetResult();
                },
                (CaptureOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Capturer(options).Capture();
                    }).GetAwaiter().GetResult();
                },
                (FFCaptureOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new FFCapturer(options).Capture();
                    }).GetAwaiter().GetResult();
                },
                (FakeOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Faker(options).Fake();
                    }).GetAwaiter().GetResult();
                },
                (PlayOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Player(options).Play();
                    }).GetAwaiter().GetResult();
                },
                (RenderOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Renderer(options).Render();
                    }).GetAwaiter().GetResult();
                },
                (FFRenderOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new FFRenderer(options).Render();
                    }).GetAwaiter().GetResult();
                },
                (LogOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Logger(options).Log();
                    }).GetAwaiter().GetResult();
                },
                (RecordOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Recorder(options).Record();
                    }).GetAwaiter().GetResult();
                },
                (InterceptOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    options.DisableNvidia = disableNvidia;
                    return Task.Run(async () =>
                    {
                        return await new Interceptor(options).Intercept();
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
