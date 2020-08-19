using System;
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
            if (Options.AudioPort <= 0)
            {
                Console.Error.WriteLine("Setting --no-audio to true because --audio-port is not specified.");
                Options.NoAudio = true;
            }
            else
            {
                if (!TransportAddress.IsIPAddress(Options.AudioIPAddress))
                {
                    Console.Error.WriteLine("--audio-ip-address is invalid.");
                    return Task.FromResult(1);
                }
            }
            if (Options.VideoPort <= 0)
            {
                Console.Error.WriteLine("Setting --no-video to true because --video-port is not specified.");
                Options.NoVideo = true;
            }
            else
            {
                if (!TransportAddress.IsIPAddress(Options.VideoIPAddress))
                {
                    Console.Error.WriteLine("--video-ip-address is invalid.");
                    return Task.FromResult(1);
                }
            }
            return Receive();
        }

        protected override NullAudioSink CreateAudioSink()
        {
            var socket = GetSocket(TransportAddress.IsIPv6(Options.AudioIPAddress));
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Options.AudioIPAddress), Options.AudioPort);
            var sink = AudioFormat.ToEncoding().CreateNullSink(true);
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
            return sink;
        }

        protected override NullVideoSink CreateVideoSink()
        {
            var socket = GetSocket(TransportAddress.IsIPv6(Options.VideoIPAddress));
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Options.VideoIPAddress), Options.VideoPort);
            var sink = VideoFormat.ToEncoding().CreateNullSink(true);
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
            return sink;
        }

        private Socket GetSocket(bool ipv6)
        {
            return new Socket(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        }
    }
}
