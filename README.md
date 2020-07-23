# LiveSwitch Connect CLI

![build](https://github.com/liveswitch/liveswitch-connect/workflows/build/badge.svg) ![license](https://img.shields.io/badge/License-MIT-yellow.svg) ![release](https://img.shields.io/github/v/release/liveswitch/liveswitch-connect.svg)

The LiveSwitch Connect CLI lets you send and receive media to and from LiveSwitch.

Requires .NET Core 3.1 or newer.

## Building

Use `dotnet publish` to create a single, self-contained file for a specific platform/architecture:

### Windows
```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o win
```

### macOS
```
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o osx
```

### Linux
```
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o linux
```

Alternatively, use `dotnet build` to create a platform-agnostic bundle (the .NET Core runtime must be installed):

```
dotnet build
```

Using this approach will generate a library instead of an executable. Use `dotnet lsconnect.dll` instead of `lsconnect` to run it.

## Usage

```
lsconnect [verb] [options]
```

### Verbs
```
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

## Shell

The `shell` verb starts an interactive shell that lets you monitor clients and connections in a LiveSwitch Server.

### Usage
```
  --gateway-url       Required. The gateway URL.

  --application-id    Required. The application ID.

  --shared-secret     Required. The shared secret for the application ID.
```

Once the `shell` is active:

```
  register    Registers a client.

  exit        Exits the shell.
```

After you `register`:

```
  unregister    Unregisters the client.

  join          Joins a channel.

  exit          Exits the shell.
```

After you `join` a channel (e.g. `join my-channel-id`):

```
  leave          Leaves the channel.

  clients        Prints remote client details to stdout.

  connections    Prints remote connection details to stdout.

  exit           Exits the shell.
```

The `clients` verb has additional options:

```
  --ids        Print IDs only.

  --listen     Listen for join/leave events. (Press Q to stop.)
```

The `connections` verb has additional options as well:

```
  --ids        Print IDs only.

  --listen     Listen for open/close events. (Press Q to stop.)
```

## Capture

The `capture` verb lets you capture local media from a named pipe and send it to a LiveSwitch server.

### Usage
```
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

  --audio-bitrate           (Default: 32) The requested audio bitrate.

  --video-bitrate           (Default: 1000) The requested video bitrate.

  --gateway-url             Required. The gateway URL.

  --application-id          Required. The application ID.

  --shared-secret           Required. The shared secret for the application ID.

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

  --h264-encoder            (Default: Auto) The H.264 encoder to use for the
                            H.264 codec.

  --h264-decoder            (Default: Auto) The H.264 decoder to use for the
                            H.264 codec.
```

## FFCapture

The `ffcapture` verb lets you capture local media from FFmpeg and send it to a LiveSwitch server.

### Usage
```
  --input-args                    Required. The FFmpeg input arguments.

  --audio-mode                    (Default: LSEncode) Where audio is encoded.

  --video-mode                    (Default: LSEncode) Where video is encoded.

  --ffencode-keyframe-interval    (Default: 30) The keyframe interval for video.
                                  Only used if video-mode is ffencode.

  --media-id                      The local media ID.

  --audio-bitrate                 (Default: 32) The requested audio bitrate.

  --video-bitrate                 (Default: 1000) The requested video bitrate.

  --gateway-url                   Required. The gateway URL.

  --application-id                Required. The application ID.

  --shared-secret                 Required. The shared secret for the
                                  application ID.

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

  --h264-encoder                  (Default: Auto) The H.264 encoder to use for the
                                  H.264 codec.

  --h264-decoder                  (Default: Auto) The H.264 decoder to use for the
                                  H.264 codec.
```

## Fake

The `fake` verb lets you generate fake media and send it to a LiveSwitch server. Audio is generated as a continuous tone following the argument provided. Video is generated as a sequence of solid-fill images that rotate through the colour wheel.

### Usage
```
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

  --video-width            (Default: 640) The video width. Must be a multiiple
                           of 2.

  --video-height           (Default: 480) The video height. Must be a multiiple
                           of 2.

  --video-frame-rate       (Default: 30) The video frame rate. Minimum value is
                           1. Maximum value is 120.

  --media-id               The local media ID.

  --audio-bitrate          (Default: 32) The requested audio bitrate.

  --video-bitrate          (Default: 1000) The requested video bitrate.

  --gateway-url            Required. The gateway URL.

  --application-id         Required. The application ID.

  --shared-secret          Required. The shared secret for the application ID.

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

  --h264-encoder           (Default: Auto) The H.264 encoder to use for the
                           H.264 codec.

  --h264-decoder           (Default: Auto) The H.264 decoder to use for the
                           H.264 codec.
```

## Play

The `play` verb lets you capture media from a local file (or pair of files) and send it to a LiveSwitch server. Note that this is specifically for files that have been recorded in the recording format of `lsconnect` or LiveSwitch itself. To stream arbitrary media, use `ffcapture`. For details, see `Stream from an arbitrary mp4 file` below.

### Usage
```
  --audio-path            The audio file path.

  --video-path            The video file path.

  --media-id              The local media ID.

  --audio-bitrate         (Default: 32) The requested audio bitrate.

  --video-bitrate         (Default: 1000) The requested video bitrate.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

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

  --h264-encoder          (Default: Auto) The H.264 encoder to use for the
                          H.264 codec.

  --h264-decoder          (Default: Auto) The H.264 decoder to use for the
                          H.264 codec.
```

## Render

The `render` verb lets you render remote media from a LiveSwitch server to a named pipe.

### Usage
```
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

  --connection-id           Required. The remote connection ID or 'mcu'.

  --gateway-url             Required. The gateway URL.

  --application-id          Required. The application ID.

  --shared-secret           Required. The shared secret for the application ID.

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

  --h264-encoder            (Default: Auto) The H.264 encoder to use for the
                            H.264 codec.

  --h264-decoder            (Default: Auto) The H.264 decoder to use for the
                            H.264 codec.
```

## FFRender

The `ffrender` verb lets you render remote media from a LiveSwitch server to FFmpeg.

### Usage
```
  --output-args           Required. The FFmpeg output arguments.

  --connection-id         Required. The remote connection ID or 'mcu'.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

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

  --h264-encoder          (Default: Auto) The H.264 encoder to use for the
                          H.264 codec.

  --h264-decoder          (Default: Auto) The H.264 decoder to use for the
                          H.264 codec.
```

## Log

The `log` verb lets you log remote media frame details from a LiveSwitch server to standard output (stdout).

### Usage
```
  --audio-log             (Default: audio: {duration}ms
                          {codec}/{clockRate}/{channelCount} frame received
                          ({footprint} bytes) for SSRC {synchronizationSource}
                          and timestamp {timestamp}) The audio log template.
                          Uses curly-brace syntax. Valid variables: footprint,
                          duration, clockRate, channelCount, mediaStreamId,
                          rtpStreamId, sequenceNumber, synchronizationSource,
                          systemTimestamp, timestamp, codec, applicationId,
                          channelId, userId, userAlias, deviceId, deviceAlias,
                          clientId, clientTag, connectionId, connectionTag,
                          mediaId

  --video-log             (Default: video: {width}x{height} {codec} frame
                          received ({footprint} bytes) for SSRC
                          {synchronizationSource} and timestamp {timestamp}) The
                          video log template. Uses curly-brace syntax. Valid
                          variables: footprint, width, height, mediaStreamId,
                          rtpStreamId, sequenceNumber, synchronizationSource,
                          systemTimestamp, timestamp, codec, applicationId,
                          channelId, userId, userAlias, deviceId, deviceAlias,
                          clientId, clientTag, connectionId, connectionTag,
                          mediaId

  --connection-id         Required. The remote connection ID or 'mcu'.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

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

  --h264-encoder          (Default: Auto) The H.264 encoder to use for the
                          H.264 codec.

  --h264-decoder          (Default: Auto) The H.264 decoder to use for the
                          H.264 codec.
```

## Record

The `record` verb lets you record remote media from a LiveSwitch server to a local pair or files.

### Usage
```
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

  --connection-id         Required. The remote connection ID or 'mcu'.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

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

  --h264-encoder          (Default: Auto) The H.264 encoder to use for the
                          H.264 codec.

  --h264-decoder          (Default: Auto) The H.264 decoder to use for the
                          H.264 codec.
```

## Intercept

The `intercept` verb lets you forward audio and/or video packets to a specific destination IP addreress and port to allow for lawful intercept via packet tracing.

### Usage
```
  --audio-port            The destination port for audio packets.

  --video-port            The destination port for video packets.

  --audio-ip-address      (Default: 127.0.0.1) The destination IP address for
                          audio packets.

  --video-ip-address      (Default: 127.0.0.1) The destination IP address for
                          video packets.

  --connection-id         Required. The remote connection ID or 'mcu'.

  --gateway-url           Required. The gateway URL.

  --application-id        Required. The application ID.

  --shared-secret         Required. The shared secret for the application ID.

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

  --h264-encoder          (Default: Auto) The H.264 encoder to use for the
                          H.264 codec.

  --h264-decoder          (Default: Auto) The H.264 decoder to use for the
                          H.264 codec.
```

## Loopback Example

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

Run `lsconnect render` with the following arguments:

- `--gateway-url` https://v1.liveswitch.fm:8443/sync
- `--application-id` my-app-id
- `--shared-secret` --replaceThisWithYourOwnSharedSecret--
- `--audio-pipe` my-audio-pipe
- `--video-pipe` my-video-pipe
- `--channel-id` (the channel ID from your web browser)
- `--connection-id` (the connection ID from your web browser)

```
lsconnect render --gateway-url https://v1.liveswitch.fm:8443/sync --application-id my-app-id --shared-secret=--replaceThisWithYourOwnSharedSecret-- --audio-pipe my-audio-pipe --video-pipe my-video-pipe --channel-id {CHANNEL_ID} --connection-id {CONNECTION_ID}
```

You should see logs indicating that:

1. A renderer client has been registered.
1. The remote connection has been found.
1. A renderer connection has been connected.

`lsconnect render` is now waiting for either:

1. An exit signal (e.g. Ctrl+C)
1. The remote connection to disconnect.

Either of these will result in a graceful disconnection from LiveSwitch.

Now run `lsconnect capture` in a new console tab with the following arguments:

- `--gateway-url` https://v1.liveswitch.fm:8443/sync
- `--application-id` my-app-id
- `--shared-secret` --replaceThisWithYourOwnSharedSecret--
- `--audio-pipe` my-audio-pipe
- `--video-pipe` my-video-pipe
- `--channel-id` (the channel ID from your web browser)

```
lsconnect capture --gateway-url https://v1.liveswitch.fm:8443/sync --application-id my-app-id --shared-secret=--replaceThisWithYourOwnSharedSecret-- --audio-pipe my-audio-pipe --video-pipe my-video-pipe --channel-id {CHANNEL_ID}
```

You should see logs indicating that:

1. A capturer client has been registered.
1. A capturer connection has been connected.
1. The audio and video pipes are connected. (You should see this in the first console tab as well.)

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

### Console Tab 1

Let's render to pipes named `audio-in` and `video-in`:
```
lsconnect render ... --audio-pipe audio-in --video-pipe video-in
```

We are now waiting for a client connection from `ffmpeg` to these input pipes.

### Console Tab 2

Let's capture from pipes named `audio-out` and `video-out`:
```
lsconnect capture ... --audio-pipe audio-out --video-pipe video-out --server
```

> Note the `--server` flag!

We are now waiting for a client connection from `ffmpeg` to these output pipes.

### Console Tab 3

Let's connect `audio-in` to `audio-out` and `video-in` to `video-out` using `ffmpeg`:

#### Windows:
```
ffmpeg -y -f s16le -i \\.\pipe\audio-1 -f rawvideo -video_size 640x480 -pix_fmt bgr24 -i \\.\pipe\video-1 -f s16le \\.\pipe\audio-2 -f rawvideo \\.\pipe\video-2
```

#### Linux:
```
ffmpeg -y -f s16le -i unix://tmp/CoreFxPipe_audio-1 -f rawvideo -video_size 640x480 -pix_fmt bgr24 -i unix://tmp/CoreFxPipe_video-1 -f s16le unix://tmp/CoreFxPipe_audio-2 -f rawvideo unix://tmp/CoreFxPipe_video-2
```

Audio and video should now be flowing!

> `s16le` indicates signed, 16-bit, little-endian PCM.

> `pix_fmt` should match the `--video-format` used by `lsconnect capture` and `lsconnect render`.

> `bgr24` indicates 24-bit BGR images and `rawvideo` indicates raw media frames without headers.


## FFmpeg RTSP Example (Simple)

Let's use `ffmpeg` to inject a live RTSP stream into LiveSwitch using the `ffcapture` verb.

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

### Console Tab

Let's provide `ffmpeg` arguments to connect a live RTSP feed:
```
lsconnect ffcapture ... --input-args="-rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov"
```

Audio and video should now be flowing!

## FFmpeg RTSP Example (Advanced)

Let's use `ffmpeg` to inject a live RTSP stream into LiveSwitch using the `capture` verb.

Open the [LiveSwitch Demo](https://v1.liveswitch.fm/) in a web browser and join a channel. Take note of the `channel-id` from the join dialog and the `connection-id` from the console output.

### Console Tab 1

Let's capture from pipes named `audio-rtsp` and `video-rtsp`:
```
lsconnect capture ... --audio-pipe audio-rtsp --video-pipe video-rtsp --server
```

> Note the `--server` flag!

We are now waiting for a client connection from `ffmpeg` to these output pipes.

### Console Tab 2

Let's connect a live RTSP feed to `audio-out` and `video-out` using `ffmpeg`:

#### Windows:
```
ffmpeg -y -rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov -map 0:0 -f s16le -ar 48000 -ac 2 \\.\pipe\audio-rtsp -map 0:1 -f rawvideo -video_size 240x160 -pix_fmt bgr24 \\.\pipe\video-rtsp
```

#### Linux:
```
ffmpeg -y -rtsp_transport tcp -i rtsp://3.84.6.190/vod/mp4:BigBuckBunny_115k.mov -map 0:0 -f s16le -ar 48000 -ac 2 unix://tmp/CoreFxPipe_audio-rtsp -map 0:1 -f rawvideo -video_size 240x160 -pix_fmt bgr24 unix://tmp/CoreFxPipe_video-rtsp
```

Audio and video should now be flowing!

## Other Examples

### Stream your screen from Windows
```
lsconnect ffcapture ... --input-args="-f gdigrab -framerate 30 -i desktop" --no-audio
```

### Stream your screen from macOS

Get the device index for the screen to share:
```
ffmpeg -f avfoundation -list_devices true -i ""
```

Replace "2" with your device index from above:
```
lsconnect ffcapture ... --input-args="-f avfoundation -i \"2\" -r 30 -vf scale=1536:960" --no-audio
```

### Stream from an arbitrary mp4 file
Sample file taken from here: https://file-examples.com/index.php/sample-video-files/
Note that `-stream_loop -1` plays the file on a loop, `-r 30` indicates 30fps and `-vf scale=640:480` scales to 640x480. You may need to tweak these depending on your file and output requirements.
```
lsconnect ffcapture ... --input-args="-stream_loop -1 -i test.mp4 -r 30 -vf scale=640:480"
```

## Contact

To learn more, visit [frozenmountain.com](https://www.frozenmountain.com) or [liveswitch.io](https://www.liveswitch.io).

For inquiries, contact [sales@frozenmountain.com](mailto:sales@frozenmountain.com).

All contents copyright © Frozen Mountain Software.
