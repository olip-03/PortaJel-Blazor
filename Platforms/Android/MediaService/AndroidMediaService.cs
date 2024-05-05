﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Com.Google.Android.Exoplayer2;
using Android.Util;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Classes.Services;
using Android.OS;
using PortaJel_Blazor.Classes;
using Android.Media.Session;
using Android.Graphics;
using Android.Runtime;
using Com.Google.Android.Exoplayer2.UI;

#pragma warning disable CS0618, CS0612, CA1422 // Type or member is obsolete
// Referce
// https://github.com/xamarin/monodroid-samples/blob/archived-xamarin/android5.0/MediaBrowserService/MediaBrowserService/MusicService.cs
namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    [Service(Name = "PortaJel.MediaService", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }

        public IExoPlayer? Player = null;
        PlayerEventListener PlayerEventListener = new();
        private int repeatMode = 0;

        MediaSession? mediaSession = null;
        MediaSessionCallback? mediaSessionCallback = null;
        MediaMetadata? mediaMetadata = null;

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
            mediaSessionCallback = new MediaSessionCallback();
            // Define callback functions here
            mediaSessionCallback.OnPlayImpl = () => {
                Play();
            };
            mediaSessionCallback.OnPauseImpl = () => {
                Pause();
            };

            mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
            mediaSession.SetCallback(mediaSessionCallback);

            playerNotification = new Notification.Builder(context, channel.Id)
                 .SetChannelId(channel.Id)
                 .SetSmallIcon(Resource.Drawable.ic_mtrl_checked_circle)
                 .SetContentTitle("Track title")
                 .SetContentText("Artist - Album")
                 .SetStyle(new Notification.MediaStyle().SetMediaSession(mediaSession.SessionToken))
                 .SetAllowSystemGeneratedContextualActions(true)
                 .Build();

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, playerNotification);
            return base.OnStartCommand(intent, flags, startId);
        }

        public override IBinder OnBind(Intent? intent)
        {
            this.Binder = new MediaServiceBinder(this);

            var HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
            var MainDataSource = new ProgressiveMediaSource.Factory(HttpDataSourceFactory);
            if (MainDataSource != null)
            {
                IExoPlayer.Builder? newBuilder = new IExoPlayer.Builder(this);
                newBuilder = newBuilder.SetMediaSourceFactory(MainDataSource);
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
                }
            };

            if (Player != null)
            {
                Player.AddListener(PlayerEventListener);
                Player.Prepare();
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
            if (Player != null)
            {
                Player.SeekTo(position);
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
                    if(mediaItem != null)
                    {
                        mediaItem.MediaId = song.id.ToString();
                        
                        Player.AddMediaItem(mediaItem);
                    }
                }

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
                        mediaItem.MediaId = song.id.ToString();
                        Player.AddMediaItem(mediaItem);
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
            SongGroup currentPlaying = new("Currently Playing");
            SongGroup queue = new("Queue");
            SongGroup songList = new("Next Up");

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

            // Add everything up to the selected song to queue
            for (int i = 0; i < songList.Count; i++)
            {
                if (i < playingIndex)
                {
                    queue.Add(songList[i]);
                }
                else
                {
                    break;
                }
            }

            // Add queue after selected song
            for (int i = 0; i < songQueue.Count; i++)
            {
                queue.Add(songQueue[i]);
            }

            // Add remainder of the tracks that were 'up next'
            if (playingIndex < songList.Count)
            {
                for (int i = playingIndex; i < songList.Count; i++)
                {
                    queue.Add(songList[i]);
                }
            }

            songGroupCollection.Add(currentPlaying);
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
                if (GetQueue().Count() < playingIndex)
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
                PlaybackInfo? newTime = new(
                        Player.CurrentPosition,
                        Player.Duration,
                        GetCurrentlyPlaying().id,
                        playingIndex
                    );
                return newTime;
            }
            return null;
        }
    }
}
#pragma warning restore CS0618, CA1422 // Type or member is obsolete