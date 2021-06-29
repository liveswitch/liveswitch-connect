using System;
using Vosk;

namespace FM.LiveSwitch.Connect
{
    class VoskTranscribeAudioSink : AudioSink
    {
        public override string Label
        {
            get { return "Vosk Transcribe Audio Sink"; }
        }

        public Model Model { get; }

        public event Action<string> OnResult;
        public event Action<string> OnPartialResult;
        public event Action<string> OnFinalResult;

        private readonly VoskRecognizer _Recognizer;

        public VoskTranscribeAudioSink(Model model)
            : base(new Pcm.Format(16000, 1))
        {
            Model = model;

            _Recognizer = new VoskRecognizer(Model, InputFormat.ClockRate);
        }

        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            bool result;
            var dataBuffer = inputBuffer.DataBuffer;
            if (dataBuffer.Index == 0)
            {
                result = _Recognizer.AcceptWaveform(dataBuffer.Data, dataBuffer.Length);
            }
            else
            {
                // less efficient, but AcceptWaveform does not take an index/offset
                var data = dataBuffer.ToArray();
                result = _Recognizer.AcceptWaveform(data, data.Length);
            }

            if (result)
            {
                OnResult?.Invoke(_Recognizer.Result());
            }
            else
            {
                OnPartialResult?.Invoke(_Recognizer.PartialResult());
            }
        }

        protected override void DoDestroy()
        {
            OnFinalResult?.Invoke(_Recognizer.FinalResult());

            _Recognizer.Dispose();
        }
    }
}
