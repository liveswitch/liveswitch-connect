using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    class NamedPipe
    {
        static ILog _Log = Log.GetLogger(typeof(NamedPipe));

        public static string GetOSPipeName(string pipeName)
        {
            if (Platform.Instance.OperatingSystem == OperatingSystem.Windows)
            {
                return $@"\\.\pipe\{pipeName}";
            }
            else
            {
                return $"unix://{Path.Combine(Path.GetTempPath(), $"CoreFxPipe_{pipeName}")}";
            }
        }

        public string PipeName { get; private set; }

        public bool Server { get; private set; }

        public bool IsConnected { get { return Stream.IsConnected; } }

        public PipeStream Stream { get; private set; }

        private readonly NamedPipeClientStream _ClientStream;
        private readonly NamedPipeServerStream _ServerStream;

        public event Action OnConnected;

        public NamedPipe(string pipeName, bool server)
        {
            PipeName = pipeName;
            Server = server;

            if (server)
            {
                Stream = _ServerStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            }
            else
            {
                Stream = _ClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            }
        }

        public void Destroy()
        {
            try
            {
                Stream.Flush();
            }
            catch
            {
                // best effort
            }
            Stream.Dispose();
        }

        public async Task DestroyAsync()
        {
            try
            {
                await Stream.FlushAsync();
            }
            catch
            {
                // best effort
            }
            await Stream.DisposeAsync();
        }

        public void WaitForConnection()
        {
            if (!Server)
            {
                throw new NotSupportedException();
            }
            _ServerStream.WaitForConnection();
            OnConnected?.Invoke();
        }

        public async Task WaitForConnectionAsync()
        {
            if (!Server)
            {
                throw new NotSupportedException();
            }
            await _ServerStream.WaitForConnectionAsync();
            OnConnected?.Invoke();
        }

        public void Connect()
        {
            if (Server)
            {
                throw new NotSupportedException();
            }
            _ClientStream.Connect();
            OnConnected?.Invoke();
        }

        public async Task ConnectAsync()
        {
            if (Server)
            {
                throw new NotSupportedException();
            }
            await _ClientStream.ConnectAsync();
            OnConnected?.Invoke();
        }

        public void Write(DataBuffer dataBuffer)
        {
            Stream.Write(dataBuffer.Data, dataBuffer.Index, dataBuffer.Length);
        }

        public Task WriteAsync(DataBuffer dataBuffer)
        {
            return Stream.WriteAsync(dataBuffer.Data, dataBuffer.Index, dataBuffer.Length);
        }

        public DataBuffer Read(int length)
        {
            return Read(DataBufferPool.Instance.Take(length));
        }

        public Task<DataBuffer> ReadAsync(int length)
        {
            return ReadAsync(DataBufferPool.Instance.Take(length));
        }

        public DataBuffer Read(DataBuffer dataBuffer)
        {
            var offset = 0;
            while (offset < dataBuffer.Length)
            {
                offset += Stream.Read(dataBuffer.Data, dataBuffer.Index + offset, dataBuffer.Length - offset);
            }
            return dataBuffer;
        }

        public async Task<DataBuffer> ReadAsync(DataBuffer dataBuffer)
        {
            var offset = 0;
            while (offset < dataBuffer.Length)
            {
                offset += await Stream.ReadAsync(dataBuffer.Data, dataBuffer.Index + offset, dataBuffer.Length - offset);
            }
            return dataBuffer;
        }

        private volatile bool _ThreadActive;
        private TaskCompletionSource<bool> _ThreadExited;

        public event Action<DataBuffer> OnReadDataBuffer;

        public void StartReading(int length)
        {
            StartReading(() => length);
        }

        public void StartReading(Func<int> getLength)
        {
            _ThreadExited = new TaskCompletionSource<bool>();
            _ThreadActive = true;

            var thread = new Thread(() =>
            {
                while (_ThreadActive)
                {
                    try
                    {
                        var length = getLength();

                        var dataBuffer = Read(length);
                        if (dataBuffer != null)
                        {
                            try
                            {
                                OnReadDataBuffer?.Invoke(dataBuffer);
                            }
                            finally
                            {
                                dataBuffer.Free();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _Log.Error(PipeName, "Could not read from named pipe.", ex);
                    }
                }

                _ThreadExited.SetResult(true);
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        public Task StopReading()
        {
            if (!_ThreadActive)
            {
                return Task.CompletedTask;
            }

            _ThreadActive = false;

            return _ThreadExited.Task;
        }

        public async Task<bool> TryAccept()
        {
            if (!Server)
            {
                return false;
            }

            try
            {
                await WaitForConnectionAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is IOException)
            {
                Console.Error.WriteLine($"Could not accept connection on named pipe '{PipeName}'.", ex);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not accept connection on named pipe '{PipeName}'. " + ex.Message, ex);
            }
            return false;
        }

        public bool TryWrite(DataBuffer dataBuffer)
        {
            if (IsConnected)
            {
                try
                {
                    Write(dataBuffer);
                    return true;
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is IOException)
                {
                    Console.Error.WriteLine($"Could not write to named pipe '{PipeName}'. " + ex.Message, ex);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Could not write to named pipe '{PipeName}'." + ex.Message, ex);
                }
            }
            return false;
        }
    }
}
