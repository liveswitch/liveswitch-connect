
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

        protected NDI.Receiver NdiReceiver { get; private set; }

        private AudioBuffer buffer;

        private bool bufferAllocated = false;
        private int bufferSize = 0;
        private readonly int numChannels;
        private readonly int sampleRate;

        public NdiAudioSource(NDI.Receiver ndiReceiver, AudioFormat format, int clockRate, int channelCount)
            : base(format)
        {
            NdiReceiver = ndiReceiver;
            sampleRate = clockRate;
            numChannels = channelCount;
        }

        protected override Future<object> DoStart()
        {
            var promise = new Promise<object>();
            if (NdiReceiver != null)
            {
                NdiReceiver.AudioFrameReceived += ProcessFrameReceived;
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
            if (NdiReceiver != null)
            {
                NdiReceiver.AudioFrameReceived -= ProcessFrameReceived;

                if (bufferAllocated)
                {
                    buffer.Free();
                    bufferSize = 0;
                    bufferAllocated = false;
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
            if (bufferAllocated)
            {
                buffer.Free();
            }

            bufferSize = numChannels * numSamples * sizeof(short);
            DataBuffer dataBuffer = DataBuffer.Allocate(bufferSize, true); // PCM requires little endian?
            buffer = new AudioBuffer(dataBuffer, new Pcm.Format(sampleRate, numChannels));
            bufferAllocated = true;
        }

        protected void ProcessFrameReceived(object sender, NDI.AudioFrameReceivedEventArgs e)
        {
            if (!bufferAllocated || (e.Frame.NumChannels * e.Frame.NumSamples * sizeof(short)) > bufferSize)
            {
                if (e.Frame.NumChannels != numChannels)
                {
                    _Log.Error($"--audio-channel-count argument doesn't match what's being received. Configured: {numChannels} Received: {e.Frame.NumChannels}");
                }
                if (e.Frame.SampleRate != sampleRate)
                {
                    _Log.Error($"--audio-clock-rate argument doesn't match what's being received. Configured: {sampleRate} Received: {e.Frame.SampleRate}");
                }

                AllocateBuffer(e.Frame.SampleRate, e.Frame.NumSamples, e.Frame.NumChannels);
            }

            // Populate buffer with NDI frame data
            Marshal.Copy(e.Frame.AudioBuffer, buffer.DataBuffer.Data, 0, e.Frame.NumChannels * e.Frame.NumSamples * sizeof(short));

            // rusty.clarkson: Due to int rounding we might be losing samples...
            int duration = (1000 * e.Frame.NumSamples) / e.Frame.SampleRate;
            RaiseFrame(new AudioFrame(duration, buffer));
        }
    }
}
