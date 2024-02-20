using Android.Content;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;

// Android Implementation
// https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui.MediaElement/Views/MediaManager.android.cs
// for advice
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService
    {
        public partial void Play()
        {
            var HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
            var MainDataSource = new ProgressiveMediaSource.Factory(HttpDataSourceFactory);
            var Exoplayer = new IExoPlayer.Builder(Platform.AppContext).SetMediaSourceFactory(MainDataSource).Build();

            var mediaItem1 = MediaItem.FromUri(Android.Net.Uri.Parse("https://ia800806.us.archive.org/15/items/Mp3Playlist_555/AaronNeville-CrazyLove.mp3"));
            var mediaItem2 = MediaItem.FromUri(Android.Net.Uri.Parse("http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4"));

            Exoplayer.AddMediaItem(mediaItem1);
            Exoplayer.AddMediaItem(mediaItem2);
            Exoplayer.Prepare();
            Exoplayer.PlayWhenReady = true;
        }
        public partial void Pause()
        {
            
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

        }
        public partial void ToggleRepeat()
        {

        }
        public partial void NextTrack()
        {

        }
        public partial void PreviousTrack()
        {

        }
    }
}
