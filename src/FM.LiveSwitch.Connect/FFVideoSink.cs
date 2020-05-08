namespace FM.LiveSwitch.Connect
{
    class FFVideoSink : VideoSink
    {
        static ILog _Log = Log.GetLogger(typeof(FFVideoSink));

        public override string Label
        {
            get { return "FFmpeg Video Sink"; }
        }

        public string PipeName { get; private set; }

        public event Action0 OnPipeConnected;

        private readonly NamedPipe _Pipe;

        private int _HeaderWidth;
        private int _HeaderHeight;

        public FFVideoSink(string pipeName)
            : base(VideoFormat.I420)
        {
            PipeName = pipeName;

            Deactivated = true;

            _Pipe = new NamedPipe(pipeName, true);
            _Pipe.OnConnected += () =>
            {
                Deactivated = false;
                OnPipeConnected?.Invoke();
            };

            _ = _Pipe.TryAccept();
        }

        private volatile bool _StreamHeaderSent = false;

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            if (_Pipe.IsConnected)
            {
                if (!_StreamHeaderSent)
                {
                    if (!(_StreamHeaderSent = WriteYuv4MpegStreamHeader(inputBuffer)))
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send video stream header.");
                        }
                    }
                }

                if (_StreamHeaderSent)
                {
                    if (WriteYuv4MpegFrameHeader(inputBuffer))
                    {
                        var width = inputBuffer.Width;
                        var height = inputBuffer.Height;
                        var strides = inputBuffer.Strides;
                        var dataBuffers = inputBuffer.DataBuffers;

                        var yStride = 0;
                        var yDataBuffer = (DataBuffer)null;

                        var uvStride = 0;
                        var uvDataBuffer = (DataBuffer)null;

                        var uStride = 0;
                        var uDataBuffer = (DataBuffer)null;

                        var vStride = 0;
                        var vDataBuffer = (DataBuffer)null;

                        var width_2 = width / 2;
                        var height_2 = height / 2;

                        if (dataBuffers.Length == 1)
                        {
                            var yuvStride = strides[0];
                            var yuvDataBuffer = dataBuffers[0];

                            var offset = 0;

                            yStride = yuvStride;

                            var yDataBufferLength = yStride * height;
                            yDataBuffer = yuvDataBuffer.Subset(offset, yDataBufferLength);
                            offset += yDataBufferLength;


                            if (InputFormat.IsNv12 || InputFormat.IsNv21)
                            {
                                uvStride = yuvStride;

                                var uvDataBufferLength = uvStride * height_2;
                                uvDataBuffer = yuvDataBuffer.Subset(offset, uvDataBufferLength);
                            }
                            else
                            {
                                uStride = yuvStride / 2;

                                var uDataBufferLength = uStride * height_2;
                                uDataBuffer = yuvDataBuffer.Subset(offset, uDataBufferLength);
                                offset += uDataBufferLength;

                                vStride = uStride;

                                var vDataBufferLength = uDataBufferLength;
                                vDataBuffer = yuvDataBuffer.Subset(offset, vDataBufferLength);
                            }
                        }
                        else if (dataBuffers.Length == 2)
                        {
                            yStride = strides[0];
                            yDataBuffer = dataBuffers[0];

                            uvStride = strides[1];
                            uvDataBuffer = dataBuffers[1];
                        }
                        else if (dataBuffers.Length == 3)
                        {
                            yStride = strides[0];
                            yDataBuffer = dataBuffers[0];

                            uStride = strides[1];
                            uDataBuffer = dataBuffers[1];

                            vStride = strides[2];
                            vDataBuffer = dataBuffers[2];
                        }

                        if (yDataBuffer != null)
                        {
                            if (yStride == width)
                            {
                                if (!_Pipe.TryWrite(yDataBuffer))
                                {
                                    if (!Deactivated)
                                    {
                                        _Log.Error(Id, $"Could not send video frame Y plane.");
                                    }
                                }
                            }
                            else
                            {
                                var yOffset = 0;
                                for (var i = 0; i < height; i++)
                                {
                                    if (!_Pipe.TryWrite(yDataBuffer.Subset(yOffset, width)))
                                    {
                                        if (!Deactivated)
                                        {
                                            _Log.Error(Id, $"Could not send video frame Y plane row.");
                                        }
                                    }
                                    yOffset += yStride;
                                }
                            }
                        }

                        if (uvDataBuffer != null)
                        {
                            if (uvStride == width)
                            {
                                if (!_Pipe.TryWrite(uvDataBuffer))
                                {
                                    if (!Deactivated)
                                    {
                                        _Log.Error(Id, $"Could not send video frame UV plane.");
                                    }
                                }
                            }
                            else
                            {
                                var uOffset = 0;
                                for (var i = 0; i < height_2; i++)
                                {
                                    if (!_Pipe.TryWrite(uvDataBuffer.Subset(uOffset, width)))
                                    {
                                        if (!Deactivated)
                                        {
                                            _Log.Error(Id, $"Could not send video frame UV plane row.");
                                        }
                                    }
                                    uOffset += uvStride;
                                }
                            }
                        }

                        if (uDataBuffer != null)
                        {
                            if (uStride == width_2)
                            {
                                if (!_Pipe.TryWrite(uDataBuffer))
                                {
                                    if (!Deactivated)
                                    {
                                        _Log.Error(Id, $"Could not send video frame U plane.");
                                    }
                                }
                            }
                            else
                            {
                                var uOffset = 0;
                                for (var i = 0; i < height_2; i++)
                                {
                                    if (!_Pipe.TryWrite(uDataBuffer.Subset(uOffset, width_2)))
                                    {
                                        if (!Deactivated)
                                        {
                                            _Log.Error(Id, $"Could not send video frame U plane row.");
                                        }
                                    }
                                    uOffset += uStride;
                                }
                            }
                        }

                        if (vDataBuffer != null)
                        {
                            if (vStride == width_2)
                            {
                                if (!_Pipe.TryWrite(vDataBuffer))
                                {
                                    if (!Deactivated)
                                    {
                                        _Log.Error(Id, $"Could not send video frame V plane.");
                                    }
                                }
                            }
                            else
                            {
                                var vOffset = 0;
                                for (var i = 0; i < height_2; i++)
                                {
                                    if (!_Pipe.TryWrite(vDataBuffer.Subset(vOffset, width_2)))
                                    {
                                        if (!Deactivated)
                                        {
                                            _Log.Error(Id, $"Could not send video frame V plane row.");
                                        }
                                    }
                                    vOffset += vStride;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!Deactivated)
                        {
                            _Log.Error(Id, "Could not send video frame header.");
                        }
                    }
                }
            }
        }

        protected override void DoDestroy()
        {
            _Pipe.Destroy();
        }

        private bool WriteYuv4MpegStreamHeader(VideoBuffer buffer)
        {
            _HeaderWidth = buffer.Width;
            _HeaderHeight = buffer.Height;

            var magicString = "YUV4MPEG2";
            var widthString = buffer.Width.ToString();
            var heightString = buffer.Height.ToString();
            var colourSpace = "420";

            var written = true;

            // signature
            foreach (var c in magicString)
            {
                written &= Write8(c);
            }

            // width
            written &= Write8(' ');
            written &= Write8('W');

            foreach (var c in widthString)
            {
                written &= Write8(c);
            }

            // height
            written &= Write8(' ');
            written &= Write8('H');

            foreach (var c in heightString)
            {
                written &= Write8(c);
            }

            // colour space
            written &= Write8(' ');
            written &= Write8('C');

            foreach (var c in colourSpace)
            {
                written &= Write8(c);
            }

            written &= Write8('\n');

            return written;
        }

        private bool WriteYuv4MpegFrameHeader(VideoBuffer buffer)
        {
            var magicString = "FRAME";
            var widthString = buffer.Width == _HeaderWidth ? null : buffer.Width.ToString();
            var heightString = buffer.Height == _HeaderHeight ? null : buffer.Height.ToString();

            var written = true;

            // signature
            foreach (var c in magicString)
            {
                written &= Write8(c);
            }

            // width
            if (widthString != null)
            {
                written &= Write8(' ');
                written &= Write8('W');

                foreach (var c in widthString)
                {
                    written &= Write8(c);
                }
            }

            // height
            if (heightString != null)
            {
                written &= Write8(' ');
                written &= Write8('H');

                foreach (var c in heightString)
                {
                    written &= Write8(c);
                }
            }

            written &= Write8('\n');

            return written;
        }

        private readonly DataBuffer _Single = DataBuffer.Allocate(1);

        private bool Write8(int value)
        {
            _Single.Write8(value, 0);
            return _Pipe.TryWrite(_Single);
        }
    }
}
