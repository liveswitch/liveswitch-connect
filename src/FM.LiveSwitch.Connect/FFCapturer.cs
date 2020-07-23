using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFCapturer : Sender<FFCaptureOptions, AudioSource, VideoSource>
    {
        public FFCapturer(FFCaptureOptions options)
            : base(options)
        { }

        private static string ShortId()
        {
            return Guid.NewGuid().ToString().Replace("-","").Substring(0, 8);
        }

        public Task<int> Capture()
        {
            if (!Options.NoAudio)
            {
                if (Options.AudioMode == FFCaptureMode.NoEncode && Options.AudioCodec == AudioCodec.Any)
                {
                    Console.Error.WriteLine("--audio-codec must be set (not 'any') if --audio-mode is 'noencode'.");
                    return Task.FromResult(1);
                }
            }
            if (!Options.NoVideo)
            {
                if (Options.VideoMode == FFCaptureMode.NoEncode && Options.VideoCodec == VideoCodec.Any)
                {
                    Console.Error.WriteLine("--video-codec must be set (not 'any') if --video-mode is 'noencode'.");
                    return Task.FromResult(1);
                }
            }
            return Send();
        }

        protected override AudioSource CreateAudioSource()
        {
            if (Options.AudioMode == FFCaptureMode.LSEncode)
            {
                var source = new PcmNamedPipeAudioSource($"ffcapture_pcm_{ShortId()}");
                source.OnPipeConnected += () =>
                {
                    Console.Error.WriteLine("Video pipe connected.");
                };
                return source;
            }
            else
            {
                return new RtpAudioSource(AudioFormat);
            }
        }

        protected override VideoSource CreateVideoSource()
        {
            if (Options.VideoMode == FFCaptureMode.LSEncode)
            {
                var source = new Yuv4MpegNamedPipeVideoSource($"ffcapture_i420_{ShortId()}");
                source.OnPipeConnected += () =>
                {
                    Console.Error.WriteLine("Video pipe connected.");
                };
                return source;
            }
            else
            {
                return new RtpVideoSource(VideoFormat);
            }
        }

        private Process FFmpeg;
        private const int G722PacketSize = 320 + 12;
        private const int PcmuPacketSize = 320 + 12;
        private const int PcmaPacketSize = 320 + 12;
        private const int Vp8PacketSize = 1000 + 12;
        private const int Vp9PacketSize = 1000 + 12;
        private const int H264PacketSize = 1000 + 12;
        private const int H265PacketSize = 1000 + 12;

        protected override Task Ready()
        {
            var ready = base.Ready();

            var args = new List<string>
            {
                "-y",
                Options.InputArgs
            };

            if (AudioSource != null)
            {
                var config = AudioSource.Config;
                if (Options.AudioMode == FFCaptureMode.LSEncode)
                {
                    var source = AudioSource as PcmNamedPipeAudioSource;
                    args.AddRange(new[]
                    {
                        $"-map 0:a:0",
                        $"-f s16le",
                        $"-ar {config.ClockRate}",
                        $"-ac {config.ChannelCount}",
                        NamedPipe.GetOSPipeName(source.PipeName)
                    });
                }
                else
                {
                    var source = AudioSource as RtpAudioSource;
                    if (Options.AudioMode == FFCaptureMode.NoEncode)
                    {
                        if (AudioFormat.IsOpus)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}"
                            });
                        }
                        else if (AudioFormat.IsG722)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={G722PacketSize}"
                            });
                        }
                        else if (AudioFormat.IsPcmu)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={PcmuPacketSize}"
                            });
                        }
                        else if (AudioFormat.IsPcma)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={PcmaPacketSize}"
                            });
                        }
                        else
                        {
                            throw new Exception("Unknown audio format.");
                        }
                    }
                    else
                    {
                        if (AudioFormat.IsOpus)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-ar {config.ClockRate}",
                                $"-ac {config.ChannelCount}",
                                $"-c libopus",
                                $"-b:a {Options.AudioBitrate}k",
                                $"rtp://127.0.0.1:{source.Port}"
                            });
                        }
                        else if (AudioFormat.IsG722)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-ar 16000",
                                $"-ac 1",
                                $"-c g722",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={G722PacketSize}"
                            });
                        }
                        else if (AudioFormat.IsPcmu)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-ar 8000",
                                $"-ac 1",
                                $"-c pcm_mulaw",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={PcmuPacketSize}"
                            });
                        }
                        else if (AudioFormat.IsPcma)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:a:0",
                                $"-f rtp",
                                $"-ar 8000",
                                $"-ac 1",
                                $"-c pcm_alaw",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={PcmaPacketSize}"
                            });
                        }
                        else
                        {
                            throw new Exception("Unknown audio format.");
                        }
                    }
                }
            }

            if (VideoSource != null)
            {
                if (Options.VideoMode == FFCaptureMode.LSEncode)
                {
                    var source = VideoSource as Yuv4MpegNamedPipeVideoSource;
                    args.AddRange(new[]
                    {
                        $"-map 0:v:0",
                        $"-f yuv4mpegpipe",
                        $"-pix_fmt yuv420p",
                        NamedPipe.GetOSPipeName(source.PipeName)
                    });
                }
                else
                {
                    var source = VideoSource as RtpVideoSource;
                    if (Options.VideoMode == FFCaptureMode.NoEncode)
                    {
                        if (VideoFormat.IsVp8)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={Vp8PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsVp9)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={Vp9PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsH264)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={H264PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsH265)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c copy",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={H265PacketSize}"
                            });
                        }
                        else
                        {
                            throw new Exception("Unknown video format.");
                        }
                    }
                    else
                    {
                        if (VideoFormat.IsVp8)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c libvpx -auto-alt-ref 0",
                                $"-pix_fmt yuv420p",
                                $"-quality realtime",
                                $"-speed 16",
                                $"-crf 10",
                                $"-b:v {Options.VideoBitrate}k",
                                $"-g {Options.FFEncodeKeyFrameInterval}",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={Vp8PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsVp9)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c libvpx-vp9 -strict experimental",
                                $"-level 0",
                                $"-pix_fmt yuv420p",
                                $"-lag-in-frames 0",
                                $"-deadline realtime",
                                $"-quality realtime",
                                $"-speed 16",
                                $"-b:v {Options.VideoBitrate}k -maxrate {Options.VideoBitrate}k",
                                $"-g {Options.FFEncodeKeyFrameInterval}",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={Vp9PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsH264)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c libx264",
                                $"-profile:v baseline",
                                $"-level:v 1.3",
                                $"-pix_fmt yuv420p",
                                $"-tune zerolatency",
                                $"-b:v {Options.VideoBitrate}k",
                                $"-g {Options.FFEncodeKeyFrameInterval} -keyint_min {Options.FFEncodeKeyFrameInterval}",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={H264PacketSize}"
                            });
                        }
                        else if (VideoFormat.IsH265)
                        {
                            args.AddRange(new[]
                            {
                                $"-map 0:v:0",
                                $"-f rtp",
                                $"-c libx265",
                                $"-profile:v baseline",
                                $"-level:v 1.3",
                                $"-pix_fmt yuv420p",
                                $"-tune zerolatency",
                                $"-b:v {Options.VideoBitrate}k",
                                $"-g {Options.FFEncodeKeyFrameInterval} -keyint_min {Options.FFEncodeKeyFrameInterval}",
                                $"rtp://127.0.0.1:{source.Port}?pkt_size={H265PacketSize}"
                            });
                        }
                        else
                        {
                            throw new Exception("Unknown video format.");
                        }
                    }
                }
            }

            FFmpeg = FFUtility.FFmpeg(string.Join(" ", args));

            return ready;
        }

        protected override Task Unready()
        {
            if (FFmpeg != null)
            {
                FFmpeg.StandardInput.Write('q');
                FFmpeg.WaitForExit();
            }

            return base.Unready();
        }
    }
}
