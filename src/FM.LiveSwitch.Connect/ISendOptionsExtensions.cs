using System;

namespace FM.LiveSwitch.Connect
{
    static class ISendOptionsExtensions
    {
        public static SfuUpstreamConnection CreateConnection(this ISendOptions options, Channel channel, AudioStream audioStream, VideoStream videoStream, DataStream dataStream, bool logState = true)
        {
            var connection = channel.CreateSfuUpstreamConnection(audioStream, videoStream, dataStream, options.MediaId);

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
