using System;
using System.Collections.Generic;
using System.Text;

namespace FM.LiveSwitch.Connect
{
    class Yuv4MpegNamedPipeVideoSource : NamedPipeVideoSource
    {
        static readonly ILog _Log = Log.GetLogger(typeof(Yuv4MpegNamedPipeVideoSource));

        public override string Label
        {
            get { return "YUV4MPEG Named Pipe Video Source"; }
        }

        private int _HeaderWidth;
        private int _HeaderHeight;
        private double _HeaderFrameRate;
        private string _HeaderInterlacing;
        private double _HeaderPixelAspectRatio;
        private string _HeaderColourSpace;
        private string _HeaderComment;

        public Yuv4MpegNamedPipeVideoSource(string pipeName)
            : base(pipeName, 0, 0, VideoFormat.I420, true)
        {
            StartAsync = true;
        }

        protected override void ReadStreamHeader()
        {
            // YUV4MPEG2
            if (Read8() != 'Y' ||
                Read8() != 'U' ||
                Read8() != 'V' ||
                Read8() != '4' ||
                Read8() != 'M' ||
                Read8() != 'P' ||
                Read8() != 'E' ||
                Read8() != 'G' ||
                Read8() != '2')
            {
                throw new Exception("Invalid stream signature.");
            }

            var c = Read8();
            if (c != '\n' && c != ' ')
            {
                throw new Exception("Malformed stream header.");
            }

            while (c != '\n')
            {
                c = Read8();
                if (c == 'W')
                {
                    var s = ReadParameter(out c);

                    if (!ParseAssistant.TryParseIntegerValue(s, out _HeaderWidth))
                    {
                        throw new Exception("Invalid stream header width.");
                    }
                }
                else if (c == 'H')
                {
                    var s = ReadParameter(out c);

                    if (!ParseAssistant.TryParseIntegerValue(s, out _HeaderHeight))
                    {
                        throw new Exception("Invalid stream header height.");
                    }
                }
                else if (c == 'F')
                {
                    var s = ReadParameter(out c);
                    var split = s.Split(':');
                    if (split.Length != 2)
                    {
                        throw new Exception("Invalid stream header frame rate.");
                    }

                    if (!ParseAssistant.TryParseIntegerValue(split[0], out var num) ||
                        !ParseAssistant.TryParseIntegerValue(split[1], out var den))
                    {
                        throw new Exception("Invalid stream header frame rate.");
                    }

                    _HeaderFrameRate = (double)num / den;
                }
                else if (c == 'I')
                {
                    _HeaderInterlacing = ReadParameter(out c);
                }
                else if (c == 'A')
                {
                    var s = ReadParameter(out c);
                    var split = s.Split(':');
                    if (split.Length != 2)
                    {
                        throw new Exception("Invalid stream header pixel aspect ratio.");
                    }

                    if (!ParseAssistant.TryParseIntegerValue(split[0], out var num) ||
                        !ParseAssistant.TryParseIntegerValue(split[1], out var den))
                    {
                        throw new Exception("Invalid stream header pixel aspect ratio.");
                    }

                    _HeaderPixelAspectRatio = (double)num / den;
                }
                else if (c == 'C')
                {
                    _HeaderColourSpace = ReadParameter(out c);
                }
                else if (c == 'X')
                {
                    _HeaderComment = ReadParameter(out c);
                }
                else
                {
                    var p = Utf8.Decode(new[] { (byte)c });
                    var s = ReadParameter(out c);
                    _Log.Warn(string.Format("Ignoring stream header parameter {0}{1}", p, s));
                }
            }

            // log details
            var headerParams = new List<string>();
            if (_HeaderWidth != 0)
            {
                headerParams.Add($"Width={_HeaderWidth}");
            }
            if (_HeaderHeight != 0)
            {
                headerParams.Add($"Height={_HeaderHeight}");
            }
            if (_HeaderFrameRate != 0)
            {
                headerParams.Add($"FrameRate={_HeaderFrameRate}");
            }
            if (_HeaderInterlacing != null)
            {
                headerParams.Add($"Interlacing={_HeaderInterlacing}");
            }
            if (_HeaderPixelAspectRatio != 0)
            {
                headerParams.Add($"PixelAspectRatio={_HeaderPixelAspectRatio}");
            }
            if (_HeaderColourSpace != null)
            {
                headerParams.Add($"ColourSpace={_HeaderColourSpace}");
            }
            if (_HeaderComment != null)
            {
                headerParams.Add($"Comment={_HeaderComment}");
            }
            _Log.Debug(Id, $"Stream Header => {string.Join(", ", headerParams)}");
        }

        protected override int ReadFrameHeader()
        {
            if (Read8() != 'F' ||
                Read8() != 'R' ||
                Read8() != 'A' ||
                Read8() != 'M' ||
                Read8() != 'E')
            {
                throw new Exception("Invalid frame signature.");
            }

            var c = Read8();
            if (c != '\n' && c != ' ')
            {
                throw new Exception("Malformed frame header.");
            }

            Width = _HeaderWidth;
            Height = _HeaderHeight;
            while (c != '\n')
            {
                c = Read8();
                if (c == 'W')
                {
                    var s = ReadParameter(out c);

                    if (ParseAssistant.TryParseIntegerValue(s, out var width))
                    {
                        Width = width;
                    }
                    else
                    {
                        throw new Exception("Invalid frame header width.");
                    }
                }
                else if (c == 'H')
                {
                    var s = ReadParameter(out c);

                    if (ParseAssistant.TryParseIntegerValue(s, out var height))
                    {
                        Height = height;
                    }
                    else
                    {
                        throw new Exception("Invalid frame header height.");
                    }
                }
                else
                {
                    var p = Utf8.Decode(new[] { (byte)c });
                    var s = ReadParameter(out c);
                    _Log.Warn(string.Format("Ignoring frame header parameter {0}{1}", p, s));
                }
            }

            return Width * Height * 3 / 2;
        }

        private readonly DataBuffer _Single = DataBuffer.Allocate(1);

        private int Read8()
        {
            return Pipe.Read(_Single).Read8(0);
        }

        private string ReadParameter(out int c)
        {
            var s = new StringBuilder();
            c = Read8();
            while (c != '\n' && c != ' ')
            {
                s.Append(Utf8.Decode(new[] { (byte)c }));
                c = Read8();
            }
            return s.ToString();
        }
    }
}
