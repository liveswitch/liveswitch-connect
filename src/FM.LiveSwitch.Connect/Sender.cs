using FM.LiveSwitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    abstract class Sender<TOptions, TAudioSource, TVideoSource>
        where TOptions : ISendOptions
        where TAudioSource : AudioSource
        where TVideoSource : VideoSource
    {
        protected async Task<int> Send(TOptions options)
        {
            try
            {
                var client = options.CreateClient();

                Console.Error.WriteLine($"{GetType().Name} client '{client.Id}' created:{Environment.NewLine}{Descriptor.Format(client.GetDescriptors())}");

                await client.Register(options);
                try
                {
                    var channel = await client.Join(options);
                    try
                    {
                        var audioSource = CreateAudioSource(options);
                        var videoSource = CreateVideoSource(options);
                        var dataSource = CreateDataSource(options);
                        var audioStream = CreateAudioStream(audioSource, options);
                        var videoStream = CreateVideoStream(videoSource, options);
                        var dataStream = CreateDataStream(dataSource, options);
                        var connection = options.CreateConnection(channel, audioStream, videoStream, dataStream);

                        Console.Error.WriteLine($"{GetType().Name} connection '{connection.Id}' created:{Environment.NewLine}{Descriptor.Format(connection.GetDescriptors())}");

                        await connection.Connect();

                        await Task.WhenAll(
                            audioSource == null ? Task.CompletedTask : audioSource.Start().AsTaskAsync(),
                            videoSource == null ? Task.CompletedTask : videoSource.Start().AsTaskAsync(),
                            dataSource == null ? Task.CompletedTask : dataSource.Start());

                        SourcesReady(audioSource, videoSource, options);

                        // watch for exit signal
                        var exitSignalledSource = new TaskCompletionSource<bool>();
                        var exitSignalled = exitSignalledSource.Task;
                        Console.CancelKeyPress += async (sender, e) =>
                        {
                            Console.Error.WriteLine("Exit signalled.");

                            e.Cancel = true;

                            SourcesUnready(audioSource, videoSource, options);

                            // stop sources
                            if (audioSource != null && audioSource.IsStarted)
                            {
                                await audioSource.Stop();
                            }
                            if (videoSource != null && videoSource.IsStarted)
                            {
                                await videoSource.Stop();
                            }
                            if (dataSource != null)
                            {
                                await dataSource.Stop();
                            }

                            // close connection gracefully
                            await connection.Disconnect();

                            exitSignalledSource.TrySetResult(true); // exit handling
                        };

                        // wait for exit signal
                        Console.Error.WriteLine("Waiting for exit signal...");
                        await exitSignalled;
                    }
                    finally
                    {
                        await client.Leave(channel.Id);
                    }
                }
                finally
                {
                    await client.Unregister();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        protected abstract TAudioSource CreateAudioSource(TOptions options);

        protected abstract TVideoSource CreateVideoSource(TOptions options);

        private DataSource CreateDataSource(TOptions options)
        {
            if (options.DataChannelLabel == null)
            {
                return null;
            }

            return new DataSource();
        }

        protected virtual void SourcesReady(TAudioSource audioSource, TVideoSource videoSource, TOptions options) { }

        protected virtual void SourcesUnready(TAudioSource audioSource, TVideoSource videoSource, TOptions options) { }

        private AudioStream CreateAudioStream(TAudioSource source, TOptions options)
        {
            if (source == null)
            {
                return null;
            }

            var track = CreateAudioTrack(source, options);
            var stream = new AudioStream(track, null);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        private VideoStream CreateVideoStream(TVideoSource source, TOptions options)
        {
            if (source == null)
            {
                return null;
            }

            var track = CreateVideoTrack(source, options);
            var stream = new VideoStream(track, null);
            stream.OnStateChange += () =>
            {
                if (stream.State == StreamState.Closed ||
                    stream.State == StreamState.Failed)
                {
                    track.Destroy();
                }
            };
            return stream;
        }

        protected DataStream CreateDataStream(DataSource source, TOptions options)
        {
            if (source == null)
            {
                return null;
            }

            var channel = new DataChannel(options.DataChannelLabel);
            source.OnMessage += (sender, message) =>
            {
                channel.SendDataString(message);
            };

            return new DataStream(channel);
        }

        private AudioTrack CreateAudioTrack(TAudioSource source, TOptions options)
        {
            var tracks = new List<AudioTrack>();
            foreach (var codec in options.AudioCodecs.Where(x => x != AudioCodec.Copy))
            {
                tracks.Add(CreateAudioTrack(codec));
            }
            return new AudioTrack(source).Next(tracks.ToArray());
        }

        private VideoTrack CreateVideoTrack(TVideoSource source, TOptions options)
        {
            var tracks = new List<VideoTrack>();
            foreach (var codec in options.VideoCodecs.Where(x => x != VideoCodec.Copy))
            {
                if (options.DisableOpenH264 && codec == VideoCodec.H264)
                {
                    continue;
                }
                tracks.Add(CreateVideoTrack(codec));
            }
            return new VideoTrack(source).Next(tracks.ToArray());
        }

        private AudioTrack CreateAudioTrack(AudioCodec codec)
        {
            var encoder = codec.CreateEncoder();
            var packetizer = codec.CreatePacketizer();
            return new AudioTrack(new SoundConverter(encoder.InputConfig)).Next(encoder).Next(packetizer);
        }

        private VideoTrack CreateVideoTrack(VideoCodec codec)
        {
            var encoder = codec.CreateEncoder();
            var packetizer = codec.CreatePacketizer();
            return new VideoTrack(new Yuv.ImageConverter(encoder.InputFormat)).Next(encoder).Next(packetizer);
        }
    }
}
