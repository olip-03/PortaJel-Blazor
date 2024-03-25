using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Media.Session;
using Android.Widget;
using AndroidX.Core.App;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Metadata;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Text;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Video;
using Org.Apache.Commons.Logging;
using PortaJel_Blazor.Data;
using System.Timers;
using static Android.App.LauncherActivity;

#pragma warning disable CS0612, CS0618 // Type or member is obsolete

// Android Implementation
// https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui.MediaElement/Views/MediaManager.android.cs
// https://putridparrot.com/blog/android-foreground-service-using-maui/
// Might be able to dig through her to get some more info on how this shit's all supposed to work
// https://github.com/Baseflow/XamarinMediaManager

// Oh my god fuck all of this
// Carve out my fucking eyes I never want to read Kotlin again 
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService
    {
        [Obsolete] IExoPlayer? Exoplayer;
        NotificationManager? notificationManager = null;
        MediaSessionCompat? mediaSession;
        Notification.MediaStyle mediaStyle = new();
        Notification.Builder? notificationBuilder;
        Notification.Action.Builder? notificationActionBuilder;
        System.Timers.Timer myTimer = null;

        [Obsolete]
        public partial void Initalize()
        {
            isPlaying = false;

            CheckPermissions();
            //Android.Content.Intent intent = new Android.Content.Intent(Android.Provider.Settings.ActionNotificationPolicyAccessSettings);
            //intent.AddFlags(ActivityFlags.NewTask);
            //Platform.AppContext.StartActivity(intent);

            var HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
            var MainDataSource = new ProgressiveMediaSource.Factory(HttpDataSourceFactory);
            if(Platform.AppContext != null && MainDataSource != null)
            {
                Exoplayer = new IExoPlayer.Builder(Platform.AppContext).SetMediaSourceFactory(MainDataSource).Build();
                Exoplayer.RepeatMode = IPlayer.RepeatModeOff;

                mediaSession = new MediaSessionCompat(Platform.AppContext, "PlayerService");
                mediaStyle.SetMediaSession((Android.Media.Session.MediaSession.Token)mediaSession.SessionToken.GetToken());

                notificationBuilder = new(Platform.AppContext);
                // var playPauseIntent = new Android.Content.Intent(Platform.AppContext, );

                //MediaSessionC

                // notification = notificationBuilder.SetStyle(mediaStyle).Build();
                // notification.Actions.Add(pauseAction);

                string deviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            }

            // Do yer notification channel stuffs 
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = "PortaJel";
            var channelDescription = "Notification Channel for the PortaJel Music Streaming App for Jellyfin";
            var channel = new NotificationChannel(AppInfo.PackageName, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            notificationManager = (NotificationManager)Platform.AppContext.GetSystemService(MainActivity.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
        
        private void CheckPermissions()
        {
            const int requestLocationId = 0;

            string[] notifPerms =
            {
                Android.Manifest.Permission.PostNotifications,
                Android.Manifest.Permission.AccessNotificationPolicy
            };

            if ((int)Build.VERSION.SdkInt < 33) return;

            if (Platform.AppContext.CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Android.Content.PM.Permission.Granted
                || Platform.AppContext.CheckSelfPermission(Android.Manifest.Permission.AccessNotificationPolicy) != Android.Content.PM.Permission.Granted
                && Platform.CurrentActivity != null)
            {
                Platform.CurrentActivity.RequestPermissions(notifPerms, requestLocationId);
            }
        }
        
        private void ThrowNotification(string CHANNEL_ID)
        {
            Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(Platform.AppContext, pendingIntentId, intent, PendingIntentFlags.OneShot);

            // Instantiate the builder and set notification elements, including pending intent:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, CHANNEL_ID)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("Sample Notification")
                .SetContentText("Hello World! This is my first action notification!")
                .SetSmallIcon(Resource.Drawable.ic_mtrl_checked_circle);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            //NotificationManager notificationManager = Platform.AppContext.GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
        
        private void UpdatePlaystate(Object source, ElapsedEventArgs e)
        {
            if(Exoplayer != null && Microsoft.Maui.Controls.Application.Current != null)
            {
                Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
                {
                    long duration = Exoplayer == null ? 0 : Exoplayer.Duration;
                    long position = Exoplayer == null ? 0 : Exoplayer.CurrentPosition;

                    if (duration > 0)
                    {
                        MauiProgram.mainPage.UpdatePlaystate(duration, position);
                    }
                });
            }
        }
        private void UpdatePlaystate(long? setPosition = -1)
        {
            long duration = Exoplayer == null ? 0 : Exoplayer.Duration;
            long position = Exoplayer == null ? 0 : Exoplayer.CurrentPosition;
            if (setPosition != -1 && setPosition != null)
            {
                position = (long)setPosition;
            }

            if(duration > 0) 
            {
                MauiProgram.mainPage.UpdatePlaystate(duration, position);
            }
        }
        
        public partial void Play()
        {
            // ThrowNotification(AppInfo.PackageName);
            UpdateCurrentlyPlaying();
            if (Exoplayer!= null)
            {
                isPlaying = true;
                Exoplayer.Play();

                MauiProgram.mainPage.RefreshPlayState();
            }
        }

        public async void UpdateCurrentlyPlaying()
        {
            Song? getSong = null;
            Album? getAlbum = null;

            if(Exoplayer == null || Microsoft.Maui.Controls.Application.Current == null)
            {
                return;
            }

            Exoplayer.ClearMediaItems();

            int dequeuedListCount = 0;
            await Task.Run(() =>
            {
                // Add dequeued songs
                foreach (Song queuedSong in MauiProgram.mediaService.songQueue.dequeuedList)
                {
                    MediaItem firstItem = MediaItem.FromUri(queuedSong.streamUrl);
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() => Exoplayer.AddMediaItem(firstItem));
                    dequeuedListCount++;
                }
                foreach (Song nextupSong in MauiProgram.mediaService.nextUpQueue.dequeuedList)
                {
                    MediaItem firstItem = MediaItem.FromUri(nextupSong.streamUrl);
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() => Exoplayer.AddMediaItem(firstItem));
                    dequeuedListCount++;
                }

                // Add queued songs
                foreach (Song queuedSong in MauiProgram.mediaService.songQueue.GetQueue())
                {
                    MediaItem firstItem = MediaItem.FromUri(queuedSong.streamUrl);
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() => Exoplayer.AddMediaItem(firstItem));
                }
                foreach (Song nextupSong in MauiProgram.mediaService.nextUpQueue.GetQueue())
                {
                    MediaItem firstItem = MediaItem.FromUri(nextupSong.streamUrl);
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() => Exoplayer.AddMediaItem(firstItem));
                }
            });
            Exoplayer.SeekToDefaultPosition(dequeuedListCount);
            Exoplayer.Prepare();
            Exoplayer.PlayWhenReady = true;
            Exoplayer.Play();

            myTimer = new System.Timers.Timer(100);
            myTimer.Elapsed += new ElapsedEventHandler(UpdatePlaystate);
            myTimer.AutoReset = true;
            myTimer.Start();

            if (getSong != null)
            {
                // await getSong.GetAlbumAsync();

                //getAlbum = await getSong.GetAlbumAsync();
                if(getSong.album != Album.Empty)
                {
                    MauiProgram.currentAlbumGuid = getSong.album.id;
                }
                else
                {
                    MauiProgram.currentAlbumGuid = Guid.Empty;
                }
            }
            else
            {
                MauiProgram.currentAlbumGuid = Guid.Empty;
            }
        }

        public partial void Pause()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Pause();

                MauiProgram.mainPage.RefreshPlayState();
            }
        }

        public partial void TogglePlay()
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        public partial void ToggleShuffle()
        {
            if (Exoplayer != null)
            {
                Exoplayer.ShuffleModeEnabled = !Exoplayer.ShuffleModeEnabled;
                shuffleOn = Exoplayer.ShuffleModeEnabled;
            }
        }

        public partial void ToggleRepeat()
        {
            if (Exoplayer != null)
            {
                switch (Exoplayer.RepeatMode)
                {
                    case IPlayer.RepeatModeAll:
                        Exoplayer.RepeatMode = IPlayer.RepeatModeOff;
                        repeatMode = 0;
                        break;
                    case IPlayer.RepeatModeOne:
                        Exoplayer.RepeatMode = IPlayer.RepeatModeAll;
                        repeatMode = 2;
                        break;
                    case IPlayer.RepeatModeOff:
                        Exoplayer.RepeatMode = IPlayer.RepeatModeOne;
                        repeatMode = 1;
                        break;
                }
            }
        }

        public partial void NextTrack()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Next();
            }
        }

        public partial void PreviousTrack()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Previous();
            }
        }

        public partial void SeekTo(long position)
        {
            if(Exoplayer != null)
            {
                Exoplayer.SeekTo(position);
            }
        }

        internal class PlaystateListener : Java.Lang.Object, IPlayer.IListener
        {
            public void OnAudioAttributesChanged(AudioAttributes? audioAttributes)
            {
                //throw new NotImplementedException();
            }

            public void OnAudioSessionIdChanged(int audioSessionId)
            {
                //throw new NotImplementedException();
            }

            public void OnAvailableCommandsChanged(IPlayer.Commands? availableCommands)
            {
                //throw new NotImplementedException();
            }

            public void OnCues(CueGroup? cueGroup)
            {
                //throw new NotImplementedException();
            }

            public void OnDeviceInfoChanged(Com.Google.Android.Exoplayer2.DeviceInfo? deviceInfo)
            {
                //throw new NotImplementedException();
            }

            public void OnDeviceVolumeChanged(int volume, bool muted)
            {
                // throw new NotImplementedException();
            }

            public void OnEvents(IPlayer? player, IPlayer.Events? events)
            {
                // throw new NotImplementedException();
            }

            public void OnIsLoadingChanged(bool isLoading)
            {
                // throw new NotImplementedException();
            }

            public void OnIsPlayingChanged(bool isPlaying)
            {

            }

            public void OnLoadingChanged(bool isLoading)
            {
                // throw new NotImplementedException();
            }

            public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
            {
                // throw new NotImplementedException();
            }

            public void OnMediaItemTransition(MediaItem? mediaItem, int reason)
            {
                // throw new NotImplementedException();
            }

            public void OnMediaMetadataChanged(MediaMetadata? mediaMetadata)
            {
                // throw new NotImplementedException();
            }

            public void OnMetadata(Metadata? metadata)
            {
               //  throw new NotImplementedException();
            }

            public void OnPlaybackParametersChanged(PlaybackParameters? playbackParameters)
            {
                // throw new NotImplementedException();
            }

            public void OnPlaybackStateChanged(int playbackState)
            {
                MauiProgram.mediaService.UpdatePlaystate(playbackState);
                // throw new NotImplementedException();
            }

            public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
            {
               // throw new NotImplementedException();
            }

            public void OnPlayerError(PlaybackException? error)
            {
                // throw new NotImplementedException();
            }

            public void OnPlayerErrorChanged(PlaybackException? error)
            {
                //throw new NotImplementedException();
            }

            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                MauiProgram.mediaService.UpdatePlaystate();
            }

            public void OnPlaylistMetadataChanged(MediaMetadata? mediaMetadata)
            {
                // throw new NotImplementedException();
            }

            public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
            {
                // throw new NotImplementedException();
            }

            public void OnPositionDiscontinuity(int reason)
            {
                // throw new NotImplementedException();
            }

            public void OnRenderedFirstFrame()
            {
                // throw new NotImplementedException();
            }

            public void OnRepeatModeChanged(int repeatMode)
            {
                // throw new NotImplementedException();
            }

            public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
            {
                // throw new NotImplementedException();
            }

            public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
            {
                // throw new NotImplementedException();
            }

            public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
            {
                // throw new NotImplementedException();
            }

            public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
            {
                // throw new NotImplementedException();
            }

            public void OnSurfaceSizeChanged(int width, int height)
            {
                // throw new NotImplementedException();
            }

            public void OnTimelineChanged(Timeline? timeline, int reason)
            {
                // throw new NotImplementedException();
            }

            public void OnTracksChanged(Tracks? tracks)
            {
                // throw new NotImplementedException();
            }

            public void OnTrackSelectionParametersChanged(TrackSelectionParameters? parameters)
            {
                // throw new NotImplementedException();
            }

            public void OnVideoSizeChanged(VideoSize? videoSize)
            {
                // throw new NotImplementedException();
            }

            public void OnVolumeChanged(float volume)
            {
                // throw new NotImplementedException();
            }
        }
    }
}
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
