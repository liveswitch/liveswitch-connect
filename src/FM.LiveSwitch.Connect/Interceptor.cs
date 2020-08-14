using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class Interceptor : Receiver<InterceptOptions, NullAudioSink, NullVideoSink>
    {
        public Interceptor(InterceptOptions options)
            : base(options)
        { }

        public Task<int> Intercept()
        {
            if (Options.AudioPort <= 0 && Options.VideoPort <= 0)
            {
                Console.Error.WriteLine("--audio-port and/or --video-port must be specified.");
                return Task.FromResult(1);
            }
            if (Options.AudioPort > 0)
            {
                if (!TransportAddress.IsIPAddress(Options.AudioIPAddress))
                {
                    Console.Error.WriteLine("--audio-ip-address is invalid.");
                    return Task.FromResult(1);
                }
            }
            if (Options.VideoPort > 0)
            {
                if (!TransportAddress.IsIPAddress(Options.VideoIPAddress))
                {
                    Console.Error.WriteLine("--video-ip-address is invalid.");
                    return Task.FromResult(1);
                }
            }
            return Receive();
        }

        protected override AudioStream CreateAudioStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.AudioPort == 0)
            {
                return null;
            }

            var track = CreateAudioTrack(remoteConnectionInfo);
            var stream = new AudioStream(null, track);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        protected override VideoStream CreateVideoStream(ConnectionInfo remoteConnectionInfo)
        {
            if (Options.VideoPort == 0)
            {
                return null;
            }

            var track = CreateVideoTrack(remoteConnectionInfo);
            var stream = new VideoStream(null, track);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        private AudioTrack CreateAudioTrack(ConnectionInfo remoteConnectionInfo)
        {
            var tracks = new List<AudioTrack>();
            foreach (var encoding in (AudioEncoding[])Enum.GetValues(typeof(AudioEncoding)))
            {
                tracks.Add(CreateAudioTrack(encoding, remoteConnectionInfo));
            }
            return new AudioTrack(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(ConnectionInfo remoteConnectionInfo)
        {
            var tracks = new List<VideoTrack>();
            foreach (var encoding in (VideoEncoding[])Enum.GetValues(typeof(VideoEncoding)))
            {
                if (Options.DisableOpenH264 && encoding == VideoEncoding.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(encoding, remoteConnectionInfo));
            }
            return new VideoTrack(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioEncoding encoding, ConnectionInfo remoteConnectionInfo)
        {
            var socket = GetSocket(TransportAddress.IsIPv6(Options.AudioIPAddress));
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Options.AudioIPAddress), Options.AudioPort);
            var sink = encoding.CreateNullSink(true);
            sink.OnProcessFrame += (frame) =>
            {
                var buffer = frame.LastBuffer;
                if (buffer != null)
                {
                    var dataBuffer = buffer.DataBuffer;
                    if (dataBuffer != null)
                    {
                        socket.SendTo(dataBuffer.Data, dataBuffer.Index, dataBuffer.Length, SocketFlags.None, remoteEndPoint);
                    }
                }
            };
            return new AudioTrack(sink);
        }

        private VideoTrack CreateVideoTrack(VideoEncoding encoding, ConnectionInfo remoteConnectionInfo)
        {
            var socket = GetSocket(TransportAddress.IsIPv6(Options.VideoIPAddress));
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Options.VideoIPAddress), Options.VideoPort);
            var sink = encoding.CreateNullSink(true);
            sink.OnProcessFrame += (frame) =>
            {
                var buffer = frame.LastBuffer;
                if (buffer != null)
                {
                    var dataBuffer = buffer.DataBuffer;
                    if (dataBuffer != null)
                    {
                        socket.SendTo(dataBuffer.Data, dataBuffer.Index, dataBuffer.Length, SocketFlags.None, remoteEndPoint);
                    }
                }
            };
            return new VideoTrack(sink);
        }

        private Socket GetSocket(bool ipv6)
        {
            return new Socket(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        }
    }
}
