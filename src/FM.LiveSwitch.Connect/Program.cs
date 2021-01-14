using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                FakeOptions,
                PlayOptions, 
                RenderOptions,
                FFRenderOptions,
                LogOptions,
                RecordOptions,
                InterceptOptions
            >(AppendEnvironmentVariables(args));

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

        private static string[] AppendEnvironmentVariables(string[] args)
        {
            if (!TryGetOptions(args.FirstOrDefault(), out var environmentVariablePrefix, out var options))
            {
                return args;
            }

            var newArgs = new List<string>(args);
            foreach (var unusedOption in FilterOptions(args, options))
            {
                var value = Environment.GetEnvironmentVariable($"{environmentVariablePrefix}_{unusedOption.LongName.ToUpperInvariant()}");
                if (value != null)
                {
                    Console.Error.WriteLine($"Environment variable discovered matching --{unusedOption.LongName} option.");
                    newArgs.Add($"--{unusedOption.LongName}={value}");
                }
            }
            return newArgs.ToArray();
        }

        private static bool TryGetOptions(string verb, out string environmentVariablePrefix, out OptionAttribute[] options)
        {
            if (verb != null && verb.StartsWith("-"))
            {
                return TryGetOptions(null, out environmentVariablePrefix, out options);
            }

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract))
            {
                var verbAttribute = type.GetCustomAttributes<VerbAttribute>().FirstOrDefault();
                if (verbAttribute != null)
                {
                    if (verb == null || verbAttribute.Name == verb)
                    {
                        environmentVariablePrefix = Assembly.GetExecutingAssembly().GetName().Name.ToUpperInvariant();
                        if (verb != null)
                        {
                            environmentVariablePrefix = $"{environmentVariablePrefix}_{verb.ToUpperInvariant()}";
                        }

                        options = type.GetProperties()
                            .Select(property => property.GetCustomAttributes<OptionAttribute>().FirstOrDefault())
                            .Where(option => option != null).ToArray();
                        return true;
                    }
                }
            }

            environmentVariablePrefix = null;
            options = null;
            return false;
        }

        private static OptionAttribute[] FilterOptions(string[] args, OptionAttribute[] options)
        {
            var usedLongNames = new HashSet<string>();
            var usedShortNames = new HashSet<string>();

            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    var longName = arg.Substring(2);
                    if (longName.Contains('='))
                    {
                        longName = longName.Substring(0, longName.IndexOf('='));
                    }
                    usedLongNames.Add(longName);
                }
                else if (arg.StartsWith("-"))
                {
                    var shortName = arg.Substring(1);
                    if (shortName.Contains('='))
                    {
                        shortName = shortName.Substring(0, shortName.IndexOf('='));
                    }
                    usedShortNames.Add(shortName);
                }
            }

            return options.Where(option => !usedLongNames.Contains(option.LongName) && !usedShortNames.Contains(option.ShortName)).ToArray();
        }
    }
}
