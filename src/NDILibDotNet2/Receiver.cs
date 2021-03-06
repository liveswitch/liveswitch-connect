﻿using NewTek;
using NewTek.NDI;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace NewTek.NDI
{
    public class AudioFrameReceivedEventArgs : EventArgs
    {
        public AudioFrame16bpp Frame { get; set; }
    }

    public class VideoFrameReceivedEventArgs : EventArgs
    {
        public VideoFrame Frame { get; set; }
    }

    public struct FramesReceived
    {
        public Int64 AudioTotal;
        public Int64 AudioDropped;
        public Int64 VideoTotal;
        public Int64 VideoDropped;
    }

    // If you do not use this control, you can remove this file
    // and remove the dependency on naudio.
    // Alternatively you can also remove any naudio related entries
    // and use it for video only, but don't forget that you will still need
    // to free any audio frames received.
    public class Receiver : IDisposable, INotifyPropertyChanged
    {
        [Category("NewTek NDI"),
        Description("If true (default) received audio will be sent to the default Windows audio playback device.")]
        public bool IsAudioEnabled
        {
            get { return _audioEnabled; }
            set
            {
                if (value != _audioEnabled)
                {
                    NotifyPropertyChanged("IsAudioEnabled");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("If true (default) received video will be sent to the screen.")]
        public bool IsVideoEnabled
        {
            get { return _videoEnabled; }
            set
            {
                if (value != _videoEnabled)
                {
                    NotifyPropertyChanged("IsVideoEnabled");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("Does the current source support PTZ functionality?")]
        public bool IsPtz
        {
            get { return _isPtz; }
            set
            {
                if (value != _isPtz)
                {
                    NotifyPropertyChanged("IsPtz");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("Does the current source support record functionality?")]
        public bool IsRecordingSupported
        {
            get { return _canRecord; }
            set
            {
                if (value != _canRecord)
                {
                    NotifyPropertyChanged("IsRecordingSupported");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("The web control URL for the current device, as a String, or an Empty String if not supported.")]
        public String WebControlUrl
        {
            get { return _webControlUrl; }
            set
            {
                if (value != _webControlUrl)
                {
                    NotifyPropertyChanged("WebControlUrl");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("Are we connected to the NDI source?")]
        public bool Connected
        {
            get { return _connected; }
            private set {
                if (_connected != value)
                {
                    _connected = value;
                    ConnectionStateChange?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Category("NewTek NDI"),
        Description("Are we receiving frames from the NDI source?")]
        public bool ReceivingFrames
        {
            get { return _receivingFrames; }
            private set {
                if (_receivingFrames != value)
                {
                    _receivingFrames = value;
                    ConnectionStateChange?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Receiver(String sourceName, String receiverName)
        {
            if (String.IsNullOrEmpty(sourceName))
                throw new ArgumentException("Source name can not be null or empty.", sourceName);
            _sourceName = sourceName;

            if (String.IsNullOrEmpty(receiverName))
                throw new ArgumentException("Receiver name can not be null or empty.", receiverName);
            _receiverName = receiverName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

#region PTZ Methods
        public bool SetPtzZoom(double value)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_zoom(_recvInstancePtr, (float)value);
        }

        public bool SetPtzZoomSpeed(double value)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_zoom_speed(_recvInstancePtr, (float)value);
        }

        public bool SetPtzPanTilt(double pan, double tilt)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_pan_tilt(_recvInstancePtr, (float)pan, (float)tilt);
        }

        public bool SetPtzPanTiltSpeed(double panSpeed, double tiltSpeed)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_pan_tilt_speed(_recvInstancePtr, (float)panSpeed, (float)tiltSpeed);
        }

        public bool PtzStorePreset(int index)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero || index < 0 || index > 99)
                return false;

            return NDIlib.recv_ptz_store_preset(_recvInstancePtr, index);
        }

        public bool PtzRecallPreset(int index, double speed)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero || index < 0 || index > 99)
                return false;

            return NDIlib.recv_ptz_recall_preset(_recvInstancePtr, index, (float)speed);
        }

        public bool PtzAutoFocus()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_auto_focus(_recvInstancePtr);
        }

        public bool SetPtzFocusSpeed(double speed)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_focus_speed(_recvInstancePtr, (float)speed);
        }

        public bool PtzWhiteBalanceAuto()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_white_balance_auto(_recvInstancePtr);
        }

        public bool PtzWhiteBalanceIndoor()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_white_balance_indoor(_recvInstancePtr);
        }

        public bool PtzWhiteBalanceOutdoor()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_white_balance_outdoor(_recvInstancePtr);
        }

        public bool PtzWhiteBalanceOneShot()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_white_balance_oneshot(_recvInstancePtr);
        }

        public bool PtzWhiteBalanceManual(double red, double blue)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_white_balance_manual(_recvInstancePtr, (float)red, (float)blue);
        }

        public bool PtzExposureAuto()
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_exposure_auto(_recvInstancePtr);
        }

        public bool PtzExposureManual(double level)
        {
            if (!_isPtz || _recvInstancePtr == IntPtr.Zero)
                return false;

            return NDIlib.recv_ptz_exposure_manual(_recvInstancePtr, (float)level);
        }

#endregion PTZ Methods

#region Recording Methods
    // This will start recording.If the recorder was already recording then the message is ignored.A filename is passed in as a ‘hint’.Since the recorder might 
	// already be recording(or might not allow complete flexibility over its filename), the filename might or might not be used.If the filename is empty, or 
	// not present, a name will be chosen automatically. 
    public bool RecordingStart(String filenameHint = "")
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        bool retVal = false;

        if(String.IsNullOrEmpty(filenameHint))
        {
            retVal = NDIlib.recv_recording_start(_recvInstancePtr, IntPtr.Zero);
        }
        else
        {
            // convert to an unmanaged UTF8 IntPtr
            IntPtr fileNamePtr = NDI.UTF.StringToUtf8(filenameHint);

            retVal = NDIlib.recv_recording_start(_recvInstancePtr, IntPtr.Zero);

            // don't forget to free it
            Marshal.FreeHGlobal(fileNamePtr);            
        }

        return retVal;
    }

	// Stop recording.
	public bool RecordingStop()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

	    return NDIlib.recv_recording_stop(_recvInstancePtr);
    }

    
    public bool RecordingSetAudioLevel(double level)
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        return NDIlib.recv_recording_set_audio_level(_recvInstancePtr, (float)level);
    }

    public bool IsRecording()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        return NDIlib.recv_recording_is_recording(_recvInstancePtr);
    }

    public String GetRecordingFilename()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return String.Empty;

        IntPtr filenamePtr = NDIlib.recv_recording_get_filename(_recvInstancePtr);
        if(filenamePtr == IntPtr.Zero)
        {
            return String.Empty;
        }
        else
        {
            String filename = NDI.UTF.Utf8ToString(filenamePtr);

            // free it
            NDIlib.recv_free_string(_recvInstancePtr, filenamePtr);

            return filename;
        }
    }

    public String GetRecordingError()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
            return String.Empty;

        IntPtr errorPtr = NDIlib.recv_recording_get_error(_recvInstancePtr);
        if (errorPtr == IntPtr.Zero)
        {
            return String.Empty;
        }
        else
        {
            String error = NDI.UTF.Utf8ToString(errorPtr);

            // free it
            NDIlib.recv_free_string(_recvInstancePtr, errorPtr);

            return error;
        }
    }

    public bool GetRecordingTimes(ref NDIlib.recv_recording_time_t recordingTimes)
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
            return false;

        return NDIlib.recv_recording_get_times(_recvInstancePtr, ref recordingTimes);
    }

