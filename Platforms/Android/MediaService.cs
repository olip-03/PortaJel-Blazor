using Android.App;
using Android.Content;
using Com.Google.Android.Exoplayer2;
using Android.Util;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using PortaJel_Blazor.Data;
using System.Timers;
using Android.OS;

#pragma warning disable CS0612, CS0618 // Type or member is obsolete

// Android Implementation
// https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui.MediaElement/Views/MediaManager.android.cs
// https://putridparrot.com/blog/android-foreground-service-using-maui/
// Might be able to dig through her to get some more info on how this shit's all supposed to work
// https://github.com/Baseflow/XamarinMediaManager
// Info on background services
// https://fabcirablog.weebly.com/blog/creating-a-never-ending-background-service-in-android
// https://learn.microsoft.com/en-us/xamarin/android/app-fundamentals/services/creating-a-service/bound-services#bound-services-overview

// Oh my god fuck all of this
// Carve out my fucking eyes I never want to read Kotlin again 
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService : IMediaInterface
    {
        System.Timers.Timer playstateRefreshTimer = new(1000);
        MediaServiceConnection serviceConnection = new();

        public MediaService()
        {
            serviceConnection = new();
            Intent mediaServiceIntent = new Intent(Platform.AppContext, typeof(AndroidMediaService));
            Platform.AppContext.BindService(mediaServiceIntent, this.serviceConnection, Bind.AutoCreate);
        }

        [Obsolete]
        public void Initalize()
        {
            CheckPermissions();
            playstateRefreshTimer.Elapsed += UpdatePlaystateUi;

            // Creates background service

            // Do yer notification channel stuffs 
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }
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

        private void UpdatePlaystateUi(Object? source, ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlaybackTimeInfo? timeInfo = GetPlaybackTimeInfo();
                MauiProgram.MainPage.MiniPlayerController.UpdateTimestamp(timeInfo);
            });
        }
       
        public void Play()
        {
            if(serviceConnection != null && 
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Play();
                playstateRefreshTimer.Start();
            }
        }

        public void Pause()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Pause();
                playstateRefreshTimer.Stop();
            }
        }

        public void TogglePlay()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.TogglePlay();
                if (serviceConnection.Binder.GetIsPlaying())
                {
                    playstateRefreshTimer.Start();
                }
                else
                {
                    playstateRefreshTimer.Stop();
                }
            }
        }

        public void ToggleShuffle()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.ToggleShuffle();
            }
        }

        public void ToggleRepeat()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.ToggleRepeat();
            }
        }

        public void NextTrack()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Next();
            }
        }

        public void PreviousTrack()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Previous();
            }
        }

        public void SeekToPosition(long position)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.SeekToPosition(position);
            }
        }

        public void SeekToIndex(int index)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.SeekToIndex(index);
            }
        }

        public void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.SetPlayingCollection(baseMusicItem, fromIndex);
            }
        }

        public void AddSong(Song song)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.AddSong(song);
            }
        }

        public void AddSongs(Song[] songs)
        {
            throw new NotImplementedException();
        }

        public void RemoveSong(int index)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.RemoveSong(index);
            }
        }

        public Song[] GetQueue()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetQueue();
            }
            return Array.Empty<Song>();
        }

        public int GetQueueIndex()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetQueueIndex();
            }
            return 0;
        }
        public bool GetIsPlaying()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetIsPlaying();
            }
            return false;
        }
        public bool GetIsShuffling()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetIsShuffling();
            }
            return false;
        }
        public int GetRepeatMode()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetRepeatMode();
            }
            return 0;
        }

        public Song GetCurrentlyPlaying()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetCurrentlyPlaying();
            }
            return Song.Empty;
        }

        public PlaybackTimeInfo? GetPlaybackTimeInfo()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetPlaybackTimeInfo();
            }
            return null;
        }
    }

    [Service(Name="PortaJel.MediaService", IsolatedProcess=true, ForegroundServiceType=Android.Content.PM.ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }
        public IExoPlayer? Exoplayer;
        private int repeatMode = 0;

        public int playingIndex { get; private set; } = 0;
        public BaseMusicItem? playingFrom { get; private set; } = null;
        public List<Song> songQueue { get; private set; } = new();

        public AndroidMediaService()
        {
            
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
                    Exoplayer = newBuilder.Build();
                }
                if (Exoplayer != null)
                {
                    Exoplayer.RepeatMode = IPlayer.RepeatModeOff;
                }
                string deviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            }

            if (Exoplayer != null)
            {
                Exoplayer.Prepare();
                Exoplayer.PlayWhenReady = true;
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
            if(Exoplayer != null)
            {
                Exoplayer.Play();
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Pause();
                return true;
            }
            return false;
        }

        public bool TogglePlay()
        {
            if (Exoplayer != null)
            {
                if (Exoplayer.IsPlaying)
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
            if (Exoplayer != null)
            {
                Exoplayer.SeekTo(position);
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
            if (Exoplayer != null)
            {
                repeatMode = (int)setRepeatMode;
                Exoplayer.RepeatMode = repeatMode;
                return true;
            }
            return false;
        }

        public bool ToggleRepeat()
        {
            if (Exoplayer != null)
            {
                switch (Exoplayer.RepeatMode)
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
                Exoplayer.RepeatMode = repeatMode;
                return true;
            }
            return false;
        }

        public bool SetShuffle(bool isShullfing)
        {
            if(Exoplayer != null)
            {
                Exoplayer.ShuffleModeEnabled = isShullfing;
                return true;
            }
            return false;
        }

        public bool GetShuffle()
        {
            if (Exoplayer != null)
            {
                return Exoplayer.ShuffleModeEnabled;
            }
            return false;
        }

        public bool ToggleShuffle()
        {
            if (Exoplayer != null)
            {
                Exoplayer.ShuffleModeEnabled = !Exoplayer.ShuffleModeEnabled;
                return Exoplayer.ShuffleModeEnabled;
            }
            return false;
        }

        public bool Next()
        {
            if (Exoplayer != null)
            {
                if (Exoplayer.HasNext)
                {
                    Exoplayer.Next();
                    playingIndex += 1;
                }
                return true;
            }
            return false;
        }

        public bool Previous()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Previous();
                playingIndex -= 1;
                return true;
            }
            return false;
        }

        public bool SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            if(Exoplayer != null)
            {
                playingFrom = baseMusicItem;

                Exoplayer.ClearMediaItems();

                foreach (Song song in GetQueue())
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.streamUrl);
                    Exoplayer.AddMediaItem(mediaItem);
                }

                // Seek to position of that last song
                playingIndex = fromIndex;
                Exoplayer.SeekTo(playingIndex, Exoplayer.Duration);
                return true;
            }
            return false;
        }

        public bool AddSong(Song song)
        {
            if(Exoplayer != null)
            {
                songQueue.Add(song);

                int index = (playingIndex + 1) + songQueue.Count();

                if(song.streamUrl != null)
                {
                    MediaItem? mediaItem = MediaItem.FromUri(song.streamUrl);
                    Exoplayer.AddMediaItem(index, mediaItem);
                }

                return true;
            }
            return false;
        }

        public bool AddSongs(Song[] songs)
        {
            if (Exoplayer != null)
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
            if (Exoplayer != null)
            {
                songQueue.RemoveAt(index);
                Exoplayer.RemoveMediaItem(index);

                return true;
            }
            return true;
        }

        public Song[] GetQueue()
        {
            if(playingFrom == null)
            {
                return Array.Empty<Song>();
            }

            List<Song> queue = new();
            List<Song> songList = new List<Song>();

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
            if(playingIndex < songList.Count)
            {
                for (int i = playingIndex; i < songList.Count; i++)
                {
                    queue.Add(songList[i]);
                }
            }

            return queue.ToArray();
        }

        public int GetQueueIndex()
        {
            return playingIndex;
        }
        public bool GetIsPlaying()
        {
            if(Exoplayer != null)
            {
                return Exoplayer.PlayWhenReady;
            }
            return false;
        }
        public bool GetIsShuffling()
        {
            if (Exoplayer != null)
            {
                return Exoplayer.ShuffleModeEnabled;
            }
            return false;
        }
        public int GetRepeatMode()
        {
            if(Exoplayer != null)
            {
                return Exoplayer.RepeatMode;
            }
            return 0;
        }
        public Song GetCurrentlyPlaying()
        {
            if (Exoplayer != null)
            {
                return GetQueue()[playingIndex];
            }
            return Song.Empty;
        }
        public PlaybackTimeInfo? GetPlaybackTimeInfo()
        {
            if (Exoplayer != null)
            {
                PlaybackTimeInfo? newTime = new(
                        Exoplayer.CurrentPosition,
                        Exoplayer.Duration,
                        GetCurrentlyPlaying().id
                    );
                return newTime;
            }
            return null;
        }
    }
    public class MediaServiceBinder : Binder
    {
        public AndroidMediaService Service { get; private set; }
        public MediaServiceBinder(AndroidMediaService service)
        {
            this.Service = service;
        }

        public bool Play()
        {
            return Service.Play();
        }
        public bool TogglePlay()
        {
            return Service.TogglePlay();
        }
        public bool Pause()
        {
            return Service.Pause();
        }
        public bool SeekToPosition(long position)
        {
            return Service.SeekToPosition(position);
        }
        public bool SeekToIndex(int index)
        {
            return Service.SeekToIndex(index);
        }
        public bool SetRepeat(MediaServiceRepeatMode repeatMode)
        {
            return Service.SetRepeat(repeatMode);
        }
        public bool ToggleRepeat()
        {
            return Service.ToggleRepeat();
        }
        public bool SetShuffle(bool isShullfing)
        {
            return Service.SetShuffle(isShullfing);
        }
        public bool GetShuffle()
        {
            return Service.GetShuffle();
        }
        public bool ToggleShuffle()
        {
            return Service.ToggleShuffle();
        }
        public bool Next()
        {
            return Service.Next();
        }
        public bool Previous()
        {
            return Service.Previous();
        }
        public bool SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            return Service.SetPlayingCollection(baseMusicItem, fromIndex);
        }
        public bool AddSong(Song song)
        {
            return Service.AddSong(song);
        }
        public bool AddSongs(Song[] songs)
        {
            return Service.AddSongs(songs);
        }
        public bool RemoveSong(int index)
        {
            return Service.RemoveSong(index);
        }
        public Song[] GetQueue() 
        {
            return Service.GetQueue();
        }
        public int GetQueueIndex()
        {
            return Service.GetQueueIndex();
        }
        public bool GetIsPlaying()
        {
            return Service.GetIsPlaying();
        }
        public bool GetIsShuffling()
        {
            return Service.GetIsShuffling();
        }
        public int GetRepeatMode()
        {
            return Service.GetRepeatMode();
        }
        public Song GetCurrentlyPlaying()
        {
            return Service.GetCurrentlyPlaying();
        }
        public PlaybackTimeInfo? GetPlaybackTimeInfo()
        {
            return Service.GetPlaybackTimeInfo();
        }
    }
    public class MediaServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string? TAG = typeof(MediaServiceConnection).FullName;

        public MediaServiceConnection()
        {
            IsConnected = false;
            Binder = null;
        }

        public bool IsConnected { get; private set; } = false;
        public MediaServiceBinder? Binder { get; private set; }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            Binder = service as MediaServiceBinder;
            IsConnected = this.Binder != null;

            if (name != null)
            {
                string message = "onServiceConnected - ";
                Log.Debug(TAG, $"OnServiceConnected {name.ClassName}");

                if (IsConnected)
                {
                    message = message + " bound to service " + name.ClassName;
                    // mainActivity.UpdateUiForBoundService();
                }
                else
                {
                    message = message + " not bound to service " + name.ClassName;
                    // mainActivity.UpdateUiForUnboundService();
                }

                Log.Info(TAG, message);
                // mainActivity.timestampMessageTextView.Text = message;
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            if(name != null)
            {
                Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
            }
            IsConnected = false;
            Binder = null;
            // mainActivity.UpdateUiForUnboundService();
        }
    }
    public enum MediaServiceRepeatMode
    {
        None = 0,
        One = 1,
        All = 2
    }
}
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
