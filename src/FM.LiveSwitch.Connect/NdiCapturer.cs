using System;
using System.Threading.Tasks;
using System.Timers;
using NewTek;
using NDI = NewTek.NDI;

namespace FM.LiveSwitch.Connect
{
    class NdiCapturer : Sender<NdiCaptureOptions, NdiAudioSource, NdiVideoSource>
    {
        public class VideoFormatNotSupportedException : Exception
        {
            public VideoFormatNotSupportedException() {}
            public VideoFormatNotSupportedException(string message)
                : base(message) {}
        }

        static ILog _Log = Log.GetLogger(typeof(NdiCapturer));

        public NdiCapturer(NdiCaptureOptions options)
            : base(options)
        {
            _NdiReceiver = new NDI.Receiver(options.StreamName, "LiveSwitchConnect");
            _NdiReceiver.IsAudioEnabled = !options.NoAudio;
            _NdiReceiver.IsVideoEnabled = !options.NoVideo;
            _PerformanceTimer = new Timer(10000);
            _PerformanceTimer.Elapsed += new ElapsedEventHandler(OnPerformanceTimer);
            ProcessVideoFormat();
        }

        public Task<int> Capture()
        {

            if (Options.AudioClockRate < 8000)
            {
                Console.Error.WriteLine("--audio-clock-rate minimum value is 8000.");
                return Task.FromResult(1);
            }
            if (Options.AudioClockRate > 48000)
            {
                Console.Error.WriteLine("--audio-clock-rate maximum value is 48000.");
                return Task.FromResult(1);
            }
            if (Options.AudioChannelCount < 1)
            {
                Console.Error.WriteLine("--audio-channel-count minimum value is 1.");
                return Task.FromResult(1);
            }
            if (Options.AudioChannelCount > 4)
            {
                Console.Error.WriteLine("--audio-channel-count maximum value is 4.");
                return Task.FromResult(1);
            }


            if (Options.VideoWidth == 0)
            {
                Console.Error.WriteLine("--video-width can't be 0. Disabling video currently not supported.");
                return Task.FromResult(1);
            }
            if (Options.VideoHeight == 0)
            {
                Console.Error.WriteLine("--video-height can't be 0. Disabling video currently not supported.");
                return Task.FromResult(1);
            }
            if (Options.VideoWidth % 2 != 0)
            {
                Console.Error.WriteLine("--video-width must be a multiple of 2.");
                return Task.FromResult(1);
            }
            if (Options.VideoHeight % 2 != 0)
            {
                Console.Error.WriteLine("--video-height must be a multiple of 2.");
                return Task.FromResult(1);
            }

            switch (Options.VideoFormat)
            {
                case ImageFormat.Rgb:
                case ImageFormat.Bgr:
                case ImageFormat.Rgba:
                case ImageFormat.Bgra:
                    // Supported
                    break;
                default:
                    // YUV formats unsupported for now. Would need to add support converting from Packed(UYVY) to Planar.
                    // Can specify UYVY to NDI on creation, but LS YUV formats are planar.
                    // That being said all of the planar formats are listed as possible VideoFrame outputs.
                    // Might be worth communicating with NDI to understand why you can't specify planar formats, but it's
                    // possible to receive them.
                    Console.Error.WriteLine("--video-format not supported");
                    return Task.FromResult(1);
            }

            return Send();
        }

        protected NDI.Receiver _NdiReceiver;
        private Timer _PerformanceTimer;

        protected override NdiAudioSource CreateAudioSource()
        {
            _Log.Info("Ndi Audio Source Created");
            var source = new NdiAudioSource(_NdiReceiver, new Pcm.Format(Options.AudioClockRate, Options.AudioChannelCount), Options.AudioClockRate, Options.AudioChannelCount);
            return source;
        }

        protected override NdiVideoSource CreateVideoSource()
        {
            _Log.Info("Ndi Video Source Created");
            var source = new NdiVideoSource(_NdiReceiver, Options.VideoFormat);
            return source;
        }

        protected override Task Ready()
        {
            _NdiReceiver.ConnectionStateChange += ProcessConnectionStateChange;
            _NdiReceiver.Connect(LsToNdiVideoFormat(Options.VideoFormat));
            _PerformanceTimer.Start();
            return base.Ready();
        }

        protected override Task Unready()
        {
            _PerformanceTimer.Stop();
            if (_NdiReceiver != null)
            {
                _NdiReceiver.ConnectionStateChange -= ProcessConnectionStateChange;
                _NdiReceiver.Disconnect();
                _NdiReceiver.Dispose();
            }
            return base.Unready();
        }

        protected NDIlib.recv_color_format_e LsToNdiVideoFormat(ImageFormat format)
        {
            switch(format)
            {
                case ImageFormat.Rgb:
                case ImageFormat.Rgba:
                    return NDIlib.recv_color_format_e.recv_color_format_RGBX_RGBA;
                case ImageFormat.Bgr:
                case ImageFormat.Bgra:
                    return NDIlib.recv_color_format_e.recv_color_format_BGRX_BGRA;
                default:
                    // YUV formats unsupported for now. Would need to add support converting from Packed(UYVY) to Planar
                    // Above formats can't be specified in NDI, but we should've failed by now.
                    throw new VideoFormatNotSupportedException("Trying to use invalid video formats.");
            }
        }

        // Need to specify a video format for NDI. Due to the options available, in testing the alpha channel was always returned.
        protected void ProcessVideoFormat()
        {
            if (Options.VideoFormat == ImageFormat.Rgb)
            {
                Options.VideoFormat = ImageFormat.Rgba;
            }
            else if (Options.VideoFormat == ImageFormat.Bgr)
            {
                Options.VideoFormat = ImageFormat.Bgra;
            }
            else
            {
                return;
            }
            _Log.Warn("Forced video format to use alpha channel.");
        }

        protected void ProcessConnectionStateChange(object sender, EventArgs e)
        {
            if (!_NdiReceiver.Connected)
            {
                _Log.Info("Connection to NDI source lost.");
            }
            else if (!_NdiReceiver.ReceivingFrames)
            {
                _Log.Info("Stopped receiving frames from NDI source.");
            }
            else
            {
                _Log.Info("Connection to NDI source established.");
            }
        }

        private void OnPerformanceTimer(object source, ElapsedEventArgs e)
        {
            if (_NdiReceiver != null)
            {
                NDI.FramesReceived fr = _NdiReceiver.GetPerformance();
                _Log.Info($"Frame metrics ~ Audio - Total: {fr.AudioTotal} Dropped: {fr.AudioDropped} | Video - Total: {fr.VideoTotal} Dropped: {fr.VideoDropped}");
            }
        }
    }
}
