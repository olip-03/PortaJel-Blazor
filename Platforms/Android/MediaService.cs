using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Media.Session;
using AndroidX.Core.App;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using PortaJel_Blazor.Data;

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

            // im not happy with it either 
            if(songQueue.GetQueue().Count() > 0)
            {
                getSong = songQueue.GetQueue().First();
            }
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
    }
}
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
