using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V4.Media.Session;
using Android.Widget;
using AndroidX.Core.App;
using Com.Google.Android.Exoplayer2;
using Android.Util;
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
using System.Dynamic;
using static Android.Icu.Text.Transliterator;
using Android.Views.Animations;

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
    public partial class MediaService
    {
        [Obsolete] IExoPlayer? Exoplayer;
        System.Timers.Timer myTimer = null;
        MediaServiceConnection serviceConnection = new();

        [Obsolete]
        partial void Initalize()
        {
            isPlaying = false;

            CheckPermissions();

            // Creates background service
            Intent mediaServiceIntent = new Intent(Platform.AppContext, typeof(AndroidMediaService));
            Platform.AppContext.BindService(mediaServiceIntent, this.serviceConnection, Bind.AutoCreate);

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
        public async void UpdateCurrentlyPlaying()
        {
            Song? getSong = null;
            Album? getAlbum = null;

            if (Exoplayer == null || Microsoft.Maui.Controls.Application.Current == null)
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
                if (getSong.album != Album.Empty)
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
        
        partial void Play()
        {
            if(serviceConnection != null && 
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Play();
            }
            //  ThrowNotification(AppInfo.PackageName);
            //  UpdateCurrentlyPlaying();
            //  if (Exoplayer!= null)
            //  {
            //      isPlaying = true;
            //      Exoplayer.Play();
                
            //      MauiProgram.mainPage.RefreshPlayState();
            //  }
        }
        partial void Pause()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Pause();
            }
            // if (Exoplayer != null)
            // {
            //     Exoplayer.Pause();
            //     MauiProgram.mainPage.RefreshPlayState();
            // }
        }

        partial void TogglePlay()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.TogglePlay();
            }
            //isPlaying = !isPlaying;
            //if (isPlaying)
            //{
            //    Play();
            //}
            //else
            //{
            //    Pause();
            //}
        }

        partial void ToggleShuffle()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.ToggleShuffle();
            }
            //if (Exoplayer != null)
            //{
            //    Exoplayer.ShuffleModeEnabled = !Exoplayer.ShuffleModeEnabled;
            //    shuffleOn = Exoplayer.ShuffleModeEnabled;
            //}
        }

        partial void ToggleRepeat()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.ToggleRepeat();
            }
            //if (Exoplayer != null)
            //{
            //    switch (Exoplayer.RepeatMode)
            //    {
            //        case IPlayer.RepeatModeAll:
            //            Exoplayer.RepeatMode = IPlayer.RepeatModeOff;
            //            repeatMode = 0;
            //            break;
            //        case IPlayer.RepeatModeOne:
            //            Exoplayer.RepeatMode = IPlayer.RepeatModeAll;
            //            repeatMode = 2;
            //            break;
            //        case IPlayer.RepeatModeOff:
            //            Exoplayer.RepeatMode = IPlayer.RepeatModeOne;
            //            repeatMode = 1;
            //            break;
            //    }
            //}
        }

        partial void NextTrack()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Next();
            }
            //if (Exoplayer != null)
            //{
            //    Exoplayer.Next();
            //}
        }

        partial void PreviousTrack()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Previous();
            }
            //if (Exoplayer != null)
            //{
            //    Exoplayer.Previous();
            //}
        }

        partial void SeekTo(long position)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.SeekToPosition(position);
            }
            //if(Exoplayer != null)
            //{
            //    Exoplayer.SeekTo(position);
            //}
        }
    }

    [Service(Exported = true, Name = "PortaJel.MediaService", IsolatedProcess = true, ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaPlayback)]
    public class AndroidMediaService : Service
    {
        public IBinder? Binder { get; private set; }
        public IExoPlayer? Exoplayer;
        private int repeatMode = 0;

        public int playingIndex { get; private set; } = 0;
        public BaseMusicItem? playingFrom { get; private set; } = null;
        public List<Song> songQueue { get; private set; } = new();

        AndroidMediaService()
        {
            var HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
            var MainDataSource = new ProgressiveMediaSource.Factory(HttpDataSourceFactory);
            if (Platform.AppContext != null && MainDataSource != null)
            {
                Exoplayer = new IExoPlayer.Builder(Platform.AppContext).SetMediaSourceFactory(MainDataSource).Build();
                Exoplayer.RepeatMode = IPlayer.RepeatModeOff;
                string deviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            }
        }

        public override IBinder OnBind(Intent? intent)
        {
            this.Binder = new MediaServiceBinder(this);
            return this.Binder;
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
        public bool TogglePlay()
        {
            if (Exoplayer != null)
            {
                if (Exoplayer.IsPlaying)
                {
                    Exoplayer.Pause();
                }
                else
                {
                    Exoplayer.Play();
                }
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
                Exoplayer.Next();
                return true;
            }
            return false;
        }
        public bool Previous()
        {
            if (Exoplayer != null)
            {
                Exoplayer.Previous();
                return true;
            }
            return false;
        }
        public bool SetPlayingCollection(BaseMusicItem baseMusicItem)
        {
            playingFrom = baseMusicItem;
            return true;
        }
        public bool AddSong(Song song)
        {
            songQueue.Add(song);
            return true;
        }
        public bool RemoveSong(int index)
        {
            songQueue.RemoveAt(index);
            return true;
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
        public bool SetPlayingCollection(BaseMusicItem baseMusicItem)
        {
            return Service.SetPlayingCollection(baseMusicItem);
        }
        public bool AddSong(Song song)
        {
            return Service.AddSong(song);
        }
        public bool RemoveSong(int index)
        {
            return Service.RemoveSong(index);
        }
    }
    public class MediaServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string TAG = typeof(MediaServiceConnection).FullName;

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

        public void OnServiceDisconnected(ComponentName? name)
        {
            Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
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