#endregion Recording Methods

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Receiver()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // tell the thread to exit
                    _exitThread = true;

                    // wait for it to exit
                    if (_receiveThread != null)
                    {
                        _receiveThread.Join();

                        _receiveThread = null;
                    }
                }
                
                // Destroy the receiver
                if (_recvInstancePtr != IntPtr.Zero)
                {
                    NDIlib.recv_destroy(_recvInstancePtr);
                    _recvInstancePtr = IntPtr.Zero;
                }
                
                // Not required, but "correct". (see the SDK documentation)
                NDIlib.destroy();

                _disposed = true;
            }
        }

        private bool _disposed = false;

        // connect to an NDI source in our Dictionary by name
        public void Connect(NDIlib.recv_color_format_e colorFormat)
        {
            // just in case we're already connected
            Disconnect();

            // a source_t to describe the source to connect to.
            NDIlib.source_t source_t = new NDIlib.source_t()
            {
                p_ndi_name = UTF.StringToUtf8(_sourceName)
            };

            // make a description of the receiver we want
            NDIlib.recv_create_v3_t recvDescription = new NDIlib.recv_create_v3_t()
            {
                // the source we selected
                source_to_connect_to = source_t,

                // we want BGRA frames for this example
                color_format = colorFormat,

                // we want full quality - for small previews or limited bandwidth, choose lowest
                bandwidth = _videoEnabled ? NDIlib.recv_bandwidth_e.recv_bandwidth_highest : NDIlib.recv_bandwidth_e.recv_bandwidth_audio_only,

                // let NDIlib deinterlace for us if needed
                allow_video_fields = false,

                // The name of the NDI receiver to create. This is a NULL terminated UTF8 string and should be
                // the name of receive channel that you have. This is in many ways symettric with the name of
                // senders, so this might be "Channel 1" on your system.
                p_ndi_recv_name = UTF.StringToUtf8(_receiverName)
            };
            
            // create a new instance connected to this source
            _recvInstancePtr = NDIlib.recv_create_v3(ref recvDescription);

            // free the memory we allocated with StringToUtf8
            Marshal.FreeHGlobal(source_t.p_ndi_name);
            Marshal.FreeHGlobal(recvDescription.p_ndi_recv_name);

            // did it work?
            System.Diagnostics.Debug.Assert(_recvInstancePtr != IntPtr.Zero, "Failed to create NDI receive instance.");

            if (_recvInstancePtr != IntPtr.Zero)
            {
                // We are now going to mark this source as being on program output for tally purposes (but not on preview)
                SetTallyIndicators(true, false);

                // start up a thread to receive on
                _receiveThread = new Thread(ReceiveThreadProc) { IsBackground = true, Name = "NdiExampleReceiveThread" };
                _receiveThread.Start();
            }
        }

        public void Disconnect()
        {
            // in case we're connected, reset the tally indicators
            SetTallyIndicators(false, false);

            // check for a running thread
            if (_receiveThread != null)
            {
                // tell it to exit
                _exitThread = true;

                // wait for it to end
                _receiveThread.Join();
            }

            // reset thread defaults
            _receiveThread = null;
            _exitThread = false;

            // Destroy the receiver
            NDIlib.recv_destroy(_recvInstancePtr);

            // set it to a safe value
            _recvInstancePtr = IntPtr.Zero;

            // set function status to defaults
            IsPtz = false;
            IsRecordingSupported = false;
            WebControlUrl = String.Empty;
        }

        public FramesReceived GetPerformance()
        {
            FramesReceived fr = new FramesReceived();

            if (_recvInstancePtr != IntPtr.Zero)
            {
                NDIlib.recv_performance_t totalFrames = new NDIlib.recv_performance_t();
                NDIlib.recv_performance_t droppedFrames = new NDIlib.recv_performance_t();
    
                NDIlib.recv_get_performance(_recvInstancePtr, ref totalFrames, ref droppedFrames);
    
                fr.AudioTotal = totalFrames.audio_frames;
                fr.VideoTotal = totalFrames.video_frames;
                fr.AudioDropped = droppedFrames.audio_frames;
                fr.VideoDropped = droppedFrames.video_frames;
            }

            return fr;
        }

        void SetTallyIndicators(bool onProgram, bool onPreview)
        {
            // we need to have a receive instance
            if (_recvInstancePtr != IntPtr.Zero)
            {
                // set up a state descriptor
                NDIlib.tally_t tallyState = new NDIlib.tally_t()
                {
                    on_program = onProgram,
                    on_preview = onPreview
                };

                // set it on the receiver instance
                NDIlib.recv_set_tally(_recvInstancePtr, ref tallyState);
            }
        }

        // the receive thread runs though this loop until told to exit
        void ReceiveThreadProc()
        {
            while (!_exitThread && _recvInstancePtr != IntPtr.Zero)
            {
                // The descriptors
                NDIlib.video_frame_v2_t videoFrame = new NDIlib.video_frame_v2_t();
                NDIlib.audio_frame_v2_t audioFrame = new NDIlib.audio_frame_v2_t();
                NDIlib.metadata_frame_t metadataFrame = new NDIlib.metadata_frame_t();

                switch (NDIlib.recv_capture_v2(_recvInstancePtr, ref videoFrame, ref audioFrame, ref metadataFrame, 1000))
                {
                // No data
                case NDIlib.frame_type_e.frame_type_none:
                    // No data received
                    ReceivingFrames = false;
                    break;

                // frame settings - check for extended functionality
                case NDIlib.frame_type_e.frame_type_status_change:
                    // check for PTZ
                    IsPtz = NDIlib.recv_ptz_is_supported(_recvInstancePtr);

                    // Check for recording
                    IsRecordingSupported = NDIlib.recv_recording_is_supported(_recvInstancePtr);

                    // Check for a web control URL
                    // We must free this string ptr if we get one.
                    IntPtr webUrlPtr = NDIlib.recv_get_web_control(_recvInstancePtr);
                    if (webUrlPtr == IntPtr.Zero)
                    {
                        WebControlUrl = String.Empty;
                    }
                    else
                    {
                        // convert to managed String
                        WebControlUrl = NDI.UTF.Utf8ToString(webUrlPtr);

                        // Don't forget to free the string ptr
                        NDIlib.recv_free_string(_recvInstancePtr, webUrlPtr);
                    }

                    break;

                // Video data
                case NDIlib.frame_type_e.frame_type_video:
                    Connected = true;
                    ReceivingFrames = true;

                    // if not enabled, just discard
                    // this can also occasionally happen when changing sources
                    if (!_videoEnabled || videoFrame.p_data == IntPtr.Zero)
                    {
                        // alreays free received frames
                        NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);

                        break;
                    }

                    VideoFrameReceivedEventArgs videoArgs = new VideoFrameReceivedEventArgs();
                    videoArgs.Frame = new VideoFrame(videoFrame);
                    VideoFrameReceived?.Invoke(this, videoArgs);

                    // free frames that were received AFTER use!
                    NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);
                    break;

                // audio is beyond the scope of this example
                case NDIlib.frame_type_e.frame_type_audio:
                    Connected = true;
                    ReceivingFrames = true;

                    // if no audio or disabled, nothing to do
                    if (!_audioEnabled || audioFrame.p_data == IntPtr.Zero || audioFrame.no_samples == 0)
                    {
                        // alreays free received frames
                        NDIlib.recv_free_audio_v2(_recvInstancePtr, ref audioFrame);

                        break;
                    }

                    // we're working in bytes, so take the size of a 16 bit sample into account
                    int sizeInBytes = (int)audioFrame.no_samples * (int)audioFrame.no_channels * sizeof(short);

                    // NDI uses planar, but we'll return interleaved which Pcm uses by default.
                    // create an interleaved frame and convert from the one we received
                    NDIlib.audio_frame_interleaved_16s_t interleavedFrame = new NDIlib.audio_frame_interleaved_16s_t()
                    {
                        sample_rate = audioFrame.sample_rate,
                        no_channels = audioFrame.no_channels,
                        no_samples = audioFrame.no_samples,
                        timecode = audioFrame.timecode
                    };

                    // we need a managed byte array to add to buffered provider
                    byte[] audBuffer = new byte[sizeInBytes];

                    // pin the byte[] and get a GC handle to it
                    // doing it this way saves an expensive Marshal.Alloc/Marshal.Copy/Marshal.Free later
                    // the data will only be moved once, during the fast interleave step that is required anyway
                    GCHandle handle = GCHandle.Alloc(audBuffer, GCHandleType.Pinned);

                    // access it by an IntPtr and use it for our interleaved audio buffer
                    interleavedFrame.p_data = handle.AddrOfPinnedObject();

                    // Convert from float planar to 16 bit interleaved audio
                    NDIlib.util_audio_to_interleaved_16s_v2(ref audioFrame, ref interleavedFrame);

                    AudioFrameReceivedEventArgs audioArgs = new AudioFrameReceivedEventArgs();
                    audioArgs.Frame = new AudioFrame16bpp(interleavedFrame);
                    AudioFrameReceived?.Invoke(this, audioArgs);

                    // release the pin on the byte[]
                    // never try to access p_data after the byte[] has been unpinned!
                    // that IntPtr will no longer be valid.
                    handle.Free();

                    // free the frame that was received
                    NDIlib.recv_free_audio_v2(_recvInstancePtr, ref audioFrame);
                    break;

                // Metadata
                case NDIlib.frame_type_e.frame_type_metadata:

                    // UTF-8 strings must be converted for use - length includes the terminating zero
                    //String metadata = Utf8ToString(metadataFrame.p_data, metadataFrame.length-1);

                    //System.Diagnostics.Debug.Print(metadata);

                    // free frames that were received
                    NDIlib.recv_free_metadata(_recvInstancePtr, ref metadataFrame);
                    break;
                
                case NDIlib.frame_type_e.frame_type_error:
                    Connected = false;
                    break;
                }
            }
        }

        // a pointer to our unmanaged NDI receiver instance
        IntPtr _recvInstancePtr = IntPtr.Zero;

        // a thread to receive frames on so that the UI is still functional
        Thread _receiveThread = null;

        // a way to exit the thread safely
        bool _exitThread = false;

        public event EventHandler<AudioFrameReceivedEventArgs> AudioFrameReceived;
        public event EventHandler<VideoFrameReceivedEventArgs> VideoFrameReceived;
        public event EventHandler ConnectionStateChange;

        private readonly String _sourceName;
        private readonly String _receiverName;

        // should we send audio to Windows or not?
        private bool _audioEnabled = true;

        // should we send video to Windows or not?
        private bool _videoEnabled = true;

        private bool _connected = false;
        private bool _receivingFrames = false;

        private bool _isPtz = false;
        private bool _canRecord = false;
        private String _webControlUrl = String.Empty;
    }
}
