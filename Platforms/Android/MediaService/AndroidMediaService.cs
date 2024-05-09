using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.InputMethodServices;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Extractor.Mp3;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using Java.Lang;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using PortaJel_Blazor.Data;
using MediaMetadata = Android.Media.MediaMetadata;

#pragma warning disable CS0618, CS0612, CA1422 // Type or member is obsolete
// Referce
// https://github.com/xamarin/monodroid-samples/blob/archived-xamarin/android5.0/MediaBrowserService/MediaBrowserService/MusicService.cs
// See lines 169 through 360 for Kotlin implementation of Exoplayer + notification service 
// https://github.com/AkaneTan/Gramophone/blob/30773abb7df9317d50d159df7f40b8d9c9418520/app/src/main/kotlin/org/akanework/gramophone/logic/GramophonePlaybackService.kt#L577
//
// Looks like an easy enough Kotlin tutorial for the MediaStyle notificaiton mrow :3 
// https://medium.com/@anafthdev_/create-mediastyle-notification-in-android-anafthdev-70fe7df3397e
//
// This is for the MusicBrainz lookup I want to add, for comparing strings. Not relevent to the Android Media Service here but it will need to be implemented and I have this file open already
// https://medium.com/@tarakshah/this-article-explains-how-to-check-the-similarity-between-two-string-in-percentage-or-score-from-0-83e206bf6bf5
namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    [Service(Name = "PortaJel.MediaService", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }

        public IExoPlayer? Player = null;
        public MediaPlayer MediaPlayer { get; private set; }
        PlayerEventListener PlayerEventListener = new();
        private long currentDuration = -1;
        private long fullDuration = -1;
        private int repeatMode = 0;

        MediaSession? mediaSession = null;
        MediaMetadata? mediaSessionMetadata = null;
        PlaybackState? mediaSessionState = null;
        MediaSessionCallback? mediaSessionCallback = null;
        // MediaMetadata? mediaMetadata = null;

        Notification? playerNotification = null;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public int playingIndex { get; private set; } = 0;
        public BaseMusicItem? playingFrom { get; private set; } = null;
        public List<Song> songQueue { get; private set; } = new();

        private string channedId = AppInfo.PackageName;

        public AndroidMediaService() 
        {
            
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var channelName = "PortaJel";
            var channelDescription = "Notification Channel for the PortaJel Music Streaming App for Jellyfin";
            var channel = new NotificationChannel(channedId, channelName, NotificationImportance.Max)
            {
                Description = channelDescription,
            };

            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;

            mediaSession = new MediaSession(context, channedId);
            mediaSession.SetMetadata(mediaSessionMetadata);
            mediaSessionCallback = new MediaSessionCallback();
            // Define callback functions here
            mediaSessionCallback.OnPlayImpl = () => {
                Play();
            };
            mediaSessionCallback.OnPauseImpl = () => {
                Pause();
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
                            if (eventVal.KeyCode == KeyEvent.KeyCodeFromString("KEYCODE_MEDIA_PLAY"))
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

            mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
            mediaSession.SetCallback(mediaSessionCallback);

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

            //Yes intent
            Intent yesReceive = new Intent();
            yesReceive.SetAction("LIKE_ACTION");
            PendingIntent? pendingIntentLike = PendingIntent.GetBroadcast(this, 12345, yesReceive, PendingIntentFlags.Immutable);

            Intent playPauseIntent = new Intent();
            playPauseIntent.SetAction("PlayPause");
            PendingIntent? pendingIntentPlayPause = PendingIntent.GetBroadcast(this, 12346, playPauseIntent, PendingIntentFlags.Immutable);

            Notification.Builder? playerNotificationBuilder = new Notification.Builder(context, channel.Id)
                 .SetChannelId(channel.Id)
                 .SetSmallIcon(Resource.Drawable.heart)
                 .SetContentTitle("Track title")
                 .SetContentText("Artist - Album")
                 .SetStyle(new Notification.MediaStyle().SetMediaSession(mediaSession.SessionToken))
                 .SetAllowSystemGeneratedContextualActions(true);
            if (pendingIntentLike != null) 
            {
                playerNotificationBuilder.AddAction(Resource.Drawable.heart, "Yes", pendingIntentLike);
                playerNotificationBuilder.AddAction(Resource.Drawable.heart, "PlayPause", pendingIntentPlayPause);
            }
            playerNotification = playerNotificationBuilder.Build();

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

                SimpleExoPlayer.Builder? newBuilder = new SimpleExoPlayer.Builder(this)
                     .SetMediaSourceFactory(new DefaultMediaSourceFactory(context, extractorsFactory));
                if (newBuilder != null)
                {
                    Player = newBuilder.Build();
                }
                if (Player != null)
                {
                    Player.RepeatMode = IPlayer.RepeatModeOff;
                }
                string deviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            }

            // Add event for when the Player changes track
            PlayerEventListener.OnMediaItemTransitionImpl = (MediaItem? song, int index) =>
            {
                if (Player != null)
                {
                    playingIndex = Player.CurrentMediaItemIndex;

                    MediaMetadata newData = new MediaMetadata.Builder()
                    .Build();
                    mediaSession.SetMetadata(newData);
                }
            };
            PlayerEventListener.OnPlayerStateChangedImpl = (bool playWhenReady, int playbackState) =>
            {
                if (playbackState == IPlayer.StateReady && Player != null)
                {
                    fullDuration = Player.Duration;

                    MediaMetadata newData = new MediaMetadata.Builder()
                    .Build();
                    mediaSession.SetMetadata(newData);
                }
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
                Player.PlayWhenReady = true;
            }

            return this.Binder;
        }

        [Obsolete]
        public override void OnStart(Intent? intent, int startId)
        {
            base.OnStart(intent, startId);
        }

        public bool Play()
        {
            if (Player != null)
            {
                Player.Play();
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if (Player != null)
            {
                Player.Pause();
                return true;
            }
            return false;
        }

        public bool TogglePlay()
        {
            if (Player != null)
            {
                if (Player.IsPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
                return true;
            }
            return false;
        }

        public bool SeekToPosition(long position)
        {
            if (Player != null && position < Player.Duration && position > 0)
            {
                Player.PlayWhenReady = false;
                try
                {
                    long TIME_UNSET = Long.MinValue + 1;
                    long seekPosition = Player.Duration == TIME_UNSET ? 0 : System.Math.Min(System.Math.Max(0, position), Player.Duration);
                    Player.SeekTo(seekPosition);
                }
                catch (System.Exception ex)
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
            playingIndex = index;
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
                    playingIndex = Player.CurrentMediaItemIndex;
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
                playingIndex = Player.CurrentMediaItemIndex;
                return true;
            }
            return false;
        }

        public bool SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            if (Player != null)
            {
                playingFrom = baseMusicItem;

                Player.ClearMediaItems();

                foreach (Song song in GetQueue().AllSongs)
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
                            mediaItem.MediaId = song.id.ToString();
                            Player.AddMediaSource(media);
                        }

                    }
                }

                Player.SeekToDefaultPosition();
                // Seek to position of that last song
                playingIndex = fromIndex;
                Player.SeekTo(playingIndex, Player.Duration);
                return true;
            }
            return false;
        }

        public bool AddSong(Song song)
        {
            if (Player != null)
            {
                songQueue.Add(song);

                int index = (playingIndex + 1) + songQueue.Count();

                if (song.streamUrl != null)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.streamUrl);
                    if(mediaItem != null)
                    {
                        // TODO: Add checks for if the file is a stream or local file
                        DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();
                        ProgressiveMediaSource.Factory mediaFactory = new ProgressiveMediaSource.Factory(dataSourceFactory);
                        IMediaSource? media = mediaFactory.CreateMediaSource(mediaItem);

                        if (media != null)
                        {
                            mediaItem.MediaId = song.id.ToString();
                            Player.AddMediaSource(media);
                        }
                    }
                }

                return true;
            }
            return false;
        }

        public bool AddSongs(Song[] songs)
        {
            if (Player != null)
            {
                foreach (Song song in songs)
                {
                    AddSong(song);
                }

                return true;
            }
            return false;
        }

        public bool RemoveSong(int index)
        {
            if (Player != null)
            {
                songQueue.RemoveAt(index);
                Player.RemoveMediaItem(index);

                return true;
            }
            return true;
        }

        public SongGroupCollection GetQueue()
        {
            if (playingFrom == null)
            {
                return new SongGroupCollection();
            }

            SongGroupCollection songGroupCollection = new();
            SongGroup queue = new("Queue");
            SongGroup songList = new("Next Up");

            // Add queue after selected song
            queue.AddRange(songQueue);
            if (playingFrom is Album)
            {
                Album album = (Album)playingFrom;
                songList.AddRange(album.songs);
            }
            else if (playingFrom is Playlist)
            {
                Playlist playlist = (Playlist)playingFrom;
                songList.AddRange(playlist.songs);
            }

            songGroupCollection.Add(queue);
            songGroupCollection.Add(songList);

            return songGroupCollection;
        }

        public int GetQueueIndex()
        {
            return playingIndex;
        }

        public bool GetIsPlaying()
        {
            if (Player != null)
            {
                return Player.PlayWhenReady;
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
                if (GetQueue().AllSongs.Count() > playingIndex)
                {
                    return GetQueue().AllSongs[playingIndex];
                }
            }
            return Song.Empty;
        }

        public BaseMusicItem? GetPlayingSource()
        {
            if (Player != null)
            {
                return GetQueue().AllSongs[playingIndex];
            }
            return null;
        }

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            if (Player != null)
            {
                PlaybackInfo? newTime = null;
                playingIndex = Player.CurrentMediaItemIndex;

                Song currentSong = GetCurrentlyPlaying();

                if (Player.PlaybackState == IPlayer.StateReady)
                {
                    if(currentSong.duration <= 0)
                    {
                        TimeSpan passedTime = TimeSpan.FromMilliseconds(Player.Duration);
                        currentSong.duration = passedTime.Ticks;
                    }
                    newTime = new(
                        Player.CurrentPosition,
                        currentSong,
                        playingIndex,
                        Player.IsPlaying
                    );
                }
                return newTime;
            }
            return null;
        }
    }
}
#pragma warning restore CS0618, CA1422 // Type or member is obsolete