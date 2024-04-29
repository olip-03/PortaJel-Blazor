using Android.App;
using Android.Content;
using Com.Google.Android.Exoplayer2;
using Android.Util;
using Com.Google.Android.Exoplayer2.Source;
using PortaJel_Blazor.Platforms.Android.MediaService;
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
            Platform.AppContext.StartForegroundService(mediaServiceIntent);
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

    public enum MediaServiceRepeatMode
    {
        None = 0,
        One = 1,
        All = 2
    }
}
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
