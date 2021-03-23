# LiveSwitch Connect CLI

![build](https://github.com/liveswitch/liveswitch-connect/workflows/build/badge.svg) ![code quality](https://app.codacy.com/project/badge/Grade/bc368b4880724ca2abf34b09ffd87092) ![license](https://img.shields.io/badge/License-MIT-yellow.svg) ![release](https://img.shields.io/github/v/release/liveswitch/liveswitch-connect.svg)

The LiveSwitch Connect CLI lets you send and receive media to and from LiveSwitch.

Requires .NET Core 3.1 or newer. Requires LiveSwitch Server 1.10.0 or newer.

## Building

Use `dotnet publish` to create a single, self-contained file for a specific platform/architecture:

### Windows

```shell
dotnet publish src/FM.LiveSwitch.Connect -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o win
```

### macOS

```shell
dotnet publish src/FM.LiveSwitch.Connect -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o osx
```

### Linux

```shell
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o linux
```

Alternatively, use `dotnet build` to create a platform-agnostic bundle (the .NET Core runtime must be installed):

```shell
dotnet build
```

Using this approach will generate a library instead of an executable. Use `dotnet lsconnect.dll` instead of `lsconnect` to run it.

## Docker

Images are also hosted on [DockerHub](https://hub.docker.com/r/frozenmountain/liveswitch-connect).

```shell
docker run --rm frozenmountain/liveswitch-connect [verb] [options]
```

## Environment Variables

Environment variables can be used in place of command-line arguments.

Environment variable names are `lsconnect_{verb}_{option}`, e.g. `lsconnect_play_gateway-url`.

Environment variable names are case-insensitive, so `lsconnect_play_application-id` is equivalent to `LSCONNECT_PLAY_APPLICATION-ID`.

Note that command-line arguments always take precedence over environment variables.

## Usage

```shell
lsconnect [verb] [options]
```

### Verbs

```shell
  shell        Starts an interactive shell.

  capture      Captures local media from a named pipe.

  ffcapture    Captures local media from FFmpeg.

  fake         Sends media from a fake source.

  play         Sends media from a local file.

  render       Renders remote media to a named pipe.

  ffrender     Renders remote media to FFmpeg.

  log          Logs remote media to stdout.

  record       Records remote media to a local file.

  intercept    Forwards packets for lawful intercept.
```

## shell

The `shell` verb starts an interactive shell that lets you monitor clients and connections in a LiveSwitch Server.

```shell
  --gateway-url       Required. The gateway URL.

  --application-id    Required. The application ID.

  --shared-secret     Required. The shared secret for the application ID.

  --log-level         (Default: Error) The LiveSwitch log level.
```

Once the `shell` is active:

```shell
  register    Registers a client.

  exit        Exits the shell.
```

After you `register`:

```shell
  unregister    Unregisters the client.

  join          Joins a channel.

  exit          Exits the shell.
```

After you `join` a channel (e.g. `join my-channel-id`):

```shell
  leave          Leaves the channel.

  clients        Prints remote client details to stdout.

  connections    Prints remote connection details to stdout.

  exit           Exits the shell.
```

The `clients` verb has additional options:

```shell
  --ids        Print IDs only.

  --listen     Listen for join/leave events. (Press Q to stop.)
```

The `connections` verb has additional options as well:

```shell
  --ids        Print IDs only.

  --listen     Listen for open/close events. (Press Q to stop.)
```

## capture

The `capture` verb lets you capture local media from a named pipe and send it to a LiveSwitch server.

```shell
  --audio-pipe              The named pipe for audio.

  --video-pipe              The named pipe for video.

  --server                  Use a named pipe server.

  --audio-clock-rate        (Default: 48000) The audio clock rate in Hz. Must be
                            a multiple of 8000. Minimum value is 8000. Maximum
                            value is 48000.

  --audio-channel-count     (Default: 2) The audio channel count. Minimum value
                            is 1. Maximum value is 2.

  --audio-frame-duration    (Default: 20) The audio frame duration in
                            milliseconds. Minimum value is 5. Maximum value is
                            100.

  --video-format            (Default: Bgr) The video format.

  --video-width             The video width.

  --video-height            The video height.

  --media-id                The local media ID.

  --audio-bitrate           The audio bitrate.

  --video-bitrate           The video bitrate.

  --video-frame-rate        The video frame-rate, if known, for signalling.

  --channel-id              Required. The channel ID.

  --data-channel-label      The data channel label.

  --user-id                 The local user ID.

  --user-alias              The local user alias.

  --device-id               The local device ID.

  --device-alias            The local device alias.

  --client-tag              The local client tag.

  --client-roles            The local client roles.

  --connection-tag          The local connection tag.

  --no-audio                Do not process audio.

  --no-video                Do not process video.

  --audio-codec             (Default: Any) The audio codec to negotiate with
                            LiveSwitch.

  --video-codec             (Default: Any) The video codec to negotiate with
                            LiveSwitch.

  --gateway-url             Required. The gateway URL.

  --application-id          Required. The application ID.

  --shared-secret           Required. The shared secret for the application ID.

  --log-level               (Default: Error) The LiveSwitch log level.
```

## ffcapture

The `ffcapture` verb lets you capture local media from FFmpeg and send it to a LiveSwitch server.

```shell
  --input-args                    Required. The FFmpeg input arguments.

  --audio-mode                    (Default: LSEncode) Where audio is encoded.

  --video-mode                    (Default: LSEncode) Where video is encoded.

  --audio-encoding                The audio encoding of the input stream, if
                                  different from audio-codec. Enables
                                  transcoding if audio-mode is noencode or
                                  ffencode.

  --video-encoding                The video encoding of the input stream, if
                                  different from video-codec. Enables
                                  transcoding if video-mode is noencode or
                                  ffencode.

  --ffencode-keyframe-interval    (Default: 30) The keyframe interval for video.
                                  Only used if video-mode is ffencode.

  --media-id                      The local media ID.

  --audio-bitrate                 The audio bitrate.

  --video-bitrate                 The video bitrate.

  --video-width                   The video width, if known, for signalling.

  --video-height                  The video height, if known, for signalling.

  --video-frame-rate              The video frame-rate, if known, for
                                  signalling.

  --channel-id                    Required. The channel ID.

  --data-channel-label            The data channel label.

  --user-id                       The local user ID.

  --user-alias                    The local user alias.

  --device-id                     The local device ID.

  --device-alias                  The local device alias.

  --client-tag                    The local client tag.

  --client-roles                  The local client roles.

  --connection-tag                The local connection tag.

  --no-audio                      Do not process audio.

  --no-video                      Do not process video.

  --audio-codec                   (Default: Any) The audio codec to negotiate
                                  with LiveSwitch.

  --video-codec                   (Default: Any) The video codec to negotiate
                                  with LiveSwitch.

  --gateway-url                   Required. The gateway URL.

  --application-id                Required. The application ID.

  --shared-secret                 Required. The shared secret for the
                                  application ID.

  --log-level                     (Default: Error) The LiveSwitch log level.
```

## ndifind

The `ndifind` verb prints a list of the discoverable [NDI®](https://ndi.tv/) devices on the network.

## ndicapture

The `ndicapture` verb lets you capture media from an [NDI®](https://ndi.tv/) device and send it to a LiveSwitch server.

```shell
  --stream-name                   Required. Name of the NDI® stream to capture.

  --audio-clock-rate              (Default: 48000) The audio clock rate in Hz.
                                  Minimum value is 8000. Maximum value is 48000.

  --audio-channel-count           (Default: 2) The audio channel count. Minimum value
                                  is 1. Maximum value is 4.

  --video-format                  (Default: Bgra) The video format. (rgb, bgr, rgba, bgra)

  --video-width                   (Default: 1920) The video width, to send to the LiveSwitch server.

  --video-height                  (Default: 1080) The video height, to send to the LiveSwitch server.

  --media-id                      The local media ID.

  --audio-bitrate                 The audio bitrate.

  --video-bitrate                 The video bitrate.

  --video-frame-rate              The video frame-rate, if known, for
                                  signalling.

  --channel-id                    Required. The channel ID.

  --data-channel-label            The data channel label.

  --user-id                       The local user ID.

  --user-alias                    The local user alias.

  --device-id                     The local device ID.

  --device-alias                  The local device alias.

  --client-tag                    The local client tag.

  --client-roles                  The local client roles.

  --connection-tag                The local connection tag.

  --no-audio                      Do not process audio.

  --no-video                      Do not process video.

  --audio-codec                   (Default: Any) The audio codec to negotiate
                                  with LiveSwitch.

  --video-codec                   (Default: Any) The video codec to negotiate
                                  with LiveSwitch.

  --gateway-url                   Required. The gateway URL.

  --application-id                Required. The application ID.

  --shared-secret                 Required. The shared secret for the
                                  application ID.

  --log-level                     (Default: Error) The LiveSwitch log level.
```

## fake

The `fake` verb lets you generate fake media and send it to a LiveSwitch server. Audio is generated as a continuous tone following the argument provided. Video is generated as a sequence of solid-fill images that rotate through the colour wheel.

```shell
  --audio-clock-rate       (Default: 48000) The audio clock rate in Hz. Must be
                           a multiple of 8000. Minimum value is 8000. Maximum
                           value is 96000.

  --audio-channel-count    (Default: 2) The audio channel count. Minimum value
                           is 1. Maximum value is 2.

  --audio-frequency        (Default: 440) The audio frequency in Hz. Minimum
                           value is 20. Maximum value is 20000.

  --audio-amplitude        (Default: 16383) The audio amplitude. Minimum value
                           is 1. Maximum value is 32767.

  --video-format           (Default: Bgr) The video format.

  --video-width            (Default: 640) The video width. Must be a multiple of
                           2.

  --video-height           (Default: 480) The video height. Must be a multiple
                           of 2.

  --video-frame-rate       (Default: 30) The video frame rate. Minimum value is
                           1. Maximum value is 120.

  --media-id               The local media ID.

  --audio-bitrate          The audio bitrate.

  --video-bitrate          The video bitrate.

  --channel-id             Required. The channel ID.

  --data-channel-label     The data channel label.

  --user-id                The local user ID.

  --user-alias             The local user alias.

  --device-id              The local device ID.

  --device-alias           The local device alias.

  --client-tag             The local client tag.

  --client-roles           The local client roles.

  --connection-tag         The local connection tag.

  --no-audio               Do not process audio.

  --no-video               Do not process video.

  --audio-codec            (Default: Any) The audio codec to negotiate with
                           LiveSwitch.

  --video-codec            (Default: Any) The video codec to negotiate with
                           LiveSwitch.

  --gateway-url            Required. The gateway URL.

  --application-id         Required. The application ID.

  --shared-secret          Required. The shared secret for the application ID.

  --log-level              (Default: Error) The LiveSwitch log level.
```

## play

The `play` verb lets you capture media from a local file (or pair of files) and send it to a LiveSwitch server. Note that this is specifically for files that have been recorded in the recording format of `lsconnect` or LiveSwitch itself. To stream arbitrary media, use `ffcapture`. For details, see `Stream from an arbitrary mp4 file` below.

```shell
  --audio-path            The audio file path.

  --video-path            The video file path.

  --media-id              The local media ID.

  --audio-bitrate         The audio bitrate.

  --video-bitrate         The video bitrate.

  --video-width           The video width, if known, for signalling.

  --video-height          The video height, if known, for signalling.

  --video-frame-rate      The video frame-rate, if known, for signalling.

  --channel-id            Required. The channel ID.

  --data-channel-label    The data channel label.

  --user-id               The local user ID.

  --user-alias            The local user alias.

  --device-id             The local device ID.

  --device-alias          The local device alias.

  --client-tag            The local client tag.

  --client-roles          The local client roles.

  --connection-tag        The local connection tag.

  --no-audio              Do not process audio.

  --no-video              Do not process video.

  --audio-codec           (Default: Any) The audio codec to negotiate with
                          LiveSwitch.

  --video-codec           (Default: Any) The video codec to negotiate with
                          LiveSwitch.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

  --log-level             (Default: Error) The LiveSwitch log level.
```

## render

The `render` verb lets you render remote media from a LiveSwitch server to a named pipe.

```shell
  --audio-pipe              The named pipe for audio.

  --video-pipe              The named pipe for video.

  --client                  Use a named pipe client.

  --audio-clock-rate        (Default: 48000) The audio clock rate in Hz. Must be
                            a multiple of 8000. Minimum value is 8000. Maximum
                            value is 48000.

  --audio-channel-count     (Default: 2) The audio channel count. Minimum value
                            is 1. Maximum value is 2.

  --audio-frame-duration    (Default: 20) The audio frame duration in
                            milliseconds. Minimum value is 5. Maximum value is
                            100.

  --video-format            (Default: Bgr) The video format.

  --video-width             The video width.

  --video-height            The video height.

  --connection-id           The remote connection ID or 'mcu'.

  --media-id                The remote media ID.

  --channel-id              Required. The channel ID.

  --data-channel-label      The data channel label.

  --user-id                 The local user ID.

  --user-alias              The local user alias.

  --device-id               The local device ID.

  --device-alias            The local device alias.

  --client-tag              The local client tag.

  --client-roles            The local client roles.

  --connection-tag          The local connection tag.

  --no-audio                Do not process audio.

  --no-video                Do not process video.

  --audio-codec             (Default: Any) The audio codec to negotiate with
                            LiveSwitch.

  --video-codec             (Default: Any) The video codec to negotiate with
                            LiveSwitch.

  --gateway-url             Required. The gateway URL.

  --application-id          Required. The application ID.

  --shared-secret           Required. The shared secret for the application ID.

  --log-level               (Default: Error) The LiveSwitch log level.
```

## ffrender

The `ffrender` verb lets you render remote media from a LiveSwitch server to FFmpeg.

```shell
  --output-args           Required. The FFmpeg output arguments.

  --connection-id         The remote connection ID or 'mcu'.

  --media-id              The remote media ID.

  --channel-id            Required. The channel ID.

  --data-channel-label    The data channel label.

  --user-id               The local user ID.

  --user-alias            The local user alias.

  --device-id             The local device ID.

  --device-alias          The local device alias.

  --client-tag            The local client tag.

  --client-roles          The local client roles.

  --connection-tag        The local connection tag.

  --no-audio              Do not process audio.

  --no-video              Do not process video.

  --audio-codec           (Default: Any) The audio codec to negotiate with
                          LiveSwitch.

  --video-codec           (Default: Any) The video codec to negotiate with
                          LiveSwitch.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

  --log-level             (Default: Error) The LiveSwitch log level.
```

## ndirender

The `ndirender` verb lets you render remote media from a LiveSwitch server to [NDI®](https://ndi.tv/).

```shell
  --stream-name             (Default: LiveswitchConnect) Name of the NDI® stream

  --audio-clock-rate        (Default: 48000) The audio clock rate in Hz. Must be
                            a multiple of 8000. Minimum value is 8000. Maximum
                            value is 48000.

  --audio-channel-count     (Default: 2) The audio channel count. Minimum value
                            is 1. Maximum value is 2.

  --audio-frame-duration    (Default: 20) The audio frame duration in
                            milliseconds. Minimum value is 5. Maximum value is
                            100.

  --video-format            (Default: I420) The video format. Currently only I420 is supported.

  --video-width             (Default: 800) The video width.

  --video-height            (Default: 800) The video height.

  --frame-rate-numerator    (Default: 30000) The frame rate numerator.

  --frame-rate-denominator  (Default: 1000) The frame rate denominator.

  --connection-id           Required. The remote connection ID or 'mcu'.

  --audio-bitrate           The audio bitrate.

  --video-bitrate           The video bitrate.

  --channel-id              Required. The channel ID.

  --data-channel-label      The data channel label.

  --region                  The local region.

  --user-id                 The local user ID.

  --user-alias              The local user alias.

  --device-id               The local device ID.

  --device-alias            The local device alias.

  --client-tag              The local client tag.

  --client-roles            The local client roles.

  --connection-tag          The local connection tag.

  --no-audio                Do not process audio.

  --no-video                Do not process video.

  --audio-codec             (Default: Any) The audio codec to negotiate with
                            LiveSwitch.

  --video-codec             (Default: Any) The video codec to negotiate with
                            LiveSwitch.

  --gateway-url             Required. The gateway URL.

  --application-id          Required. The application ID.

  --shared-secret           Required. The shared secret for the application ID.

  --log-level               (Default: Error) The LiveSwitch log level.
```

## log

The `log` verb lets you log remote media frame details from a LiveSwitch server to standard output (stdout).

```shell
  --audio-log             (Default: audio: {duration}ms
                          {encoding}/{clockRate}/{channelCount} frame received
                          ({footprint} bytes) for SSRC {synchronizationSource}
                          and timestamp {timestamp}) The audio log template.
                          Uses curly-brace syntax. Valid variables: footprint,
                          duration, clockRate, channelCount, mediaStreamId,
                          rtpStreamId, sequenceNumber, synchronizationSource,
                          systemTimestamp, timestamp, encoding, applicationId,
                          channelId, userId, userAlias, deviceId, deviceAlias,
                          clientId, clientTag, connectionId, connectionTag,
                          mediaId

  --video-log             (Default: video: {width}x{height} {encoding} frame
                          received ({footprint} bytes) for SSRC
                          {synchronizationSource} and timestamp {timestamp}) The
                          video log template. Uses curly-brace syntax. Valid
                          variables: footprint, width, height, mediaStreamId,
                          rtpStreamId, sequenceNumber, synchronizationSource,
                          systemTimestamp, timestamp, encoding, applicationId,
                          channelId, userId, userAlias, deviceId, deviceAlias,
                          clientId, clientTag, connectionId, connectionTag,
                          mediaId

  --connection-id         The remote connection ID or 'mcu'.

  --media-id              The remote media ID.

  --channel-id            Required. The channel ID.

  --data-channel-label    The data channel label.

  --user-id               The local user ID.

  --user-alias            The local user alias.

  --device-id             The local device ID.

  --device-alias          The local device alias.

  --client-tag            The local client tag.

  --client-roles          The local client roles.

  --connection-tag        The local connection tag.

  --no-audio              Do not process audio.

  --no-video              Do not process video.

  --audio-codec           (Default: Any) The audio codec to negotiate with
                          LiveSwitch.

  --video-codec           (Default: Any) The video codec to negotiate with
                          LiveSwitch.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

  --log-level             (Default: Error) The LiveSwitch log level.
```

## record

The `record` verb lets you record remote media from a LiveSwitch server to a local pair or files.

```shell
  --output-path           (Default: .) The output path for the recordings. Uses
                          curly-brace syntax. Valid variables: applicationId,
                          channelId, userId, userAlias, deviceId, deviceAlias,
                          clientId, clientTag, connectionId, connectionTag,
                          mediaId

  --output-file-name      (Default: {connectionId}) The output file name
                          template. Uses curly-brace syntax. Valid variables:
                          applicationId, channelId, userId, userAlias, deviceId,
                          deviceAlias, clientId, clientTag, connectionId,
                          connectionTag, mediaId

  --connection-id         The remote connection ID or 'mcu'.

  --media-id              The remote media ID.

  --channel-id            Required. The channel ID.

  --data-channel-label    The data channel label.

  --user-id               The local user ID.

  --user-alias            The local user alias.

  --device-id             The local device ID.

  --device-alias          The local device alias.

  --client-tag            The local client tag.

  --client-roles          The local client roles.

  --connection-tag        The local connection tag.

  --no-audio              Do not process audio.

  --no-video              Do not process video.

  --audio-codec           (Default: Any) The audio codec to negotiate with
                          LiveSwitch.

  --video-codec           (Default: Any) The video codec to negotiate with
                          LiveSwitch.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

  --log-level             (Default: Error) The LiveSwitch log level.
```

## intercept

The `intercept` verb lets you forward audio and/or video packets to a specific destination IP addreress and port to allow for lawful intercept via packet tracing.

```shell
  --audio-port            The destination port for audio packets.

  --video-port            The destination port for video packets.

  --audio-ip-address      (Default: 127.0.0.1) The destination IP address for
                          audio packets.

  --video-ip-address      (Default: 127.0.0.1) The destination IP address for
                          video packets.

  --connection-id         The remote connection ID or 'mcu'.

  --media-id              The remote media ID.

  --channel-id            Required. The channel ID.

  --data-channel-label    The data channel label.

  --user-id               The local user ID.

  --user-alias            The local user alias.

  --device-id             The local device ID.

  --device-alias          The local device alias.

  --client-tag            The local client tag.

  --client-roles          The local client roles.

  --connection-tag        The local connection tag.

  --no-audio              Do not process audio.

  --no-video              Do not process video.

  --audio-codec           (Default: Any) The audio codec to negotiate with
                          LiveSwitch.

  --video-codec           (Default: Any) The video codec to negotiate with
                          LiveSwitch.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

  --log-level             (Default: Error) The LiveSwitch log level.
```

## Loopback Example

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

Open a terminal and run `lsconnect render` with the following arguments:

-   `--gateway-url` <https://v1.liveswitch.fm:8443/sync>
-   `--application-id` my-app-id
-   `--shared-secret` --replaceThisWithYourOwnSharedSecret--
-   `--audio-pipe` my-audio-pipe
-   `--video-pipe` my-video-pipe
-   `--channel-id` (the channel ID from your web browser)
-   `--connection-id` (the connection ID from your web browser)

```shell
lsconnect render --gateway-url https://v1.liveswitch.fm:8443/sync --application-id my-app-id --shared-secret=--replaceThisWithYourOwnSharedSecret-- --audio-pipe my-audio-pipe --video-pipe my-video-pipe --channel-id {CHANNEL_ID} --connection-id {CONNECTION_ID}
```

You should see logs indicating that:

1.  A renderer client has been registered.
2.  The remote connection has been found.
3.  A renderer connection has been connected.

`lsconnect render` is now waiting for either:

1.  An exit signal (e.g. Ctrl+C)
2.  The remote connection to disconnect.

Either of these will result in a graceful disconnection from LiveSwitch.

Open a new terminal and run `lsconnect capture` in a new console tab with the following arguments:

-   `--gateway-url` <https://v1.liveswitch.fm:8443/sync>
-   `--application-id` my-app-id
-   `--shared-secret` --replaceThisWithYourOwnSharedSecret--
-   `--audio-pipe` my-audio-pipe
-   `--video-pipe` my-video-pipe
-   `--channel-id` (the channel ID from your web browser)

```shell
lsconnect capture --gateway-url https://v1.liveswitch.fm:8443/sync --application-id my-app-id --shared-secret=--replaceThisWithYourOwnSharedSecret-- --audio-pipe my-audio-pipe --video-pipe my-video-pipe --channel-id {CHANNEL_ID}
```

You should see logs indicating that:

1.  A capturer client has been registered.
2.  A capturer connection has been connected.
3.  The audio and video pipes are connected. (You should see this in the first console tab as well.)

`lsconnect capture` is now waiting for an exit signal (e.g. Ctrl+C), at which point it will gracefully disconnect from LiveSwitch.

Check your web browser! You should see your camera echoed back as a new connection!

## Using FFmpeg

[FFmpeg](https://www.ffmpeg.org/) can read from and write to the named pipes that `lsconnect` creates. The process is slightly different for Windows and Linux.

> `ffmpeg` always runs in the client role, so make sure your `lsconnect capture` command uses the `--server` flag.

.NET Core on Windows uses Windows named pipes. Use `\\.\pipe\{name}` (e.g. `\\.\pipe\my-video-pipe`) to identify a pipe to FFmpeg on Windows.

.NET Core on Linux **does not use Linux named pipes** (FIFOs). [Unix domain sockets are used instead](https://github.com/dotnet/corefx/pull/6833), so use `unix://tmp/CoreFxPipe_{name}` (e.g. `unix://tmp/CoreFxPipe_my-video-pipe`) to identify a pipe to FFmpeg on Linux.

## FFmpeg Loopback Example

Let's put `ffmpeg` between `lsconnect render` and `lsconnect capture`.

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

Open a terminal and use `lsconnect` to render to pipes named `audio-in` and `video-in`:

```shell
lsconnect render ... --audio-pipe audio-in --video-pipe video-in
```

Open a new terminal and use `lsconnect` to capture from pipes named `audio-out` and `video-out`:

```shell
lsconnect capture ... --audio-pipe audio-out --video-pipe video-out --server
```

> Note the `--server` flag!

We are now waiting for a client connection from `ffmpeg` to these pipes.

Finally, open another terminal and connect `audio-in` to `audio-out` and `video-in` to `video-out` using `ffmpeg`:

**Linux**

```shell
ffmpeg -y -f s16le -i unix://tmp/CoreFxPipe_audio-1 -f rawvideo -video_size 640x480 -pix_fmt bgr24 -i unix://tmp/CoreFxPipe_video-1 -f s16le unix://tmp/CoreFxPipe_audio-2 -f rawvideo unix://tmp/CoreFxPipe_video-2
```

**Windows**

```shell
ffmpeg -y -f s16le -i \\.\pipe\audio-1 -f rawvideo -video_size 640x480 -pix_fmt bgr24 -i \\.\pipe\video-1 -f s16le \\.\pipe\audio-2 -f rawvideo \\.\pipe\video-2
```

Audio and video should now be flowing!

> `s16le` indicates signed, 16-bit, little-endian PCM.

> `pix_fmt` should match the `--video-format` used by `lsconnect capture` and `lsconnect render`.

> `bgr24` indicates 24-bit BGR images and `rawvideo` indicates raw media frames without headers.

## FFmpeg RTSP Example (Simple)

Let's use `ffmpeg` to inject a live RTSP stream into LiveSwitch using the `ffcapture` verb.

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

Open a terminal and use `lsconnect ffcapture` to connect a live RTSP feed:

```shell
lsconnect ffcapture ... --input-args="-rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov"
```

Audio and video should now be flowing!

## FFmpeg RTSP Example (Advanced)

Let's use `ffmpeg` to inject a live RTSP stream into LiveSwitch using the `capture` verb.

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

Open a terminal and use `lsconnect` to capture from pipes named `audio-rtsp` and `video-rtsp`:

```shell
lsconnect capture ... --audio-pipe audio-rtsp --video-pipe video-rtsp --server
```

> Note the `--server` flag!

We are now waiting for a client connection from `ffmpeg` to these pipes.

Open a new terminal and use `ffmpeg` to direct a live RTSP feed to these pipes:

**Linux**

```shell
ffmpeg -y -rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov -map 0:0 -f s16le -ar 48000 -ac 2 unix://tmp/CoreFxPipe_audio-rtsp -map 0:1 -f rawvideo -video_size 240x160 -pix_fmt bgr24 unix://tmp/CoreFxPipe_video-rtsp
```

**Windows**

```shell
ffmpeg -y -rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov -map 0:0 -f s16le -ar 48000 -ac 2 \\.\pipe\audio-rtsp -map 0:1 -f rawvideo -video_size 240x160 -pix_fmt bgr24 \\.\pipe\video-rtsp
```

Audio and video should now be flowing!

## Screen share from Windows

```shell
lsconnect ffcapture ... --input-args="-f gdigrab -framerate 30 -i desktop" --no-audio
```

## Screen share from macOS

Get the device index for the screen to share:

```shell
ffmpeg -f avfoundation -list_devices true -i ""
```

Replace "2" with your device index from above:

```shell
lsconnect ffcapture ... --input-args="-f avfoundation -i \"2\" -r 30 -vf scale=1536:960" --no-audio
```

## Stream an MP4 file

Sample file taken from here: <https://file-examples.com/index.php/sample-video-files/>
Note that `-stream_loop -1` plays the file on a loop, `-r 30` indicates 30fps and `-vf scale=640:480` scales to 640x480. You may need to tweak these depending on your file and output requirements.

```shell
lsconnect ffcapture ... --input-args="-stream_loop -1 -i test.mp4 -r 30 -vf scale=640:480"
```

## Broadcast an RTMP stream from OBS

Assuming a 1920x1080@30fps screen capture stream from OBS out to an RTMP server, you can direct that stream to LiveSwitch efficiently. The `video-mode` is `noencode` so `ffmpeg` acts as a passthrough, forwarding the RTP packets through to `lsconnect` without decoding or modifying them. Because of this, we have to declare the `video-encoding`, which from OBS is typically `h264`. By declaring both a `video-encoding` and `video-codec`, we are also forcing transcoding, which allows us to respond to keyframe requests as remote clients access the feed more efficiently than can be done by relying on the OBS feed alone. In this case, we are selecting `vp8` as the `video-codec` to negotiate with the LiveSwitch server, which is efficient and broadly supported by WebRTC clients.

If we know the `video-width`, `video-height`, and/or `video-frame-rate` ahead of time, it is helpful to declare them so this information can be signalled to the LiveSwitch server to assist with bitrate estimation and bandwidth adaptation. The `video-bitrate` can also be set if desired.

```shell
lsconnect ffcapture ... --input-args=-i rtmp://{server}/live/obs \
  --video-mode noencode --video-encoding h264 --video-codec vp8 \
  --video-width 1920 --video-height 1080 --video-frame-rate 30 \
  --video-bitrate 3000
```

## Stream from LiveSwitch to an RTMP server (e.g. YouTube)

You can stream the content in a LiveSwitch channel to an RTMP server. The following is an example of how to stream to YouTube's RTMP server.

```shell
lsconnect ffrender ... --output-args="-f flv rtmp://a.rtmp.youtube.com/live2/<YouTube Stream Key>"
```

## Using NDI®

For Windows: You'll need to install the NDI® Runtime found here: <http://new.tk/NDIRedistV4>
Add the runtime directory to the Path environment variable. Default: `C:\Program Files\NDI.tv\NDI 4 Runtime\v4`

For Mac: You'll need to install the NDI® SDK found here: <https://downloads.ndi.tv/SDK/NDI_SDK_Mac/InstallNDISDK_v4_Apple.pkg>

## Contact

To learn more, visit [frozenmountain.com](https://www.frozenmountain.com) or [liveswitch.io](https://www.liveswitch.io).

For inquiries, contact [sales@frozenmountain.com](mailto:sales@frozenmountain.com).

All contents copyright © Frozen Mountain Software.

NDI® is a registered trademark of NewTek, Inc.