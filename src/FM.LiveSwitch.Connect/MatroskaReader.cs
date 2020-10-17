using System;
using System.Collections.Generic;
using FM.LiveSwitch.Matroska;

namespace FM.LiveSwitch.Connect
{
    class MatroskaReader
    {
        private readonly System.IO.Stream _Stream;

        private static byte[] EbmlCrc32Id { get { return new byte[] { 0xBF }; } }
        private static byte[] EbmlVoidId { get { return new byte[] { 0xEC }; } }
        private static byte[] EbmlClusterTimecodeId = new byte[] { 0xE7 };
        private static byte[] EbmlClusterPositionId = new byte[] { 0xA7 };
        private static byte[] EbmlClusterPrevSizeId = new byte[] { 0xAB };
        private static byte[] EbmlBlockGroupReferenceBlockId = new byte[] { 0xFB };
        private static byte[] EbmlBlockGroupBlockDurationId = new byte[] { 0x9B };

        private Ebml _Ebml = null;

        private volatile bool _HasEbml = false;
        private volatile bool _InSegment = false;
        private volatile bool _InCluster = false;
        private volatile bool _InBlockGroup = false;

        private long _SegmentEndPosition = -1;
        private long _ClusterEndPosition = -1;
        private long _BlockGroupEndPosition = -1;

        private SegmentInfo _SegmentInfo = null;
        private List<SeekHead> _SeekHeads = new List<SeekHead>();
        private List<Track> _Tracks = new List<Track>();

        private long _ClusterTimecode = -1;
        private long _ClusterPosition = -1;
        private long _ClusterPrevSize = -1;

        private List<long> _ClusterBlockGroupReferenceBlocks = new List<long>();
        private long _ClusterBlockGroupBlockDuration = -1;

        private long _ClusterBlockTrackNumber = -1;
        private int _ClusterBlockTimecode = -1;
        private byte _ClusterBlockFlags = 0;

        private long _StreamPosition = 0;
        private int _NextFrameLength = -1;

        private long _FirstFrameTimestamp = -1;

        public MatroskaReader(System.IO.Stream stream)
        {
            _Stream = stream;
        }

        public void ReadStreamHeader()
        {
            _NextFrameLength = ReadNextFrameLength();
        }

        public int ReadFrameHeader()
        {
            if (_FirstFrameTimestamp == -1)
            {
                _FirstFrameTimestamp = ManagedStopwatch.GetTimestamp();
            }
            if (_NextFrameLength == -1)
            {
                _NextFrameLength = ReadNextFrameLength();
            }
            var frameLength = _NextFrameLength;
            _NextFrameLength = -1;
            _StreamPosition += frameLength;
            return frameLength;
        }

        public long WaitForFrameTimestamp(int clockRate)
        {
            var elapsedTicks = ManagedStopwatch.GetTimestamp() - _FirstFrameTimestamp;
            var nanoTimestamp = (_ClusterTimecode + _ClusterBlockTimecode) * _SegmentInfo.TimecodeScale;
            var tickTimestamp = nanoTimestamp / 100;
            if (tickTimestamp > elapsedTicks)
            {
                // hold up
                ManagedThread.Sleep((int)((tickTimestamp - elapsedTicks) / Constants.TicksPerMillisecond));
            }

            _ClusterBlockTrackNumber = -1;
            _ClusterBlockTimecode = -1;
            _ClusterBlockFlags = 0;

            return clockRate * tickTimestamp / Constants.TicksPerSecond;
        }

