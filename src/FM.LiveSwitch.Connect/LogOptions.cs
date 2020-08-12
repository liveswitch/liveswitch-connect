using CommandLine;

namespace FM.LiveSwitch.Connect
{
    [Verb("log", HelpText = "Logs remote media to stdout.")]
    class LogOptions : ReceiveOptions
    {
        [Option("audio-log", Required = false, Default = "audio: {duration}ms {encoding}/{clockRate}/{channelCount} frame received ({footprint} bytes) for SSRC {synchronizationSource} and timestamp {timestamp}", HelpText = "The audio log template. Uses curly-brace syntax. Valid variables: footprint, duration, clockRate, channelCount, mediaStreamId, rtpStreamId, sequenceNumber, synchronizationSource, systemTimestamp, timestamp, encoding, applicationId, channelId, userId, userAlias, deviceId, deviceAlias, clientId, clientTag, connectionId, connectionTag, mediaId")]
        public string AudioLog { get; set; }

        [Option("video-log", Required = false, Default = "video: {width}x{height} {encoding} frame received ({footprint} bytes) for SSRC {synchronizationSource} and timestamp {timestamp}", HelpText = "The video log template. Uses curly-brace syntax. Valid variables: footprint, width, height, mediaStreamId, rtpStreamId, sequenceNumber, synchronizationSource, systemTimestamp, timestamp, encoding, applicationId, channelId, userId, userAlias, deviceId, deviceAlias, clientId, clientTag, connectionId, connectionTag, mediaId")]
        public string VideoLog { get; set; }
    }
}
