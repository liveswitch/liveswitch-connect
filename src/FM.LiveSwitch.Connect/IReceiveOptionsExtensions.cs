using System;

namespace FM.LiveSwitch.Connect
{
    static class IReceiveOptionsExtensions
    {
        public static ManagedConnection CreateConnection(this IReceiveOptions options, Channel channel, ConnectionInfo remoteConnectionInfo, AudioStream audioStream, VideoStream videoStream, DataStream dataStream, bool logState = true)
        {
            ManagedConnection connection;
            if (remoteConnectionInfo != null)
            {
                if (remoteConnectionInfo.Id == "mcu")
                {
                    connection = channel.CreateMcuConnection(audioStream, videoStream, dataStream);
                }
                else
                {
                    connection = channel.CreateSfuDownstreamConnection(remoteConnectionInfo, audioStream, videoStream, dataStream);
                }
            }
            else
            {
                //Connect by Media ID
                connection = channel.CreateSfuDownstreamConnection(options.MediaId, audioStream, videoStream, dataStream);
            }

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
