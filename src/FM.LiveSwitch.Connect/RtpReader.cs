using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class RtpReader
    {
        public int ClockRate { get; private set; }

        public int Port { get; private set; }

        public event Action<RtpPacket> OnPacket;

        private UdpClient _Server;

        public RtpReader(int clockRate)
        {
            ClockRate = clockRate;

            var port = 49152;
            while (_Server == null)
            {
                try
                {
                    _Server = CreateServer(port);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.AddressAlreadyInUse)
                    {
                        throw;
                    }
                    port += 2;
                }
            }

            Port = port;
        }

        public RtpReader(int clockRate, int port)
        {
            ClockRate = clockRate;

            _Server = CreateServer(port);

            Port = port;
        }

        private UdpClient CreateServer(int port)
        {
            var listener = new UdpClient(AddressFamily.InterNetwork);
            listener.ExclusiveAddressUse = true;
            listener.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            return listener;
        }

        public void Destroy()
        {
            if (_Server != null)
            {
                _Server.Dispose();
                _Server = null;
            }
        }

        private volatile bool _LoopActive;
        private Task _LoopTask;

        public Future<object> Start()
        {
            var promise = new Promise<object>();
            try
            {
                _LoopActive = true;
                _LoopTask = Loop();
                promise.Resolve(null);
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return promise;
        }

        public Future<object> Stop()
        {
            var promise = new Promise<object>();
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        _LoopActive = false;
                        _Server.Close();
                        await _LoopTask;
                        promise.Resolve(null);
                    }
                    catch (Exception ex)
                    {
                        promise.Reject(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return promise;
        }

        private async Task Loop()
        {
            var dispatchQueue = new DispatchQueue<UdpReceiveResult>((result) =>
            {
                var buffer = DataBuffer.Wrap(result.Buffer);
                var header = RtpPacketHeader.ReadFrom(buffer);
                try
                {
                    OnPacket?.Invoke(new RtpPacket(buffer.Subset(header.CalculateHeaderLength()), header.SequenceNumber, header.Timestamp, header.Marker)
                    {
                        PayloadType = header.PayloadType,
                        SynchronizationSource = header.SynchronizationSource
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Unexpected exception raising packet. {ex}");
                }
            });

            while (_LoopActive)
            {
                try
                {
                    dispatchQueue.Enqueue(await _Server.ReceiveAsync());
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            dispatchQueue.WaitForDrain();
            dispatchQueue.Destroy();
        }
    }
}
