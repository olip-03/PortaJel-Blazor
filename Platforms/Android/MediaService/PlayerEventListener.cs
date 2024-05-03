using Android.Media;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Metadata;
using Com.Google.Android.Exoplayer2.Text;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Video;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0618, CS0612, CA1422 // Type or member is obsolete

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    internal class PlayerEventListener : Com.Google.Android.Exoplayer2.IPlayer.IListener
    {
        // Define actions for each method in PlayerEventListener
        public Action<Com.Google.Android.Exoplayer2.Audio.AudioAttributes?>? OnAudioAttributesChangedImpl { get; set; }
        public Action<int>? OnAudioSessionIdChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.IPlayer.Commands?>? OnAvailableCommandsChangedImpl { get; set; }
        public Action<CueGroup?>? OnCuesImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.DeviceInfo?>? OnDeviceInfoChangedImpl { get; set; }
        public Action<int, bool>? OnDeviceVolumeChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.IPlayer?, Com.Google.Android.Exoplayer2.IPlayer.Events?>? OnEventsImpl { get; set; }
        public Action<bool>? OnIsLoadingChangedImpl { get; set; }
        public Action<bool>? OnIsPlayingChangedImpl { get; set; }
        public Action<bool>? OnLoadingChangedImpl { get; set; }
        public Action<long>? OnMaxSeekToPreviousPositionChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.MediaItem?, int>? OnMediaItemTransitionImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.MediaMetadata?>? OnMediaMetadataChangedImpl { get; set; }
        public Action<Metadata?>? OnMetadataImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.PlaybackParameters?>? OnPlaybackParametersChangedImpl { get; set; }
        public Action<int>? OnPlaybackStateChangedImpl { get; set; }
        public Action<int>? OnPlaybackSuppressionReasonChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.PlaybackException?>? OnPlayerErrorImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.PlaybackException?>? OnPlayerErrorChangedImpl { get; set; }
        public Action<bool, int>? OnPlayerStateChangedImpl { get; set; }
        public Action<MediaMetadata>? OnPlaylistMetadataChangedImpl { get; set; }
        public Action<bool, int>? OnPlayWhenReadyChangedImpl { get; set; }
        public Action<int>? OnPositionDiscontinuityImpl { get; set; }
        public Action? OnRenderedFirstFrameImpl { get; set; }
        public Action<int>? OnRepeatModeChangedImpl { get; set; }
        public Action<long>? OnSeekBackIncrementChangedImpl { get; set; }
        public Action<long>? OnSeekForwardIncrementChangedImpl { get; set; }
        public Action<bool>? OnShuffleModeEnabledChangedImpl { get; set; }
        public Action<bool>? OnSkipSilenceEnabledChangedImpl { get; set; }
        public Action<int, int>? OnSurfaceSizeChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.Timeline?, int>? OnTimelineChangedImpl { get; set; }
        public Action<Com.Google.Android.Exoplayer2.Tracks?>? OnTracksChangedImpl { get; set; }
        public Action<TrackSelectionParameters?>? OnTrackSelectionParametersChangedImpl { get; set; }
        public Action<VideoSize?>? OnVideoSizeChangedImpl { get; set; }
        public Action<float>? OnVolumeChangedImpl { get; set; }

        public nint Handle => throw new NotImplementedException();
        public int JniIdentityHashCode => throw new NotImplementedException();

        public JniObjectReference PeerReference => throw new NotImplementedException();

        public JniPeerMembers JniPeerMembers => throw new NotImplementedException();

        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Disposed()
        {
            throw new NotImplementedException();
        }

        public void DisposeUnlessReferenced()
        {
            throw new NotImplementedException();
        }

        public void Finalized()
        {
            throw new NotImplementedException();
        }

        public void OnAudioAttributesChanged(Com.Google.Android.Exoplayer2.Audio.AudioAttributes? audioAttributes)
        {
            if (OnAudioAttributesChangedImpl != null)
            {
                OnAudioAttributesChangedImpl(audioAttributes);
            }
        }

        public void OnAudioSessionIdChanged(int audioSessionId)
        {
            if (OnAudioSessionIdChangedImpl != null)
            {
                OnAudioSessionIdChangedImpl(audioSessionId);
            }
        }

        public void OnAvailableCommandsChanged(Com.Google.Android.Exoplayer2.IPlayer.Commands? availableCommands)
        {
            if(OnAvailableCommandsChangedImpl != null)
            {
                OnAvailableCommandsChangedImpl(availableCommands);
            }
        }

        public void OnCues(CueGroup? cueGroup)
        {
            if(OnCuesImpl != null)
            {
                OnCuesImpl(cueGroup);
            }
        }

        public void OnDeviceInfoChanged(Com.Google.Android.Exoplayer2.DeviceInfo? deviceInfo)
        {
            if(OnDeviceInfoChangedImpl != null)
            {
                OnDeviceInfoChangedImpl(deviceInfo);
            }
        }

        public void OnDeviceVolumeChanged(int volume, bool muted)
        {
            if(OnDeviceVolumeChangedImpl != null)
            {
                OnDeviceVolumeChangedImpl(volume, muted);
            }
        }

        public void OnEvents(Com.Google.Android.Exoplayer2.IPlayer? player, Com.Google.Android.Exoplayer2.IPlayer.Events? events)
        {
            if(OnEventsImpl != null)
            {
                OnEventsImpl(player, events);
            }
        }

        public void OnIsLoadingChanged(bool isLoading)
        {
            if(OnIsLoadingChangedImpl != null)
            {
                OnIsLoadingChangedImpl(isLoading);
            }
        }

        public void OnIsPlayingChanged(bool isPlaying)
        {
            if(OnIsPlayingChangedImpl != null)
            {
                OnIsPlayingChangedImpl(isPlaying);
            }
        }

        public void OnLoadingChanged(bool isLoading)
        {
            if(OnLoadingChangedImpl != null)
            {
                OnLoadingChangedImpl(isLoading);
            }
        }

        public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
        {
            if(OnMaxSeekToPreviousPositionChangedImpl != null)
            {
                OnMaxSeekToPreviousPositionChangedImpl(maxSeekToPreviousPositionMs);
            }
        }

        public void OnMediaItemTransition(Com.Google.Android.Exoplayer2.MediaItem? mediaItem, int reason)
        {
            if(OnMediaItemTransitionImpl != null)
            {
                OnMediaItemTransitionImpl(mediaItem, reason);
            }
        }

        public void OnMediaMetadataChanged(Com.Google.Android.Exoplayer2.MediaMetadata? mediaMetadata)
        {
            if(OnMediaMetadataChangedImpl != null)
            {
                OnMediaMetadataChangedImpl(mediaMetadata);
            }
        }

        public void OnMetadata(Metadata? metadata)
        {
            if(OnMetadataImpl != null)
            {
                OnMetadataImpl(metadata);
            }
        }

        public void OnPlaybackParametersChanged(Com.Google.Android.Exoplayer2.PlaybackParameters? playbackParameters)
        {
            throw new NotImplementedException();
        }

        public void OnPlaybackStateChanged(int playbackState)
        {
            throw new NotImplementedException();
        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerError(Com.Google.Android.Exoplayer2.PlaybackException? error)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerErrorChanged(Com.Google.Android.Exoplayer2.PlaybackException? error)
        {
            throw new NotImplementedException();
        }


        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            throw new NotImplementedException();
        }

        public void OnPlaylistMetadataChanged(MediaMetadata? mediaMetadata)
        {
            throw new NotImplementedException();
        }

        public void OnPlaylistMetadataChanged(Com.Google.Android.Exoplayer2.MediaMetadata? mediaMetadata)
        {
            throw new NotImplementedException();
        }

        public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
        {
            throw new NotImplementedException();
        }

        public void OnPositionDiscontinuity(int reason)
        {
            throw new NotImplementedException();
        }

        public void OnRenderedFirstFrame()
        {
            throw new NotImplementedException();
        }

        public void OnRepeatModeChanged(int repeatMode)
        {
            throw new NotImplementedException();
        }

        public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
        {
            throw new NotImplementedException();
        }

        public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
        {
            throw new NotImplementedException();
        }

        public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
        {
            throw new NotImplementedException();
        }

        public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
        {
            throw new NotImplementedException();
        }

        public void OnSurfaceSizeChanged(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void OnTimelineChanged(Com.Google.Android.Exoplayer2.Timeline? timeline, int reason)
        {
            throw new NotImplementedException();
        }

        public void OnTracksChanged(Com.Google.Android.Exoplayer2.Tracks? tracks)
        {
            throw new NotImplementedException();
        }

        public void OnTrackSelectionParametersChanged(TrackSelectionParameters? parameters)
        {
            throw new NotImplementedException();
        }

        public void OnVideoSizeChanged(VideoSize? videoSize)
        {
            throw new NotImplementedException();
        }

        public void OnVolumeChanged(float volume)
        {
            throw new NotImplementedException();
        }

        public void SetJniIdentityHashCode(int value)
        {
            throw new NotImplementedException();
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
            throw new NotImplementedException();
        }

        public void SetPeerReference(JniObjectReference reference)
        {
            throw new NotImplementedException();
        }

        public void UnregisterFromRuntime()
        {
            throw new NotImplementedException();
        }
    }
}
