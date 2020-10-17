using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class FFRenderer : Receiver<FFRenderOptions, AudioSink, VideoSink>
    {
        public AudioFormat RtpAudioFormat
        {
            get
            {
                if (Options.AudioEncoding.HasValue)
                {
                    return Options.AudioEncoding.Value.CreateFormat(true);
                }
                return AudioFormat;
            }
        }

        public VideoFormat RtpVideoFormat
        {
            get
            {
                if (Options.VideoEncoding.HasValue)
                {
                    return Options.VideoEncoding.Value.CreateFormat(true);
                }
                return VideoFormat;
            }
        }

        public FFRenderer(FFRenderOptions options)
            : base(options)
        { }

        private static string ShortId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        }

        public Task<int> Render()
        {
            if (!Options.NoAudio)
            {
                if (Options.AudioMode == FFRenderMode.NoDecode && Options.AudioCodec == AudioCodec.Any)
                {
                    Console.Error.WriteLine("--audio-codec must be set (not 'any') if --audio-mode is 'nodecode'.");
                    return Task.FromResult(1);
                }
                if (Options.AudioEncoding.HasValue)
                {
                    Options.AudioTranscode = true;
                }
            }
            if (!Options.NoVideo)
            {
                if (Options.VideoMode == FFRenderMode.NoDecode && Options.VideoCodec == VideoCodec.Any)
                {
                    Console.Error.WriteLine("--video-codec must be set (not 'any') if --video-mode is 'nodecode'.");
                    return Task.FromResult(1);
                }
                if (Options.VideoEncoding.HasValue)
                {
                    Options.VideoTranscode = true;
                }
            }
            return Receive();
        }

        protected override AudioSink CreateAudioSink()
        {
            if (Options.AudioMode == FFRenderMode.LSDecode)
            {
                var sink = new PcmNamedPipeAudioSink($"ffrender_pcm_{ShortId()}");
                sink.OnPipeConnected += () =>
                {
                    Console.Error.WriteLine("Audio pipe connected.");
                };
                return sink;
            }
            else
            {
                return new RtpAudioSink(RtpAudioFormat)
                {
                    Deactivated = true
                };
            }
        }

        protected override VideoSink CreateVideoSink()
        {
            if (Options.VideoMode == FFRenderMode.LSDecode)
            {
                var sink = new Yuv4MpegNamedPipeVideoSink($"ffrender_i420_{ShortId()}");
                sink.OnPipeConnected += () =>
                {
                    Console.Error.WriteLine("Video pipe connected.");
                };
                return sink;
            }
            else
            {
                return new RtpVideoSink(RtpVideoFormat)
                {
                    Deactivated = true,
                    KeyFrameInterval = Options.KeyFrameInterval
                };
            }
        }

        private Process FFmpeg;
        private volatile bool _Done;
        private string AudioSdpFileName;
        private string VideoSdpFileName;

        protected override async Task Ready()
        {
            await base.Ready();

            var args = new List<string>
            {
                "-y"
            };

            if (AudioSink != null)
            {
                var config = AudioSink.Config;

                if (Options.AudioMode == FFRenderMode.LSDecode)
                {
                    var sink = AudioSink as PcmNamedPipeAudioSink;

                    args.AddRange(new[]
                    {
                        $"-guess_layout_max 0",
                        $"-f s16le",
                        $"-ar {config.ClockRate}",
                        $"-ac {config.ChannelCount}",
                        $"-i {NamedPipe.GetOSPipeName(sink.PipeName)}",
                    });
                }
                else
                {
                    var sink = AudioSink as RtpAudioSink;

                    sink.IPAddress = "127.0.0.1";
                    sink.Port = LockedRandomizer.Next(49162, 65536);
                    sink.PayloadType = 96;

                    var sdpMediaDescription = new Sdp.MediaDescription(new Sdp.Media(Sdp.MediaType.Audio, sink.Port, Sdp.Rtp.Media.RtpAvpTransportProtocol, sink.PayloadType.ToString()));
                    sdpMediaDescription.AddMediaAttribute(new Sdp.SendReceiveAttribute());

                    if (RtpAudioFormat.IsOpus)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, AudioFormat.OpusName, Opus.Format.DefaultClockRate, Opus.Format.DefaultChannelCount.ToString()));
                        sdpMediaDescription.AddMediaAttribute(new Sdp.FormatParametersAttribute(sink.PayloadType, "useinbandfec=1"));
                    }
                    else if (RtpAudioFormat.IsG722)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, AudioFormat.G722Name, G722.Format.DefaultClockRate, G722.Format.DefaultChannelCount.ToString()));
                    }
                    else if (RtpAudioFormat.IsPcmu)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, AudioFormat.PcmuName, G711.Format.DefaultClockRate, G711.Format.DefaultChannelCount.ToString()));
                    }
                    else if (RtpAudioFormat.IsPcma)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, AudioFormat.PcmaName, G711.Format.DefaultClockRate, G711.Format.DefaultChannelCount.ToString()));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unexpected audio format '{RtpAudioFormat.Name}'.");
                    }

                    if (Options.AudioBitrate.HasValue && !RtpAudioFormat.IsFixedBitrate)
                    {
                        sdpMediaDescription.AddBandwidth(new Sdp.Bandwidth(Sdp.BandwidthType.ApplicationSpecific, Options.AudioBitrate.Value));
                    }

                    var sdpMessage = new Sdp.Message(new Sdp.Origin("127.0.0.1"), "liveswitch-audio")
                    {
                        ConnectionData = new Sdp.ConnectionData("127.0.0.1")
                    };
                    if (sdpMediaDescription != null)
                    {
                        sdpMessage.AddMediaDescription(sdpMediaDescription);
                    }

                    var sdp = sdpMessage.ToString();

                    AudioSdpFileName = $"audio_{Utility.GenerateId()}.sdp";

                    File.WriteAllText(AudioSdpFileName, sdp);

                    Console.Error.WriteLine($"Audio SDP:{Environment.NewLine}{sdp}");

                    args.AddRange(new[]
                    {
                        $"-protocol_whitelist file,crypto,udp,rtp",
                        $"-i {AudioSdpFileName}"
                    });
                }
            }

            if (VideoSink != null)
            {
                if (Options.VideoMode == FFRenderMode.LSDecode)
                {
                    var sink = VideoSink as Yuv4MpegNamedPipeVideoSink;

                    args.AddRange(new[]
                    {
                        $"-f yuv4mpegpipe",
                        $"-i {NamedPipe.GetOSPipeName(sink.PipeName)}",
                    });
                }
                else
                {
                    var sink = VideoSink as RtpVideoSink;

                    sink.IPAddress = "127.0.0.1";
                    sink.Port = LockedRandomizer.Next(49162, 65536);
                    sink.PayloadType = 97;

                    var sdpMediaDescription = new Sdp.MediaDescription(new Sdp.Media(Sdp.MediaType.Video, sink.Port, Sdp.Rtp.Media.RtpAvpTransportProtocol, sink.PayloadType.ToString()));
                    sdpMediaDescription.AddMediaAttribute(new Sdp.SendReceiveAttribute());
                    if (Options.VideoFrameRate.HasValue)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.FrameRateAttribute(Options.VideoFrameRate.ToString()));
                    }

                    if (RtpVideoFormat.IsVp8)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, VideoFormat.Vp8Name, VideoFormat.DefaultClockRate));
                    }
                    else if (RtpVideoFormat.IsVp9)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, VideoFormat.Vp9Name, VideoFormat.DefaultClockRate));
                    }
                    else if (RtpVideoFormat.IsH264)
                    {
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, VideoFormat.H264Name, VideoFormat.DefaultClockRate));
                        sdpMediaDescription.AddMediaAttribute(new Sdp.FormatParametersAttribute(sink.PayloadType, "profile-level-id=42001f;level-asymmetry-allowed=1;packetization-mode=1"));
                    }
                    else if (RtpVideoFormat.IsH265)
                    {
                        //TODO: review these settings
                        sdpMediaDescription.AddMediaAttribute(new Sdp.Rtp.MapAttribute(sink.PayloadType, VideoFormat.H265Name, VideoFormat.DefaultClockRate));
                        sdpMediaDescription.AddMediaAttribute(new Sdp.FormatParametersAttribute(sink.PayloadType, "profile-level-id=42001f;level-asymmetry-allowed=1;packetization-mode=1"));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unexpected video format '{RtpVideoFormat.Name}'.");
                    }

                    if (Options.VideoBitrate.HasValue && !RtpVideoFormat.IsFixedBitrate)
                    {
                        sdpMediaDescription.AddBandwidth(new Sdp.Bandwidth(Sdp.BandwidthType.ApplicationSpecific, Options.VideoBitrate.Value));
                    }

                    var sdpMessage = new Sdp.Message(new Sdp.Origin("127.0.0.1"), "liveswitch-video")
                    {
                        ConnectionData = new Sdp.ConnectionData("127.0.0.1")
                    };
                    if (sdpMediaDescription != null)
                    {
                        sdpMessage.AddMediaDescription(sdpMediaDescription);
                    }

                    var sdp = sdpMessage.ToString();

                    VideoSdpFileName = $"video_{Utility.GenerateId()}.sdp";

                    File.WriteAllText(VideoSdpFileName, sdp);

                    Console.Error.WriteLine($"Video SDP:{Environment.NewLine}{sdp}");

                    if (Options.VideoFrameRate.HasValue)
                    {
                        args.Add($"-r {Options.VideoFrameRate}");
                    }

                    args.AddRange(new[]
                    {
                        $"-protocol_whitelist file,crypto,udp,rtp",
                        $"-i {VideoSdpFileName}",
                    });
                }
            }

            args.Add(Options.OutputArgs);

            FFmpeg = FFUtility.FFmpeg(string.Join(" ", args), ProcessFFmpegOutput);
            Activate();

            var monitor = new Thread(() =>
            {
                while (!_Done)
                {
                    FFmpeg.WaitForExit();
                    Deactivate();
                    if (!_Done)
                    {
                        Console.Error.WriteLine("FFmpeg exited unexpectedly.");
                        FFmpeg = FFUtility.FFmpeg(string.Join(" ", args), ProcessFFmpegOutput);
                        Activate();
                    }
                }
            })
            {
                IsBackground = true
            };
            monitor.Start();
        }

        private void ProcessFFmpegOutput(string line)
        {
            var restartFFmpeg = false;
            if (AudioSink != null && Options.AudioMode == FFRenderMode.NoDecode)
            {
                if (line.Contains(".sdp: Unknown error") || // input media has triggered an unknown error
                    line.Contains(".sdp: Invalid data"))    // input media is not appreciated
                {
                    restartFFmpeg = true;
                }
            }

            if (VideoSink != null && Options.VideoMode == FFRenderMode.NoDecode)
            {
                if (line.Contains(".sdp: Unknown error") || // input media has triggered an unknown error
                    line.Contains(".sdp: Invalid data"))    // input media is not appreciated
                {
                    restartFFmpeg = true;
                }
            }

            // signal exit so we can start again
            if (restartFFmpeg)
            {
                FFmpeg.StandardInput.Write('q');
            }
        }

        protected override Task Unready()
        {
            _Done = true;

            Deactivate();

            if (FFmpeg != null)
            {
                FFmpeg.StandardInput.Write('q');
                FFmpeg.WaitForExit();
            }

            if (AudioSdpFileName != null)
            {
                try
                {
                    File.Delete(AudioSdpFileName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Audio SDP input file could not be deleted. {ex}");
                }
            }

            if (VideoSdpFileName != null)
            {
                try
                {
                    File.Delete(VideoSdpFileName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Video SDP input file could not be deleted. {ex}");
                }
            }

            return base.Unready();
        }

        private void Deactivate()
        {
            if (AudioSink != null)
            {
                AudioSink.Deactivated = true;
            }

            if (VideoSink != null)
            {
                VideoSink.Deactivated = true;
            }
        }

        private void Activate()
        {
            if (AudioSink != null)
            {
                AudioSink.Deactivated = false;
            }

            if (VideoSink != null)
            {
                VideoSink.Deactivated = false;
            }
        }
    }
}
