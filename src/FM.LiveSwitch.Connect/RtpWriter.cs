using System;
using System.Net;
using System.Net.Sockets;

namespace FM.LiveSwitch.Connect
{
    class RtpWriter
    {
        public int ClockRate { get; private set; }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public int PayloadType { get; set; }

        public long SynchronizationSource { get; set; }

        private readonly UdpClient _Client;
        private readonly DataBuffer _Buffer;

        private IPEndPoint _IPEndPoint;

        public RtpWriter(int clockRate)
        {
            ClockRate = clockRate;

            _Client = CreateClient();
            _Buffer = DataBuffer.Allocate(2048);
        }

        private UdpClient CreateClient()
        {
            return new UdpClient(AddressFamily.InterNetwork);
        }

        public void Destroy()
        {
            _Client.Dispose();
        }

        public bool Write(RtpPacket packet)
        {
            if (_IPEndPoint == null)
            {
                if (IPAddress == null || Port == 0)
                {
                    Console.Error.WriteLine("IP address and port not set yet. Discarding packet.");
                    return false;
                }
                _IPEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port);
            }

            var header = new RtpPacketHeader
            {
                Marker = packet.Marker,
                Timestamp = packet.Timestamp,
                SequenceNumber = packet.SequenceNumber,
                PayloadType = packet.PayloadType,
                SynchronizationSource = packet.SynchronizationSource
            };
            var headerLength = header.CalculateHeaderLength();

            header.WriteTo(_Buffer, 0);
            _Buffer.Write(packet.Payload, headerLength);

            _Client.Send(_Buffer.Data, headerLength + packet.Payload.Length, _IPEndPoint);

            return true;
        }
    }
}
