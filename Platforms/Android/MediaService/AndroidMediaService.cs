using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.InputMethodServices;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Views;
using AndroidX.Lifecycle;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Extractor.Mp3;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using Java.Lang;
using Microsoft.Maui.Controls.PlatformConfiguration;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using PortaJel_Blazor.Data;
using System.Collections;
using System.Collections.Generic;
using static Android.Provider.MediaStore.Audio;
using MediaMetadata = Android.Media.MediaMetadata;

#pragma warning disable CS0618, CS0612, CA1422 // Type or member is obsolete
// Referce
// https://github.com/xamarin/monodroid-samples/blob/archived-xamarin/android5.0/MediaBrowserService/MediaBrowserService/MusicService.cs
// See lines 169 through 360 for Kotlin implementation of Exoplayer + notification service 
// https://github.com/AkaneTan/Gramophone/blob/30773abb7df9317d50d159df7f40b8d9c9418520/app/src/main/kotlin/org/akanework/gramophone/logic/GramophonePlaybackService.kt#L577
//
// Looks like an easy enough Kotlin tutorial for the MediaStyle notificaiton mrow :3 
// https://medium.com/@anafthdev_/create-mediastyle-notification-in-android-anafthdev-70fe7df3397e
// https://android-developers.googleblog.com/2020/08/playing-nicely-with-media-controls.html
//
// This is for the MusicBrainz lookup I want to add, for comparing strings. Not relevent to the Android Media Service here but it will need to be implemented and I have this file open already
// https://medium.com/@tarakshah/this-article-explains-how-to-check-the-similarity-between-two-string-in-percentage-or-score-from-0-83e206bf6bf5
//
// To bind service with attached data, see explanations:
// https://stackoverflow.com/questions/45574022/how-to-pass-a-value-when-starting-a-background-service-on-android-using-xamarin

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    [Service(Name = "PortaJel.MediaService", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }

        public IExoPlayer? Player = null;
        PlayerEventListener PlayerEventListener = new();
        private long currentDuration = -1;
        private long fullDuration = -1;
        private int repeatMode = 0;

        MediaSession? MediaSession = null;
        MediaMetadata? mediaSessionMetadata = null;
        MediaSessionCallback? mediaSessionCallback = null;
        MediaMetadata? mediaMetadata = null;

        Notification? playerNotification = null;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public int PlayingIndex { get; private set; } = 0;
        public BaseMusicItem? playingFrom { get; private set; } = null;
        public Song CurrentlyPlaying { get; private set; } = Song.Empty;
        public List<Song> QueueRepresentation { get; private set; } = new();
        public List<Song> MainQueue { get; private set; } = new();
        private int QueueStartIndex { get; set; } = -1;

        private string channedId = AppInfo.PackageName;

        public AndroidMediaService()
        {

        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var channel = GetNotificationChannel();
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;

            MediaSession = new MediaSession(context, channedId);
            PlaybackState.Builder? psBuilder = new PlaybackState.Builder();
            mediaSessionCallback = new MediaSessionCallback();

            // Define callback functions here
            if (mediaSessionCallback != null)
            {
                mediaSessionCallback.OnPlayImpl = () =>
                {
                    Play();
                };
                mediaSessionCallback.OnPauseImpl = () =>
                {
                    Pause();
                };
                mediaSessionCallback.OnSkipToNextImpl = () =>
                {
                    Next();
                };
                mediaSessionCallback.OnSkipToPreviousImpl = () =>
                {
                    Previous();
                };
                mediaSessionCallback.OnMediaButtonEventImpl = (Intent? intent) =>
                {
                    if (intent != null)
                    {
                        if (intent.Action == Intent.ActionMediaButton)
                        {
                            KeyEvent? eventVal = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
                            if (eventVal != null)
                            {
                                if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_PLAY_PAUSE"))
                                {
                                    TogglePlay();
                                }
                                if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_PLAY"))
                                {
                                    Play();
                                }
                                if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_PAUSE"))
                                {
                                    Pause();
                                }
                                if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_NEXT"))
                                {
                                    Next();
                                }
                                if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_PREVIOUS"))
                                {
                                    Previous();
                                }
                            }
                        }
                    }
                };
                MediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
                MediaSession.SetCallback(mediaSessionCallback);
            }

            var startIntent = new Intent(context, typeof(MainActivity));
            if (intent != null)
            {
                startIntent.PutExtras(intent);
            }

            var playIntent = new Intent(context, typeof(MainActivity));
            if (intent != null)
            {
                startIntent.PutExtras(intent);
            }
 
            UpdateNotification();
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, playerNotification);
            return base.OnStartCommand(startIntent, flags, startId);
        }

        public override IBinder OnBind(Intent? intent)
        {
            this.Binder = new MediaServiceBinder(this);
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;

            var HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
            var MainDataSource = new ProgressiveMediaSource.Factory(HttpDataSourceFactory);
            if (MainDataSource != null)
            {
                DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();

                // Extractor factory for additional functionality
                DefaultExtractorsFactory extractorsFactory = new DefaultExtractorsFactory();
                extractorsFactory.SetConstantBitrateSeekingEnabled(true);
                extractorsFactory.SetMp3ExtractorFlags(Mp3Extractor.FlagEnableIndexSeeking);

                SimpleExoPlayer.Builder? newBuilder = new SimpleExoPlayer.Builder(this).SetMediaSourceFactory(new DefaultMediaSourceFactory(context, extractorsFactory));
                if (newBuilder != null)
                { //Create the player here!
                    Player = newBuilder.Build();
                }
                if (Player != null)
                {
                    Player.RepeatMode = IPlayer.RepeatModeOff;
                }
                string deviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            }

            // Add all songs that might have been added on initialization
            foreach (Song item in MainQueue)
            {
                AddSong(item);
            }

            // Add event for when the Player changes track
            PlayerEventListener.OnMediaItemTransitionImpl = (MediaItem? song, int index) =>
            {
                if (Player != null)
                {
                    if(GetQueue().AllSongs.Count() > PlayingIndex)
                    {
                        PlayingIndex = Player.CurrentMediaItemIndex;
                        CurrentlyPlaying = GetQueue().AllSongs[PlayingIndex];
                        UpdateMetadata();
                        // Check if the song we've just played was a part of the queue, if so remove it from the queue list \
                        if (song == null) return;
                        if (string.IsNullOrWhiteSpace(song.MediaId)) return; // Only Queued Songs have a MediaID 
                        QueueStartIndex = index;
                        int queueIndex = MainQueue.FindIndex(queueSong => queueSong.playlistId == song.MediaId);
                        if (queueIndex >= 0)
                        {
                            MainQueue.RemoveAt(queueIndex);
                        }
                    }
                }
            };
            PlayerEventListener.OnPlayerStateChangedImpl = (bool playWhenReady, int playbackState) =>
            {
                if (playbackState == IPlayer.StateReady && Player != null)
                {
                    fullDuration = Player.Duration;
                    CurrentlyPlaying = GetQueue().AllSongs[PlayingIndex];
                    UpdateMetadata();
                }
            };
            PlayerEventListener.OnIsPlayingChangedImpl = (isPlaying) =>
            {
                UpdatePlaybackState();
            };

            Com.Google.Android.Exoplayer2.Audio.AudioAttributes? audioAttributes = new Com.Google.Android.Exoplayer2.Audio.AudioAttributes.Builder()
                .SetUsage((int)AudioUsageKind.Media)
                .SetContentType((int)AudioContentType.Music)
                .Build();

            if (Player != null)
            {
                Player.AddListener(PlayerEventListener);
                Player.Prepare();
                Player.SetAudioAttributes(audioAttributes, true);
            }

            return this.Binder;
        }

        [Obsolete]
        public override void OnStart(Intent? intent, int startId)
        {
            base.OnStart(intent, startId);
        }

        private void UpdateNotification()
        {
            var channel = GetNotificationChannel();
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            Notification.Style? mediaStyle = new Notification.MediaStyle().SetMediaSession(MediaSession.SessionToken);

            Notification.Builder? playerNotificationBuilder = new Notification.Builder(context, channel.Id)
             .SetChannelId(channel.Id)
             .SetSmallIcon(Resource.Drawable.heart)
             .SetContentTitle(CurrentlyPlaying.name)
             .SetContentText(CurrentlyPlaying.artistCongregate)
             .SetStyle(mediaStyle)
             .SetOngoing(true)
             .SetAllowSystemGeneratedContextualActions(true);

            //Yes intent
            Intent yesReceive = new Intent();
            yesReceive.SetAction("LIKE_ACTION");
            PendingIntent? pendingIntentLike = PendingIntent.GetBroadcast(this, 12345, yesReceive, PendingIntentFlags.Immutable);

            Intent playPauseIntent = new Intent();
            playPauseIntent.SetAction("PlayPause");
            PendingIntent? pendingIntentPlayPause = PendingIntent.GetBroadcast(this, 12346, playPauseIntent, PendingIntentFlags.Immutable);

            int previousIconResId = _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.skip_previous;
            int playPauseIconResId = _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.play;
            int nextIconResId = _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.skip_next;
            int heartIconResId = _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.heart;

            if (pendingIntentLike != null)
            {
                playerNotificationBuilder.AddAction(playPauseIconResId, "Play/Pause", pendingIntentPlayPause);
                playerNotificationBuilder.AddAction(previousIconResId, "Previous", pendingIntentLike);
                playerNotificationBuilder.AddAction(nextIconResId, "Next", pendingIntentLike);
                playerNotificationBuilder.AddAction(heartIconResId, "Favourite", pendingIntentLike);
            }
            UpdateMetadata();
            playerNotification = playerNotificationBuilder.Build();
            // StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, playerNotification);
        }

        private void UpdateMetadata()
        {
            if (Player == null) return;
            if (MediaSession == null) return;

            if (Player.CurrentPosition > 0)
            {
                currentDuration = Player.CurrentPosition;
                fullDuration = Player.Duration;
            }

            System.Diagnostics.Trace.WriteLine("Updating metadata for " + CurrentlyPlaying.name);

            MediaMetadata.Builder metadataBuilder = new MediaMetadata.Builder(); // mannnn shut up
            metadataBuilder.PutString(MediaMetadata.MetadataKeyTitle, CurrentlyPlaying.name);
            metadataBuilder.PutString(MediaMetadata.MetadataKeyArtist, CurrentlyPlaying.artistCongregate);
            metadataBuilder.PutString(MediaMetadata.MetadataKeyAlbumArtUri, CurrentlyPlaying.image.source);
            metadataBuilder.PutString(MediaMetadata.MetadataKeyAlbum, CurrentlyPlaying.album.name);
            metadataBuilder.PutString(MediaMetadata.MetadataKeyAlbumArtUri, CurrentlyPlaying.album.image.sourceAtResolution);
            metadataBuilder.PutLong(MediaMetadata.MetadataKeyDuration, fullDuration);
            MediaSession.SetMetadata(metadataBuilder.Build());
        }

        // https://developer.android.com/training/tv/playback/media-session 
        // UGGHHHHHHHH`
        private void UpdatePlaybackState()
        {
            if (MediaSession == null) return;
            if (Player == null) return;

            // Like action
            PlaybackState.CustomAction.Builder psActionBuilder = new PlaybackState.CustomAction.Builder("CUSTOM_ACTION_FAVOURITE_ADD", "CUSTOM_ACTION_FAVOURITE_ADD", Resource.Drawable.heart);
            if (CurrentlyPlaying.isFavourite)
            {
                psActionBuilder = new PlaybackState.CustomAction.Builder("CUSTOM_ACTION_FAVOURITE_REMOVE", "CUSTOM_ACTION_FAVOURITE_REMOVE", Resource.Drawable.heart_full);
            }

            PlaybackStateCode playstateCode = PlaybackStateCode.Playing;
            if (!Player.IsPlaying)
            {
                playstateCode = PlaybackStateCode.Paused;
            }

            PlaybackState.Builder? psBuilder = new PlaybackState.Builder();
            if (psBuilder != null)
            {
                psBuilder.SetState(playstateCode, currentDuration, 1f);
                psBuilder.SetActions(PlaybackState.ActionSeekTo |
                            PlaybackState.ActionPause |
                            PlaybackState.ActionSkipToNext |
                            PlaybackState.ActionSkipToPrevious |
                            PlaybackState.ActionPlayPause);
                psBuilder.AddCustomAction(psActionBuilder.Build());

                MediaSession.SetPlaybackState(psBuilder.Build());
            }
        }

        //private long getAvailableActions()
        //{
        //    long actions = PlaybackState.ActionSeekTo |
        //                    PlaybackState.ActionPause |
        //                    PlaybackState.ActionSkipToNext |
        //                    PlaybackState.ActionSkipToPrevious |
        //                    PlaybackState.ActionPlayPause;
        //    if (playingQueue == null || playingQueue.isEmpty())
        //    {
        //        return actions;
        //    }
        //    if (mState == PlaybackState.STATE_PLAYING)
        //    {
        //        actions |= PlaybackState.ACTION_PAUSE;
        //    }
        //    else
        //    {
        //        actions |= PlaybackState.ACTION_PLAY;
        //    }
        //    if (currentIndexOnQueue > 0)
        //    {
        //        actions |= PlaybackState.ACTION_SKIP_TO_PREVIOUS;
        //    }
        //    if (currentIndexOnQueue < playingQueue.size() - 1)
        //    {
        //        actions |= PlaybackState.ACTION_SKIP_TO_NEXT;
        //    }
        //    return actions;
        //}

        public NotificationChannel GetNotificationChannel()
        {
            var channelName = "PortaJel";
            var channelDescription = "Notification Channel for the PortaJel Music Streaming App for Jellyfin";
            return new NotificationChannel(channedId, channelName, NotificationImportance.Max)
            {
                Description = channelDescription,
            };
        }

        public void Initalize()
        {

        }

        public bool Play()
        {
            if (Player != null && MediaSession != null)
            {
                Player.Play();
                if (!MediaSession.Active)
                {
                    MediaSession.Active = true;
                }
                UpdateMetadata();
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if (Player != null)
            {
                Player.Pause();
                UpdateMetadata();
                return true;
            }
            return false;
        }

        public bool? TogglePlay()
        {
            if (Player != null)
            {
                if (Player.IsPlaying)
                {
                    Pause();
                    return false;
                }
                else
                {
                    Play();
                    return true;
                }

            }
            return null;
        }

        public bool SeekToPosition(long position)
        {
            if (Player != null)
            {
                Player.PlayWhenReady = false;
                try
                {
                    long TIME_UNSET = Long.MinValue + 1;
                    long seekPosition = Player.Duration == TIME_UNSET ? 0 : System.Math.Min(System.Math.Max(0, position), Player.Duration);
                    Player.SeekTo(seekPosition);
                    UpdateMetadata();
                }
                catch
                {
                    return false;
                }
                // :3 Player.PlaybackParameters.Pitch = 0.8f;
                Player.PlayWhenReady = true;
                return true;
            }
            return false;
        }

        public bool SeekToIndex(int index)
        {
            PlayingIndex = index;
            return false;
        }

        public bool SetRepeat(MediaServiceRepeatMode setRepeatMode)
        {
            if (Player != null)
            {
                repeatMode = (int)setRepeatMode;
                Player.RepeatMode = repeatMode;
                return true;
            }
            return false;
        }

        public bool ToggleRepeat()
        {
            if (Player != null)
            {
                switch (Player.RepeatMode)
                {
                    case IPlayer.RepeatModeOff: // 0
                        repeatMode = 1;
                        break;
                    case IPlayer.RepeatModeOne: // 1
                        repeatMode = 2;
                        break;
                    case IPlayer.RepeatModeAll: // 2
                        repeatMode = 0;
                        break;
                }
                Player.RepeatMode = repeatMode;
                return true;
            }
            return false;
        }

        public bool SetShuffle(bool isShullfing)
        {
            if (Player != null)
            {
                Player.ShuffleModeEnabled = isShullfing;
                return true;
            }
            return false;
        }

        public bool GetShuffle()
        {
            if (Player != null)
            {
                return Player.ShuffleModeEnabled;
            }
            return false;
        }

        public bool ToggleShuffle()
        {
            if (Player != null)
            {
                Player.ShuffleModeEnabled = !Player.ShuffleModeEnabled;
                return Player.ShuffleModeEnabled;
            }
            return false;
        }

        public bool Next()
        {
            if (Player != null)
            {
                if (Player.HasNext)
                {
                    Player.Next();
                    currentDuration = 0;
                    PlayingIndex = Player.CurrentMediaItemIndex;
                    UpdatePlaybackState();
                }
                return true;
            }
            return false;
        }

        public bool Previous()
        {
            if (Player != null)
            {
                Player.Previous();
                currentDuration = 0;
                PlayingIndex = Player.CurrentMediaItemIndex;
                UpdatePlaybackState();
                return true;
            }
            return false;
        }

        public bool SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            // Create the list of songs from the baseMusicItem
            List<Song> toAdd = new();
            if (baseMusicItem is Album)
            {
                Album album = (Album)baseMusicItem;
                toAdd.AddRange(album.songs);
            }
            if (baseMusicItem is Playlist)
            {
                Playlist playlist = (Playlist)baseMusicItem;
                toAdd.AddRange(playlist.songs);
            }

            if (Player != null)
            {
                playingFrom = baseMusicItem;

                Player.ClearMediaItems();
                if (MainQueue.Count > 0)
                {
                    QueueStartIndex = fromIndex + 1;
                    QueueRepresentation.InsertRange(QueueStartIndex, MainQueue);
                    toAdd.InsertRange(QueueStartIndex, MainQueue);
                }
                QueueRepresentation = toAdd;
                PlayingIndex = fromIndex;

                foreach (Song song in toAdd)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.streamUrl);
                    if (mediaItem != null)
                    {
                        // TODO: Add checks for if the file is a stream or local file
                        DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();
                        ProgressiveMediaSource.Factory mediaFactory = new ProgressiveMediaSource.Factory(dataSourceFactory);
                        IMediaSource? media = mediaFactory.CreateMediaSource(mediaItem);
                        if (media != null)
                        {
                            mediaItem.MediaId = song.playlistId; 
                            Player.AddMediaItem(mediaItem);
                        }
                    }
                }
                CurrentlyPlaying = toAdd[fromIndex];
                Player.SeekToDefaultPosition();
                PlayingIndex = fromIndex;
                Player.SeekTo(PlayingIndex, 0);
                UpdateMetadata();
                return true;
            }

            playingFrom = baseMusicItem;

            List<Song> songList = new();
            if (playingFrom is Album)
            {
                Album tempAlbum = (Album)playingFrom;
                songList.AddRange(tempAlbum.songs);
            }
            if (playingFrom is Playlist)
            {
                Playlist tempPlaylist = (Playlist)playingFrom;
                songList.AddRange(tempPlaylist.songs);
            }

            CurrentlyPlaying = songList[fromIndex];

            foreach (Song song in songList)
            {
                song.playlistId = Guid.NewGuid().ToString();
                MainQueue.Add(song);
            }
            return true;
        }

        public bool AddSong(Song song)
        {
            if (Player != null)
            {
                if (MainQueue.Count() == 0)
                {
                    CurrentlyPlaying = song;
                    QueueStartIndex = PlayingIndex;
                    UpdatePlaybackState();
                }
                else if(QueueStartIndex + MainQueue.Count() - 1 < PlayingIndex)
                {
                    MainQueue.Clear();
                    QueueStartIndex = PlayingIndex + 1;
                }

                song.playlistId = Guid.NewGuid().ToString();
                MainQueue.Add(song);
                int insertIndex = QueueStartIndex + MainQueue.Count();

                if (song.streamUrl != null)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.streamUrl);
                    if (mediaItem != null)
                    {
                        // TODO: Add checks for if the file is a stream or local file
                        DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();
                        ProgressiveMediaSource.Factory mediaFactory = new ProgressiveMediaSource.Factory(dataSourceFactory);
                        IMediaSource? media = mediaFactory.CreateMediaSource(mediaItem);

                        if (media != null)
                        {
                            // MediaID is only added to songs that are in the queue
                            mediaItem.MediaId = song.playlistId;
                            QueueRepresentation.Insert(insertIndex, song);
                            Player.AddMediaItem(insertIndex, mediaItem);
                        }
                    }
                }
                UpdateMetadata();
                return true;
            }

            CurrentlyPlaying = song;
            MainQueue.Add(song);
            return true;
        }

        public bool AddSongs(Song[] songs)
        {
            if (Player != null)
            {
                foreach (Song song in songs)
                {
                    AddSong(song);
                }
                UpdateMetadata();
                return true;
            }
            return false;
        }

        public bool RemoveSong(int index)
        {
            if (Player != null)
            {
                MainQueue.RemoveAt(index);
                Player.RemoveMediaItem(index);
                UpdateMetadata();
                return true;
            }
            return true;
        }

        /// <summary>
        /// This should be the most reliable fukcing method ever. Bruh. TRUST THE METHOD
        /// </summary>
        /// <returns></returns>
        public SongGroupCollection GetQueue()
        {
            SongGroupCollection songGroupCollection = new();
            songGroupCollection.QueueStartIndex = 0;

            SongGroup previous = new("Previous");
            SongGroup current = new("Currently Playing");
            SongGroup queue = new("Queue");

            if (Player == null)
            {
                songGroupCollection.Add(previous);
                songGroupCollection.Add(current);
                songGroupCollection.Add(queue);
                return songGroupCollection;
            }

            List<Song> totalList = new();

            if(QueueStartIndex == -1)
            {
                QueueStartIndex = PlayingIndex + 1;
            }

            // Create a list to query songs from
            List<Song> querySongs = new();
            if (playingFrom is Album)
            {
                Album album = (Album)playingFrom;
                querySongs.AddRange(album.songs);
            }
            querySongs.AddRange(MainQueue);
            songGroupCollection.QueueCount = MainQueue.Count();
            songGroupCollection.QueueStartIndex = QueueStartIndex;

            // construct queue from player
            for (int i = 0; i < Player.MediaItemCount; i++)
            {
                Song? song = QueueRepresentation[i];
                if (song == null) break;

                if (i < PlayingIndex) previous.Add(song);
                if (i == PlayingIndex) current.Add(song);
                if (i > PlayingIndex) queue.Add(song);
            }

            songGroupCollection.Add(previous);
            songGroupCollection.Add(current);
            songGroupCollection.Add(queue);
            return songGroupCollection;
        }

        public int GetQueueIndex()
        {
            return PlayingIndex;
        }

        public bool GetIsPlaying()
        {
            if (Player != null)
            {
                return Player.IsPlaying;
            }
            return false;
        }

        public bool GetIsShuffling()
        {
            if (Player != null)
            {
                return Player.ShuffleModeEnabled;
            }
            return false;
        }

        public int GetRepeatMode()
        {
            if (Player != null)
            {
                return Player.RepeatMode;
            }
            return 0;
        }

        public Song GetCurrentlyPlaying()
        {
            if (Player != null)
            {
                if (GetQueue().AllSongs.Count() > PlayingIndex)
                {
                    return GetQueue().AllSongs[PlayingIndex];
                }
            }
            return Song.Empty;
        }

        public BaseMusicItem? GetPlayingSource()
        {
            if (Player != null)
            {
                return GetQueue().AllSongs[PlayingIndex];
            }
            return null;
        }

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            if (Player != null)
            {
                PlaybackInfo? newTime = null;
                PlayingIndex = Player.CurrentMediaItemIndex;

                Song currentSong = GetCurrentlyPlaying();
                if(Player.CurrentPosition > 0)
                {
                    currentDuration = Player.CurrentPosition;
                    fullDuration = Player.Duration;
                }
                CurrentlyPlaying = currentSong;

                if (Player.PlaybackState == IPlayer.StateReady)
                {
                    string PlaybackTimeValue = "00:00";
                    string PlaybackMaximumTimeValue = "00:00";

                    TimeSpan fullDuratioinTimeSpan = TimeSpan.FromMilliseconds(currentSong.duration);

                    PlaybackTimeValue = "00:00";
                    PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", fullDuratioinTimeSpan.Minutes, fullDuratioinTimeSpan.Seconds);

                    if (Player.Duration >= 0)
                    {
                        TimeSpan playbackTimeSpan = TimeSpan.FromMilliseconds(Player.CurrentPosition);
                        fullDuratioinTimeSpan = TimeSpan.FromMilliseconds(Player.Duration);

                        currentSong.duration = Player.Duration;

                        PlaybackTimeValue = string.Format("{0:D2}:{1:D2}", playbackTimeSpan.Minutes, playbackTimeSpan.Seconds);
                        PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", fullDuratioinTimeSpan.Minutes, fullDuratioinTimeSpan.Seconds);
                    }

                    newTime = new(
                        currentDuration,
                        currentSong,
                        PlayingIndex,
                        Player.IsPlaying,
                        PlaybackTimeValue,
                        PlaybackMaximumTimeValue
                    );
                }
                return newTime;
            }
            return null;
        }
    }
}
#pragma warning restore CS0618, CA1422 // Type or member is obsolete