        private int ReadNextFrameLength()
        {
            while (true)
            {
                if (_StreamPosition == _BlockGroupEndPosition)
                {
                    _InBlockGroup = false;
                    _BlockGroupEndPosition = -1;
                }

                if (_StreamPosition == _ClusterEndPosition)
                {
                    _InCluster = false;
                    _ClusterEndPosition = -1;
                }

                if (_StreamPosition == _SegmentEndPosition)
                {
                    _InSegment = false;
                    _SegmentEndPosition = -1;
                }

                var id = ReadId(_Stream);
                if (id.Length == 1 && id[0] == 0xFF)
                {
                    // end of stream
                    return 0;
                }
                if (!_HasEbml)
                {
                    if (Element.Compare(id, Ebml.EbmlId))
                    {
                        // read this in one block
                        _Ebml = new Ebml(ReadValue(id));

                        _HasEbml = true;
                    }
                    else if (Element.Compare(id, EbmlCrc32Id))
                    {
                        // verify
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlVoidId))
                    {
                        // ignore
                        ReadValue(id);
                    }
                    else
                    {
                        throw new Exception($"Unexpected ID '{BitAssistant.GetHexString(id)}' at stream offset '{_StreamPosition}'.");
                    }
                }
                else if (!_InSegment)
                {
                    if (Element.Compare(id, Segment.EbmlId))
                    {
                        // don't read the value
                        var segmentLength = ReadValueLength(id);
                        if (segmentLength != -1)
                        {
                            _SegmentEndPosition = _StreamPosition + segmentLength;
                        }

                        _InSegment = true;
                    }
                    else if (Element.Compare(id, EbmlCrc32Id))
                    {
                        // verify
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlVoidId))
                    {
                        // ignore
                        ReadValue(id);
                    }
                    else
                    {
                        throw new Exception($"Unexpected ID '{BitAssistant.GetHexString(id)}' at stream offset '{_StreamPosition}'.");
                    }
                }
                else if (!_InCluster)
                {
                    if (Element.Compare(id, SegmentInfo.EbmlId))
                    {
                        _SegmentInfo = new SegmentInfo(ReadValue(id));
                    }
                    else if (Element.Compare(id, SeekHead.EbmlId))
                    {
                        _SeekHeads.Add(new SeekHead(ReadValue(id)));
                    }
                    else if (Element.Compare(id, Track.EbmlId))
                    {
                        _Tracks.Add(new Track(ReadValue(id)));
                    }
                    else if (Element.Compare(id, Cluster.EbmlId))
                    {
                        // don't read the value
                        var clusterLength = ReadValueLength(id);
                        if (clusterLength != -1)
                        {
                            _ClusterEndPosition = _StreamPosition + clusterLength;
                        }

                        _InCluster = true;

                        _ClusterTimecode = -1;
                        _ClusterPosition = -1;
                        _ClusterPrevSize = -1;
                    }
                    else if (Element.Compare(id, new byte[] { 0x12, 0x54, 0xC3, 0x67 }))
                    {
                        //TODO: Tags
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, new byte[] { 0x1C, 0x53, 0xBB, 0x6B }))
                    {
                        //TODO: Cues
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlCrc32Id))
                    {
                        // verify
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlVoidId))
                    {
                        // ignore
                        ReadValue(id);
                    }
                    else
                    {
                        throw new Exception($"Unexpected ID '{BitAssistant.GetHexString(id)}' at stream offset '{_StreamPosition}'.");
                    }
                }
                else if (!_InBlockGroup)
                {
                    if (Element.Compare(id, EbmlClusterTimecodeId))
                    {
                        _ClusterTimecode = Element.ReadUnsignedInteger(ReadValue(id));
                    }
                    else if (Element.Compare(id, EbmlClusterPositionId))
                    {
                        _ClusterPosition = Element.ReadUnsignedInteger(ReadValue(id));
                    }
                    else if (Element.Compare(id, EbmlClusterPrevSizeId))
                    {
                        _ClusterPrevSize = Element.ReadUnsignedInteger(ReadValue(id));
                    }
                    else if (Element.Compare(id, BlockGroup.EbmlId))
                    {
                        // don't read the value
                        var blockGroupLength = ReadValueLength(id);
                        if (blockGroupLength != -1)
                        {
                            _BlockGroupEndPosition = _StreamPosition + blockGroupLength;
                        }

                        _InBlockGroup = true;

                        _ClusterBlockGroupReferenceBlocks.Clear();
                        _ClusterBlockGroupBlockDuration = -1;
                    }
                    else if (Element.Compare(id, SimpleBlock.EbmlId))
                    {
                        var frameLength = ReadFrameLength(id);
                        if (_ClusterBlockTrackNumber == 1)
                        {
                            return frameLength;
                        }
                        Read(frameLength);
                    }
                    else if (Element.Compare(id, EbmlCrc32Id))
                    {
                        // verify
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlVoidId))
                    {
                        // ignore
                        ReadValue(id);
                    }
                    else
                    {
                        throw new Exception($"Unexpected ID '{BitAssistant.GetHexString(id)}' at stream offset '{_StreamPosition}'.");
                    }
                }
                else
                {
                    if (Element.Compare(id, Block.EbmlId))
                    {
                        var frameLength = ReadFrameLength(id);
                        if (_ClusterBlockTrackNumber == 1)
                        {
                            return frameLength;
                        }
                        Skip(frameLength);
                    }
                    else if (Element.Compare(id, EbmlBlockGroupReferenceBlockId))
                    {
                        _ClusterBlockGroupReferenceBlocks.Add(Element.ReadSignedInteger(ReadValue(id)));
                    }
                    else if (Element.Compare(id, EbmlBlockGroupBlockDurationId))
                    {
                        _ClusterBlockGroupBlockDuration = Element.ReadSignedInteger(ReadValue(id));
                    }
                    else if (Element.Compare(id, new byte[] { 0x75, 0xA1 }))
                    {
                        //TODO: BlockAdditions
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlCrc32Id))
                    {
                        // verify
                        ReadValue(id);
                    }
                    else if (Element.Compare(id, EbmlVoidId))
                    {
                        // ignore
                        ReadValue(id);
                    }
                    else
                    {
                        throw new Exception($"Unexpected ID '{BitAssistant.GetHexString(id)}' at stream offset '{_StreamPosition}'.");
                    }
                }
            }
        }

        private void Skip(int length)
        {
            Read(length);
        }

        private byte[] Read(int length)
        {
            var output = new byte[length];
            _Stream.Read(output);
            return output;
        }

        private int ReadFrameLength(byte[] id)
        {
            var valueLength = ReadValueLength(id);

            var startPosition = _StreamPosition;
            _ClusterBlockTrackNumber = ReadVariableInteger(_Stream, out var bytesRead);
            _StreamPosition += bytesRead;

            _ClusterBlockTimecode = Binary.FromBytes16(new[] { (byte)_Stream.ReadByte(), (byte)_Stream.ReadByte() }, 0, false);
            _StreamPosition += 2;

            _ClusterBlockFlags = (byte)_Stream.ReadByte();
            _StreamPosition++;

            return valueLength - (int)(_StreamPosition - startPosition);
        }

        private byte[] ReadValue(byte[] id)
        {
            var valueLength = ReadValueLength(id);
            var value = Read(valueLength);
            _StreamPosition += valueLength;
            return value;
        }

        private int ReadValueLength(byte[] id)
        {
            _StreamPosition += id.Length;
            var valueLength = (int)ReadValueLength(_Stream, out var bytesRead);
            _StreamPosition += bytesRead;
            return valueLength;
        }

        private static byte[] ReadId(System.IO.Stream stream)
        {
            var b1 = (byte)stream.ReadByte();
            if ((b1 & 0x80) == 0x80) // 1xxx xxxx
            {
                // Class A ID (1 byte)
                return new byte[] { b1 };
            }

            var b2 = (byte)stream.ReadByte();
            if ((b1 & 0xC0) == 0x40) // 01xx xxxx xxxx xxxx
            {
                // Class B ID (2 bytes)
                return new byte[] { b1, b2 };
            }

            var b3 = (byte)stream.ReadByte();
            if ((b1 & 0xE0) == 0x20) // 001x xxxx xxxx xxxx xxxx xxxx
            {
                // Class C ID (3 bytes)
                return new byte[] { b1, b2, b3 };
            }

            var b4 = (byte)stream.ReadByte();
            if ((b1 & 0xF0) == 0x10) // 0001 xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                // Class D ID (4 bytes)
                return new byte[] { b1, b2, b3, b4 };
            }

            throw new Exception("Cannot read ID. Stream is corrupt.");
        }

        private static long ReadValueLength(System.IO.Stream stream, out int bytesRead)
        {
            bytesRead = 0;
            var b1 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0x80) == 0x80) // 1xxx xxxx
            {
                var length = (b1 & 0x7F);
                if (length == 0x7F)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b2 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xC0) == 0x40) // 01xx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes16(new[] { b1, b2 }, 0, false) & 0x3FFF);
                if (length == 0x3FFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b3 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xE0) == 0x20) // 001x xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes24(new[] { b1, b2, b3 }, 0, false) & 0x1FFFFF);
                if (length == 0x1FFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b4 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xF0) == 0x10) // 0001 xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes32(new[] { b1, b2, b3, b4 }, 0, false) & 0x0FFFFFFF);
                if (length == 0x0FFFFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b5 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xF8) == 0x08) // 0000 1xxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes40(new[] { b1, b2, b3, b4, b5 }, 0, false) & 0x07FFFFFFFF);
                if (length == 0x07FFFFFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b6 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xFC) == 0x04) // 0000 01xx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes48(new[] { b1, b2, b3, b4, b5, b6 }, 0, false) & 0x03FFFFFFFFFF);
                if (length == 0x03FFFFFFFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b7 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xFE) == 0x02) // 0000 001x xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes56(new[] { b1, b2, b3, b4, b5, b6, b7 }, 0, false) & 0x01FFFFFFFFFFFF);
                if (length == 0x01FFFFFFFFFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            var b8 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xFF) == 0x01) // 0000 0001 xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var length = (Binary.FromBytes64(new[] { b1, b2, b3, b4, b5, b6, b7, b8 }, 0, false) & 0x00FFFFFFFFFFFFFF);
                if (length == 0x00FFFFFFFFFFFFFF)
                {
                    // reserved for unknown
                    length = -1;
                }
                return length;
            }

            throw new Exception("Cannot read value length. Stream is corrupt.");
        }

        private static long ReadVariableInteger(System.IO.Stream stream, out int bytesRead)
        {
            bytesRead = 0;
            var b1 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0x80) == 0x80) // 1xxx xxxx
            {
                return (b1 & 0x7F);
            }

            var b2 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xC0) == 0x40) // 01xx xxxx xxxx xxxx
            {
                var value = new[] { b1, b2 };
                value[0] &= 0x3F;
                return Binary.FromBytes16(value, 0, false);
            }

            var b3 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xE0) == 0x20) // 001x xxxx xxxx xxxx xxxx xxxx
            {
                var value = new[] { b1, b2, b3 };
                value[0] &= 0x1F;
                return Binary.FromBytes24(value, 0, false);
            }

            var b4 = (byte)stream.ReadByte(); bytesRead++;
            if ((b1 & 0xF0) == 0x10) // 0001 xxxx xxxx xxxx xxxx xxxx xxxx xxxx
            {
                var value = new[] { b1, b2, b3, b4 };
                value[0] &= 0x0F;
                return Binary.FromBytes32(value, 0, false);
            }

            throw new Exception("Cannot read variable integer. Stream is corrupt.");
        }
    }
}
