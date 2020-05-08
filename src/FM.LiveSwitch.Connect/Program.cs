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
                    return Task.Run(async () =>
                    {
                        return await new ShellRunner().Run(options);
                    }).GetAwaiter().GetResult();
                },
                (CaptureOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Capturer().Capture(options);
                    }).GetAwaiter().GetResult();
                },
                (FFCaptureOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new FFCapturer().Capture(options);
                    }).GetAwaiter().GetResult();
                },
                (FakeOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Faker().Fake(options);
                    }).GetAwaiter().GetResult();
                },
                (PlayOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Player().Play(options);
                    }).GetAwaiter().GetResult();
                },
                (RenderOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Renderer().Render(options);
                    }).GetAwaiter().GetResult();
                },
                (FFRenderOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new FFRenderer().Render(options);
                    }).GetAwaiter().GetResult();
                },
                (LogOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Logger().Log(options);
                    }).GetAwaiter().GetResult();
                },
                (RecordOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Recorder().Record(options);
                    }).GetAwaiter().GetResult();
                },
                (InterceptOptions options) =>
                {
                    options.DisableOpenH264 = disableOpenH264;
                    return Task.Run(async () =>
                    {
                        return await new Interceptor().Intercept(options);
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
