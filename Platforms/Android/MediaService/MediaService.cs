using Android.Content;
using PortaJel_Blazor.Platforms.Android.MediaService;
using PortaJel_Blazor.Data;
using System.Timers;
using Android.OS;
using System.Runtime.CompilerServices;
using Java.Lang;

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
        public Action? PlayAddonAction { get; set; } = null;

        MediaServiceConnection serviceConnection = new();
        IDispatcherTimer? DispatcherTimer= null;

        public MediaService()
        {
            serviceConnection = new();
            Intent mediaServiceIntent = new Intent(Platform.AppContext, typeof(AndroidMediaService));
            Platform.AppContext.StartForegroundService(mediaServiceIntent);
            Platform.AppContext.BindService(mediaServiceIntent, this.serviceConnection, Bind.AutoCreate);
        }

        [Obsolete]
        public async Task<bool> Initalize()
        {
            try
            {
                CheckPermissions();

                if (Application.Current != null)
                {
                    DispatcherTimer = Application.Current.Dispatcher.CreateTimer();

                    DispatcherTimer.Interval = TimeSpan.FromSeconds(0.25);
                    DispatcherTimer.IsRepeating = true;
                    DispatcherTimer.Tick += (s, e) => UpdatePlaystateUi();
                    DispatcherTimer.Start();
                }

                // Dont allow init to complete until the Binding has been made. Idgaf if we gotta wait a while 
                while (serviceConnection.Binder == null)
                {
                    await Task.Delay(100).ConfigureAwait(false); // Adjust the delay as needed
                }
                return true;
            }
            catch (System.Exception)
            {
                return false;
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
                || Platform.AppContext.CheckSelfPermission(Android.Manifest.Permission.AccessNotificationPolicy) != Android.Content.PM.Permission.Granted )
            {
                if(Platform.CurrentActivity != null)
                {
                    Platform.CurrentActivity.RequestPermissions(notifPerms, requestLocationId);
                }
            }
        }

        private void UpdatePlaystateUi()
        {
            PlaybackInfo? timeInfo = GetPlaybackTimeInfo();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
                MauiProgram.MainPage.MainMediaController.UpdateTimestamp(timeInfo);
            });
        }
       
        public void Play()
        {
            if(serviceConnection != null && 
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Play();
                UpdatePlaystateUi();
                // DispatcherTimer.Start();
                if(PlayAddonAction != null)
                {
                    PlayAddonAction();
                }
            }
        }

        public void SetPlayAddonAction(Action addonAction)
        {
            PlayAddonAction = addonAction;
        }

        public void Pause()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.Pause();
                UpdatePlaystateUi();
                if (PlayAddonAction != null)
                {
                    PlayAddonAction();
                }
                // DispatcherTimer.Stop();
            }
        }

        public void TogglePlay()
        {
            if (serviceConnection != null &&
               serviceConnection.Binder != null)
            {
                serviceConnection.Binder.TogglePlay();
                if (serviceConnection.Binder.GetIsPlaying())
                { // Is Playing
                    UpdatePlaystateUi();
                    if (PlayAddonAction != null)
                    {
                        PlayAddonAction();
                    }
                    // DispatcherTimer.Start();
                }
                else
                { // Is Paused
                    UpdatePlaystateUi();
                    if (PlayAddonAction != null)
                    {
                        PlayAddonAction();
                    }
                    // DispatcherTimer.Stop();
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
                MauiProgram.currentAlbumGuid = baseMusicItem.id;
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
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.AddSongs(songs);
            }
        }

        public void RemoveSong(int index)
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                serviceConnection.Binder.RemoveSong(index);
            }
        }

        public SongGroupCollection GetQueue()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetQueue();
            }
            return new SongGroupCollection();
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

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            if (serviceConnection != null &&
                serviceConnection.Binder != null)
            {
                return serviceConnection.Binder.GetPlaybackTimeInfo();
            }
            return null;
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
