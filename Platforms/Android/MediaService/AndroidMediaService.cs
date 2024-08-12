using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Media.Session;
using Blurhash;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Extractor.Mp3;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using Java.Lang;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using PortaJel_Blazor.Data;
using Bitmap = Android.Graphics.Bitmap;
using Color = Android.Graphics.Color;
using Math = System.Math;
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
//
// TODO: 
// All of this shit but again, like, all over again. It's been like, a month and a half and I've decided this whole class is an atrocity and needs to fucking go.
// Like, it works, but fuck nobody else in the world would look at this and say 'yeah yeah we can keep that for the final draft'. Noooo fucking way

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    [Service(Name = "PortaJel.MediaService", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }

        public IExoPlayer? Player = null;
        PlayerEventListener PlayerEventListener = new();
        private bool CanGetDuration = false;
        private long CurrentDuration = -1;
        private long FullDuration = -1;
        private int repeatMode = 0;

        MediaSession? MediaSession = null;
        MediaMetadata? mediaSessionMetadata = null;
        MediaSessionCallback? mediaSessionCallback = null;

        Notification? playerNotification = null;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public int PlayingIndex { get; private set; } = 0;
        public BaseMusicItem? playingFrom { get; private set; } = null;
        public Song CurrentlyPlaying { get; private set; } = Song.Empty;
        public List<Song> QueueRepresentation { get; private set; } = new();
        public List<Song> MainQueue { get; private set; } = new();
        private int QueueStartIndex { get; set; } = -1;
        private string channedId = AppInfo.PackageName;

        private DataConnector ServerApi = new();

        public AndroidMediaService()
        {

        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var channel = GetNotificationChannel();
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;
 
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
            UpdatePlaybackState();
            playerNotification = GetNotification();
            
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
                        UpdatePlaybackState();
                        // Check if the song we've just played was a part of the queue, if so remove it from the queue list \
                        if (song == null) return;
                        if (string.IsNullOrWhiteSpace(song.MediaId)) return; // Only Queued Songs have a MediaID 
                        QueueStartIndex = index;
                        int queueIndex = MainQueue.FindIndex(queueSong => queueSong.PlaylistId == song.MediaId);
                        if (queueIndex >= 0)
                        {
                            MainQueue.RemoveAt(queueIndex);
                        }
                    }
                }
            };
            PlayerEventListener.OnPlayerStateChangedImpl = (bool playWhenReady, int playbackState) =>
            {
                if (Player == null) return;
                if (playbackState == IPlayer.StateReady)
                {
                    FullDuration = Player.Duration;
                    CurrentlyPlaying = GetQueue().AllSongs[PlayingIndex];
                    UpdatePlaybackState();
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

        /// <summary>
        ///  You should create and initialize a media session in the onCreate() method of the activity or service that owns the session.
        /// </summary>
        public override void OnCreate()
        {
            this.Binder = new MediaServiceBinder(this);
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
                MediaSession.SetCallback(mediaSessionCallback);
                MediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
            }
        }

        public override void OnDestroy()
        {
            Destroy();
        }
        public void Destroy()
        {
            this.StopForeground(true);
            if (Binder!=null)
            {
                Binder.UnregisterFromRuntime();
                Binder.Dispose();
            }
            if(MediaSession!=null)
            {
                MediaSession.Dispose();
                MediaSession.Release();
            }
            if (Player != null) Player.Dispose();
            if (playerNotification!=null) playerNotification.Dispose();
            this.StopSelf();
            this.Dispose();
        }

        private Notification GetNotification()
        {
            if(MediaSession == null)
            {
                throw new System.Exception("Media Session is null!");
            }

            var channel = GetNotificationChannel();

            NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);

            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            Notification.Style? mediaStyle = new Notification.MediaStyle().SetMediaSession(MediaSession.SessionToken);
            
            MediaController controller = MediaSession.Controller;
            MediaMetadata? mediaMetadata = controller.Metadata;
            MediaDescription? description = mediaMetadata.Description; // System.NullReferenceException: 'Object reference not set to an instance of an object.'

            Pixel[,] pixels = new Pixel[2, 2];
            if (!string.IsNullOrWhiteSpace(CurrentlyPlaying.ImgBlurhash))
            {
                Core.Decode(CurrentlyPlaying.ImgBlurhash, pixels);
            }
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = pixels[x, y];
                    int scaledRed = (int)(pixel.Red * 255);
                    int scaledGreen = (int)(pixel.Green * 255);
                    int scaledBlue = (int)(pixel.Blue * 255);

                    // Increase brightness by multiplying with a factor (e.g., 1.2 for 20% increase)
                    float brightnessFactor = 2f * 1; // Additional brightness
                    scaledRed = (int)(scaledRed * brightnessFactor);
                    scaledGreen = (int)(scaledGreen * brightnessFactor);
                    scaledBlue = (int)(scaledBlue * brightnessFactor);

                    // Clamp values to ensure they stay within the valid range (0-255)
                    scaledRed = Math.Clamp(scaledRed, 0, 255);
                    scaledGreen = Math.Clamp(scaledGreen, 0, 255);
                    scaledBlue = Math.Clamp(scaledBlue, 0, 255);

                    // Convert int to byte
                    byte red = (byte)scaledRed;
                    byte green = (byte)scaledGreen;
                    byte blue = (byte)scaledBlue;

                    Color color = Color.Argb(255, red, green, blue);
                    bitmap.SetPixel(x, y, color);
                }
            }

            Notification.Builder? playerNotificationBuilder = new Notification.Builder(context, channel.Id)
                // Add the metadata for the currently playing track
                .SetContentTitle(description.Title)
                .SetContentText(description.Subtitle)
                .SetSubText(description.Description)
                .SetLargeIcon(bitmap)
                // Enable launching the player by clicking the notification
                .SetContentIntent(controller.SessionActivity)
                // Stop the service when the notification is swiped away
                .SetDeleteIntent(MediaButtonReceiver.BuildMediaButtonPendingIntent(context,
                   PlaybackStateCompat.ActionStop))
                // Make the transport controls visible on the lockscreen
                .SetVisibility(NotificationVisibility.Public)
                // Add an app icon and set its accent color
                // Be careful about the color
                .SetSmallIcon(Resource.Drawable.exo_notification_play)
                .SetColor(ContextCompat.GetColor(context, Resource.Color.primary_dark_material_dark))
                // Add a pause button
                //.AddAction(new NotificationCompat.Action(
                //    R.drawable.pause, getString(R.string.pause),
                //    MediaButtonReceiver.buildMediaButtonPendingIntent(context,
                //        PlaybackStateCompat.ACTION_PLAY_PAUSE)))
                // Take advantage of MediaStyle features
                .SetStyle(mediaStyle);
            
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
            return playerNotificationBuilder.Build();
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, playerNotification);
        }

        // https://developer.android.com/training/tv/playback/media-session 
        // UGGHHHHHHHH`
        private void UpdatePlaybackState()
        {
            if (MediaSession == null) throw new System.Exception("Media Session is null!");

            // Like action
            PlaybackState.CustomAction.Builder psActionBuilder = new PlaybackState.CustomAction.Builder("CUSTOM_ACTION_FAVOURITE_ADD", "CUSTOM_ACTION_FAVOURITE_ADD", Resource.Drawable.heart);
            if (CurrentlyPlaying.IsFavourite)
            {
                psActionBuilder = new PlaybackState.CustomAction.Builder("CUSTOM_ACTION_FAVOURITE_REMOVE", "CUSTOM_ACTION_FAVOURITE_REMOVE", Resource.Drawable.heart_full);
            }
            PlaybackState.Builder? psBuilder = new PlaybackState.Builder();
            PlaybackStateCode playstateCode = PlaybackStateCode.Playing;
            if (Player != null)
            {
                if (!Player.IsPlaying)
                {
                    playstateCode = PlaybackStateCode.Paused;
                }
                psBuilder.SetState(playstateCode, Player.CurrentPosition, 1f);
            }
            if (!MediaSession.Active)
            {
                MediaSession.Active = true;
            }

            if (psBuilder != null)
            {
                psBuilder.SetActions(PlaybackState.ActionSeekTo |
                            PlaybackState.ActionPause |
                            PlaybackState.ActionSkipToNext |
                            PlaybackState.ActionSkipToPrevious |
                            PlaybackState.ActionPlayPause);
                psBuilder.AddCustomAction(psActionBuilder.Build());

                Pixel[,] pixels = new Pixel[2, 2];
                if (!string.IsNullOrWhiteSpace(CurrentlyPlaying.ImgBlurhash))
                {
                    Core.Decode(CurrentlyPlaying.ImgBlurhash, pixels);
                }
                int width = pixels.GetLength(0);
                int height = pixels.GetLength(1);

                Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var pixel = pixels[x, y];
                        int scaledRed = (int)(pixel.Red * 255);
                        int scaledGreen = (int)(pixel.Green * 255);
                        int scaledBlue = (int)(pixel.Blue * 255);

                        // Increase brightness by multiplying with a factor (e.g., 1.2 for 20% increase)
                        float brightnessFactor = 2f * 1; // Additional brightness
                        scaledRed = (int)(scaledRed * brightnessFactor);
                        scaledGreen = (int)(scaledGreen * brightnessFactor);
                        scaledBlue = (int)(scaledBlue * brightnessFactor);

                        // Clamp values to ensure they stay within the valid range (0-255)
                        scaledRed = Math.Clamp(scaledRed, 0, 255);
                        scaledGreen = Math.Clamp(scaledGreen, 0, 255);
                        scaledBlue = Math.Clamp(scaledBlue, 0, 255);

                        // Convert int to byte
                        byte red = (byte)scaledRed;
                        byte green = (byte)scaledGreen;
                        byte blue = (byte)scaledBlue;

                        Color color = Color.Argb(255, red, green, blue);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                MediaMetadata.Builder metadataBuilder = new MediaMetadata.Builder(); // mannnn shut up
                metadataBuilder.PutString(MediaMetadata.MetadataKeyTitle, CurrentlyPlaying.Name);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyArtist, CurrentlyPlaying.ArtistNames);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyAlbumArtUri, CurrentlyPlaying.ImgSource);
                metadataBuilder.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, bitmap);
                metadataBuilder.PutBitmap(MediaMetadata.MetadataKeyDisplayIcon, bitmap);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyDisplayIconUri, CurrentlyPlaying.ImgSource);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyDisplayDescription, CurrentlyPlaying.Name);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyDisplaySubtitle, CurrentlyPlaying.Name);
                metadataBuilder.PutString(MediaMetadata.MetadataKeyDisplayTitle, CurrentlyPlaying.Name);
                //metadataBuilder.PutString(MediaMetadata.MetadataKeyAlbum, CurrentlyPlaying.Album.Name);
                //metadataBuilder.PutLong(MediaMetadata.MetadataKeyDuration, FullDuration);
                MediaSession.SetPlaybackState(psBuilder.Build());
                MediaSession.SetMetadata(metadataBuilder.Build());
                //MediaSession.Notify();
                playerNotification = GetNotification();
                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, playerNotification);
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
            NotificationChannel channel = new NotificationChannel(channedId, channelName, NotificationImportance.Low)
            {
                Description = channelDescription,
            };
            channel.SetVibrationPattern(new long[] { 0 });
            channel.EnableVibration(true);
            return channel; 
        }

        public void Initalize()
        {

        }

        public bool Play()
        {
            if (Player != null && MediaSession != null)
            {
                Player.Play();
                UpdatePlaybackState();
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if (Player != null)
            {
                Player.Pause();
                UpdatePlaybackState();
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
                    UpdatePlaybackState();
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
                    CurrentDuration = 0;
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
                CurrentDuration = 0;
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
                if(album.Songs != null)
                {
                    toAdd.AddRange(album.Songs.Select(song => new Song(song)));
                }
            }
            if (baseMusicItem is Playlist)
            {
                Playlist playlist = (Playlist)baseMusicItem;
                if (playlist.Songs != null)
                {
                    toAdd.AddRange(playlist.Songs.Select(song => new Song(song)));
                }
            }

            if (Player != null)
            {
                playingFrom = baseMusicItem;

                Player.ClearMediaItems();
                if (MainQueue.Count > 0)
                {
                    QueueStartIndex = fromIndex + 1;
                    QueueRepresentation.InsertRange(QueueStartIndex, MainQueue);
                    foreach (Song queueSong in MainQueue)
                    {
                        queueSong.SetPlaylistId(Guid.NewGuid().ToString());
                    }                    
                    toAdd.InsertRange(QueueStartIndex, MainQueue);
                }
                QueueRepresentation = toAdd;
                PlayingIndex = fromIndex;

                foreach (Song song in toAdd)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.StreamUrl);
                    if (mediaItem != null)
                    {
                        // TODO: Add checks for if the file is a stream or local file
                        DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();
                        ProgressiveMediaSource.Factory mediaFactory = new ProgressiveMediaSource.Factory(dataSourceFactory);
                        IMediaSource? media = mediaFactory.CreateMediaSource(mediaItem);
                        if (media != null)
                        {
                            // mediaItem.MediaId = song.PlaylistId; 
                            Player.AddMediaItem(mediaItem);
                        }
                    }
                }
                CurrentlyPlaying = toAdd[fromIndex];
                Player.SeekToDefaultPosition();
                PlayingIndex = fromIndex;
                Player.SeekTo(PlayingIndex, 0);
                UpdatePlaybackState();
                UpdatePlaybackState();
                return true;
            }

            playingFrom = baseMusicItem;

            List<Song> songList = new();
            if (playingFrom is Album)
            {
                Album tempAlbum = (Album)playingFrom;
                if(tempAlbum.Songs != null)
                {
                    songList.AddRange(tempAlbum.Songs.Select(song => new Song(song)));
                }
            }
            if (playingFrom is Playlist)
            {
                Playlist tempPlaylist = (Playlist)playingFrom;
                songList.AddRange(tempPlaylist.Songs.Select(song => new Song(song)));
            }

            CurrentlyPlaying = songList[fromIndex];

            foreach (Song song in songList)
            {
                song.SetPlaylistId(Guid.NewGuid().ToString());
                MainQueue.Add(song);
            }
            return true;
        }

        public BaseMusicItem? GetPlayingCollection()
        {
            return playingFrom;
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

                song.SetPlaylistId(Guid.NewGuid().ToString());
                MainQueue.Add(song);
                int insertIndex = QueueStartIndex + MainQueue.Count();

                if (song.StreamUrl != null)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.StreamUrl);
                    if (mediaItem != null)
                    {
                        // TODO: Add checks for if the file is a stream or local file
                        DefaultHttpDataSource.Factory dataSourceFactory = new DefaultHttpDataSource.Factory();
                        ProgressiveMediaSource.Factory mediaFactory = new ProgressiveMediaSource.Factory(dataSourceFactory);
                        IMediaSource? media = mediaFactory.CreateMediaSource(mediaItem);

                        if (media != null)
                        {
                            // MediaID is only added to songs that are in the queue
                            mediaItem.MediaId = song.PlaylistId;
                            QueueRepresentation.Insert(insertIndex, song);
                            Player.AddMediaItem(insertIndex, mediaItem);
                        }
                    }
                }
                UpdatePlaybackState();
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
                UpdatePlaybackState();
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
                UpdatePlaybackState();
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
            SongGroup nextUp = new("Next Up");

            if (Player == null)
            {
                songGroupCollection.Add(previous);
                songGroupCollection.Add(current);
                songGroupCollection.Add(queue);
                songGroupCollection.Add(nextUp);
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
                if(album.Songs != null)
                {
                    querySongs.AddRange(album.Songs.Select(song => new Song(song)));
                }
            }
            querySongs.AddRange(MainQueue);
            songGroupCollection.QueueCount = MainQueue.Count();
            songGroupCollection.QueueStartIndex = QueueStartIndex;

            // construct queue from player
            for (int i = 0; i < Player.MediaItemCount; i++)
            {
                Song? song = QueueRepresentation[i];
                MediaItem? mediaItem = Player.GetMediaItemAt(i);
                if (song == null) continue;
                if (mediaItem == null) continue;
                if (i < PlayingIndex) previous.Add(song);
                if (i == PlayingIndex) current.Add(song);
                if (i > PlayingIndex && !string.IsNullOrWhiteSpace(mediaItem.MediaId)) queue.Add(song);
                if (i > PlayingIndex && string.IsNullOrWhiteSpace(mediaItem.MediaId)) nextUp.Add(song);
            }

            songGroupCollection.Add(previous);
            songGroupCollection.Add(current);
            songGroupCollection.Add(queue);
            songGroupCollection.Add(nextUp);
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
                PlaybackInfo? newTime = new(
                    setDuration: FullDuration >= 0 ? TimeSpan.FromMilliseconds(FullDuration) : GetCurrentlyPlaying().Duration, 
                    setCurrentDuration: Player.CurrentPosition >= 0 ? TimeSpan.FromMilliseconds(Player.CurrentPosition): TimeSpan.FromMilliseconds(0),
                    currentSong: GetCurrentlyPlaying(), 
                    setPlayingIndex: PlayingIndex,
                    isBuffering: Player.IsLoading,
                    isPlaying: Player.IsPlaying);
                return newTime;
            }
            return null;
        }
    }
}
#pragma warning restore CS0618, CA1422 // Type or member is obsolete