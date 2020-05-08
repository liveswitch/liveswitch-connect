using System.Collections.Generic;

namespace FM.LiveSwitch.Connect
{
    static class ConnectionInfoExtensions
    {
        public static Descriptor[] GetDescriptors(this ConnectionInfo connectionInfo, bool includeClientDescriptors = false)
        {
            var descriptors = new List<Descriptor>();
            if (connectionInfo.Id != null)
            {
                descriptors.Add(new Descriptor("Connection ID", connectionInfo.Id));
            }
            if (connectionInfo.Type != null)
            {
                descriptors.Add(new Descriptor("Connection Type", connectionInfo.Type));
            }
            if (connectionInfo.Tag != null)
            {
                descriptors.Add(new Descriptor("Connection Tag", connectionInfo.Tag));
            }
            if (connectionInfo.MediaId != null)
            {
                descriptors.Add(new Descriptor("Media ID", connectionInfo.MediaId));
            }

            var audioStream = connectionInfo.AudioStream;
            if (audioStream != null)
            {
                descriptors.Add(new Descriptor("Audio", "true"));
                if (audioStream.Tag != null)
                {
                    descriptors.Add(new Descriptor("Audio Tag", audioStream.Tag));
                }
                descriptors.Add(new Descriptor("Audio Muted", audioStream.SendMuted.ToString().ToLower()));
                descriptors.Add(new Descriptor("Audio Disabled", audioStream.SendDisabled.ToString().ToLower()));

                var audioSender = audioStream.Sender;
                if (audioSender != null)
                {
                    if (audioSender.Codec != null)
                    {
                        descriptors.Add(new Descriptor("Audio Codec", audioSender.Codec.ToString()));
                    }
                    if (audioSender.Source != null)
                    {
                        descriptors.Add(new Descriptor("Audio Source", audioSender.Source.ToString()));
                    }
                }

                var audioEncodings = audioStream.SendEncodings;
                if (audioEncodings != null)
                {
                    for (var i = 0; i < audioEncodings.Length; i++)
                    {
                        var audioEncoding = audioEncodings[i];
                        if (audioEncoding.Bitrate != -1)
                        {
                            descriptors.Add(new Descriptor($"Audio Encoding {i + 1} Bitrate", audioEncoding.Bitrate.ToString()));
                        }
                    }
                }
            }
            var videoStream = connectionInfo.VideoStream;
            if (videoStream != null)
            {
                descriptors.Add(new Descriptor("Video", "true"));
                if (videoStream.Tag != null)
                {
                    descriptors.Add(new Descriptor("Video Tag", videoStream.Tag));
                }
                descriptors.Add(new Descriptor("Video Muted", videoStream.SendMuted.ToString().ToLower()));
                descriptors.Add(new Descriptor("Video Disabled", videoStream.SendDisabled.ToString().ToLower()));

                var videoSender = videoStream.Sender;
                if (videoSender != null)
                {
                    if (videoSender.Codec != null)
                    {
                        descriptors.Add(new Descriptor("Video Codec", videoSender.Codec.ToString()));
                    }
                    if (videoSender.Source != null)
                    {
                        descriptors.Add(new Descriptor("Video Source", videoSender.Source.ToString()));
                    }
                }

                var videoEncodings = videoStream.SendEncodings;
                if (videoEncodings != null)
                {
                    for (var i = 0; i < videoEncodings.Length; i++)
                    {
                        var videoEncoding = videoEncodings[i];
                        if (videoEncoding.Bitrate != -1)
                        {
                            descriptors.Add(new Descriptor($"Video Encoding {i + 1} Bitrate", videoEncoding.Bitrate.ToString()));
                        }
                        if (videoEncoding.Width != -1)
                        {
                            descriptors.Add(new Descriptor($"Video Encoding {i + 1} Width", videoEncoding.Width.ToString()));
                        }
                        if (videoEncoding.Height != -1)
                        {
                            descriptors.Add(new Descriptor($"Video Encoding {i + 1} Height", videoEncoding.Height.ToString()));
                        }
                        if (videoEncoding.FrameRate != -1)
                        {
                            descriptors.Add(new Descriptor($"Video Encoding {i + 1} Frame Rate", videoEncoding.FrameRate.ToString()));
                        }
                    }
                }
            }

            var dataStream = connectionInfo.DataStream;
            if (dataStream != null)
            {
                descriptors.Add(new Descriptor("Data", "true"));
                if (dataStream.Tag != null)
                {
                    descriptors.Add(new Descriptor("Data Tag", dataStream.Tag));
                }

                var dataChannels = dataStream.Channels;
                if (dataChannels != null)
                {
                    for (var i = 0; i < dataChannels.Length; i++)
                    {
                        var dataChannel = dataChannels[i];
                        if (dataChannel.Label != null)
                        {
                            descriptors.Add(new Descriptor($"Data Channel {i + 1} Label", dataChannel.Label));
                        }
                        descriptors.Add(new Descriptor($"Data Channel {i + 1} Ordered", dataChannel.Ordered.ToString().ToLower()));
                        if (!string.IsNullOrEmpty(dataChannel.Subprotocol))
                        {
                            descriptors.Add(new Descriptor($"Data Channel {i + 1} Subprotocol", dataChannel.Subprotocol));
                        }
                    }
                }
            }

            if (includeClientDescriptors)
            {
                descriptors.AddRange(connectionInfo.CreateClientInfo().GetDescriptors());
            }

            return descriptors.ToArray();
        }
    }
}
