using System;

namespace FM.LiveSwitch.Connect
{
    static class IReceiveOptionsExtensions
    {
        public static SfuDownstreamConnection CreateConnection(this IReceiveOptions options, Channel channel, ConnectionInfo remoteConnectionInfo, AudioStream audioStream, VideoStream videoStream, DataStream dataStream, bool logState = true)
        {
            var connection = channel.CreateSfuDownstreamConnection(remoteConnectionInfo, audioStream, videoStream, dataStream);

            connection.Tag = options.ConnectionTag;

            if (logState)
            {
                connection.OnStateChange += (c) =>
                {
                    Console.Error.WriteLine($"Connection '{connection.Id}' state is {connection.State.ToString().ToLower()}.");
                };
            }

            return connection;
        }
    }
}
