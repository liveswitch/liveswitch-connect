using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    internal static class FFUtility
    {
        public static Process FFmpeg(string arguments, Action<string> onOutput = null)
        {
            return Execute("ffmpeg", arguments, true, true, onOutput);
        }

        public static Process FFprobe(string arguments, Action<string> onOutput = null)
        {
            return Execute("ffprobe", arguments, false, false, onOutput);
        }

        private static Process Execute(string command, string arguments, bool useStandardError, bool logOutput, Action<string> onOutput)
        {
            // prep the process arguments
            var processStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = command,
                Arguments = arguments,
                RedirectStandardInput = true
            };

            if (useStandardError)
            {
                processStartInfo.RedirectStandardError = true;
            }
            else
            {
                processStartInfo.RedirectStandardOutput = true;
            }

            // log what we're about to do
            Console.Error.WriteLine();
            Console.Error.WriteLine($"{processStartInfo.FileName} {processStartInfo.Arguments}");

            try
            {
                // let 'er rip
                var process = Process.Start(processStartInfo);

                Task.Run(async () =>
                {
                    // process each line
                    var stream = useStandardError ? process.StandardError : process.StandardOutput;
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        if (line != null)
                        {
                            if (logOutput)
                            {
                                Console.Error.WriteLine(line);
                            }
                            if (onOutput != null)
                            {
                                onOutput(line);
                            }
                        }
                    }
                });

                return process;
            }
            catch (Win32Exception wex)
            {
                throw new Exception($"Could not start {command}. Is ffmpeg installed and available on your PATH?", wex);
            }
        }
    }
}
