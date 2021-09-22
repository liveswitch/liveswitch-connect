using System.Text;

namespace FM.LiveSwitch.Connect
{
    class Yuv4MpegNamedPipeVideoSink : NamedPipeVideoSink
    {
        public override string Label
        {
            get { return "YUV4MPEG Named Pipe Video Sink"; }
        }

        private int _HeaderWidth;
        private int _HeaderHeight;

        public Yuv4MpegNamedPipeVideoSink(string pipeName)
            : base(pipeName, false, VideoFormat.I420)
        { }

        protected override bool WriteStreamHeader(VideoFrame frame, VideoBuffer buffer)
        {
            _HeaderWidth = buffer.Width;
            _HeaderHeight = buffer.Height;

            var magicString = "YUV4MPEG2";
            var widthString = buffer.Width.ToString();
            var heightString = buffer.Height.ToString();
            var colourSpace = "420";

            var streamHeader = new StringBuilder();

            // signature
            streamHeader.Append(magicString);

            // width
            streamHeader.Append(" W");
            streamHeader.Append(widthString);

            // height
            streamHeader.Append(" H");
            streamHeader.Append(heightString);

            // colour space
            streamHeader.Append(" C");
            streamHeader.Append(colourSpace);

            streamHeader.Append("\n");

            return Pipe.TryWriteAsync(DataBuffer.Wrap(Encoding.ASCII.GetBytes(streamHeader.ToString()))).Result;
        }

        protected override bool WriteFrameHeader(VideoFrame frame, VideoBuffer buffer)
        {
            var magicString = "FRAME";
            var widthString = buffer.Width == _HeaderWidth ? null : buffer.Width.ToString();
            var heightString = buffer.Height == _HeaderHeight ? null : buffer.Height.ToString();

            var frameHeader = new StringBuilder();

            // signature
            frameHeader.Append(magicString);

            // width
            if (widthString != null)
            {
                frameHeader.Append(" W");
                frameHeader.Append(widthString);
            }

            // height
            if (heightString != null)
            {
                frameHeader.Append(" H");
                frameHeader.Append(heightString);
            }

            frameHeader.Append("\n");

            return Pipe.TryWriteAsync(DataBuffer.Wrap(Encoding.ASCII.GetBytes(frameHeader.ToString()))).Result;
        }

        protected override bool WriteFrame(VideoFrame frame, VideoBuffer buffer)
        {
            var width = buffer.Width;
            var height = buffer.Height;
            var strides = buffer.Strides;
            var dataBuffers = buffer.DataBuffers;

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
                    if (!Pipe.TryWriteAsync(yDataBuffer).Result)
                    {
                        return false;
                    }
                }
                else
                {
                    var yOffset = 0;
                    for (var i = 0; i < height; i++)
                    {
                        if (!Pipe.TryWriteAsync(yDataBuffer.Subset(yOffset, width)).Result)
                        {
                            return false;
                        }
                        yOffset += yStride;
                    }
                }
            }

            if (uvDataBuffer != null)
            {
                if (uvStride == width)
                {
                    if (!Pipe.TryWriteAsync(uvDataBuffer).Result)
                    {
                        return false;
                    }
                }
                else
                {
                    var uOffset = 0;
                    for (var i = 0; i < height_2; i++)
                    {
                        if (!Pipe.TryWriteAsync(uvDataBuffer.Subset(uOffset, width)).Result)
                        {
                            return false;
                        }
                        uOffset += uvStride;
                    }
                }
            }

            if (uDataBuffer != null)
            {
                if (uStride == width_2)
                {
                    if (!Pipe.TryWriteAsync(uDataBuffer).Result)
                    {
                        return false;
                    }
                }
                else
                {
                    var uOffset = 0;
                    for (var i = 0; i < height_2; i++)
                    {
                        if (!Pipe.TryWriteAsync(uDataBuffer.Subset(uOffset, width_2)).Result)
                        {
                            return false;
                        }
                        uOffset += uStride;
                    }
                }
            }

            if (vDataBuffer != null)
            {
                if (vStride == width_2)
                {
                    if (!Pipe.TryWriteAsync(vDataBuffer).Result)
                    {
                        return false;
                    }
                }
                else
                {
                    var vOffset = 0;
                    for (var i = 0; i < height_2; i++)
                    {
                        if (!Pipe.TryWriteAsync(vDataBuffer.Subset(vOffset, width_2)).Result)
                        {
                            return false;
                        }
                        vOffset += vStride;
                    }
                }
            }

            return true;
        }
    }
}
