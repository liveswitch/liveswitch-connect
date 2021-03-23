
using NDI = NewTek.NDI;
using System;
using System.Runtime.InteropServices;

namespace FM.LiveSwitch.Connect
{

    class NdiAudioSource : AudioSource
    {
        static readonly ILog _Log = Log.GetLogger(typeof(NdiAudioSource));

        public override string Label
        {
            get { return "Ndi Audio Source"; }
        }

        protected NDI.Receiver _NdiReceiver { get; private set; }

        private AudioBuffer _Buffer;

        private bool _BufferAllocated = false;
        private int _BufferSize = 0;
        private readonly int _NumChannels;
        private readonly int _SampleRate;

        public NdiAudioSource(NDI.Receiver ndiReceiver, AudioFormat format, int clockRate, int channelCount)
            : base(format)
        {
            _NdiReceiver = ndiReceiver;
            _SampleRate = clockRate;
            _NumChannels = channelCount;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            if (_NdiReceiver != null)
            {
                _NdiReceiver.AudioFrameReceived += ProcessFrameReceived;
                promise.Resolve(null);
            }
            else
            {
                promise.Reject(new Exception("NDI.Receiver not initialized."));
            }
            return promise;
        }

        protected override Future<object> DoStop()
        {
            var promise = new Promise<object>();
            if (_NdiReceiver != null)
            {
                _NdiReceiver.AudioFrameReceived -= ProcessFrameReceived;

                if (_BufferAllocated)
                {
                    _Buffer.Free();
                    _BufferSize = 0;
                    _BufferAllocated = false;
                }

                promise.Resolve(null);
            }
            else
            {
                promise.Reject(new Exception("NDI.Receiver already gone."));
            }
            return promise;
        }

        private void AllocateBuffer(int sampleRate, int numSamples, int numChannels)
        {
            _Log.Debug($"Creating audio buffer - sample rate: {sampleRate}, samples: {numSamples}, channels: {numChannels}");
            if (_BufferAllocated)
            {
                _Buffer.Free();
            }

            _BufferSize = numChannels * numSamples * sizeof(short);
            DataBuffer dataBuffer = DataBuffer.Allocate(_BufferSize, true); // PCM requires little endian?
            _Buffer = new AudioBuffer(dataBuffer, new Pcm.Format(sampleRate, numChannels));
            _BufferAllocated = true;
        }

        protected void ProcessFrameReceived(object sender, NDI.AudioFrameReceivedEventArgs e)
        {
            if (!_BufferAllocated || (e.Frame.NumChannels * e.Frame.NumSamples * sizeof(short)) > _BufferSize)
            {
                if (e.Frame.NumChannels != _NumChannels)
                {
                    _Log.Error($"--audio-channel-count argument doesn't match what's being received. Configured: {_NumChannels} Received: {e.Frame.NumChannels}");
                }
                if (e.Frame.SampleRate != _SampleRate)
                {
                    _Log.Error($"--audio-clock-rate argument doesn't match what's being received. Configured: {_SampleRate} Received: {e.Frame.SampleRate}");
                }

                AllocateBuffer(e.Frame.SampleRate, e.Frame.NumSamples, e.Frame.NumChannels);
            }

            // Populate buffer with NDI frame data
            Marshal.Copy(e.Frame.AudioBuffer, _Buffer.DataBuffer.Data, 0, e.Frame.NumChannels * e.Frame.NumSamples * sizeof(short));

            // rusty.clarkson: Due to int rounding we might be losing samples...
            int duration = (1000 * e.Frame.NumSamples) / e.Frame.SampleRate;
            RaiseFrame(new AudioFrame(duration, _Buffer));
        }
    }
}
