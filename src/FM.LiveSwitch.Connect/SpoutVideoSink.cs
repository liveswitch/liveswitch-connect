
using System;
using FM.Spout;

namespace FM.LiveSwitch.Connect
{
    class SpoutVideoSink : VideoSink
    {
        static ILog _Log = Log.GetLogger(typeof(NdiVideoSink));

        public override string Label
        {
            get { return "Spout Video Sink"; }
        }

        protected SpoutBuffer SpoutBuffer { get; private set; }

        public SpoutVideoSink(string name, int videoWidth, int videoHeight, VideoFormat format)
            : base(format)
        {
            Initialize(name, videoWidth, videoHeight);
        }

        private void Initialize(string name, int videoWidth, int videoHeight)
        {
            SpoutBuffer = new SpoutBuffer(name, Convert.ToUInt32(videoWidth), Convert.ToUInt32(videoHeight));
        }

        protected override void DoProcessFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            
            if (!WriteFrame(frame, inputBuffer))
            {
                if (!Deactivated)
                {
                    _Log.Error(Id, "Could not send spout video frame.");
                }
            }
        }

        protected virtual bool WriteFrame(VideoFrame frame, VideoBuffer inputBuffer)
        {
            foreach (var dataBuffer in inputBuffer.DataBuffers)
            {
                if (!SpoutBuffer.TryWrite(dataBuffer.Data))
                {
                    return false;
                }
            }
            return true;
        }

        protected override void DoDestroy()
        {
          //  SpoutBuffer.Dispose();
        }
    }
}
