using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using SkiaSharp;
using System.Diagnostics;
namespace PortaJel_Blazor.Shared;

public partial class MediaController : ContentView
{
    public MediaControllerViewModel ViewModel { get; set; } = new();

    public bool IsOpen { get; private set; } = false;
    public double PositionX { get => TranslationX; set => TranslationX = value; }
    public double PositionY { get => TranslationY; set => TranslationY = value; }

    private double btnInOpacity = 0.5;
    private double btnInSize = 0.8;
    private uint btnAnimSpeedMs = 400;
    private string currentBlurhash = string.Empty;

    private bool isPlaying = false;
    private bool pauseTimeUpdate = false;
    private Guid currentPlayingId = Guid.Empty;

    public MediaController()
    {
        InitializeComponent();
        BindingContext = ViewModel;
    }

    public async Task<bool> Open(bool? animate = true)
    {
        UpdatePlayButton();
        UpdateFavouriteButton();

        Opacity = 1;
        TranslationY = MauiProgram.MainPage.ContentHeight;
        BackgroundImage.WidthRequest = MauiProgram.MainPage.ContentHeight;
        IsVisible = true;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut)).ConfigureAwait(false); ;
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = MauiProgram.MainPage.ContentHeight * -1;
            TranslationY = 0;
        }
        IsOpen = true;
        return true;
    }

    public async Task<bool> Close(bool? animate = true)
    {
        IsOpen = false;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, 0, 450, Easing.SinOut),
                this.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut)).ConfigureAwait(false);
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = 0;
            TranslationY = MauiProgram.MainPage.ContentHeight;
        }
        return true;
    }

    public async void UpdateData(SongGroupCollection? songs = null, int? playFromIndex = 0)
    {
        if (MauiProgram.MediaService == null) return;
        if (playFromIndex == null) playFromIndex = 0;

        if (songs != null)
        {
            ViewModel.Queue = songs.AllSongs;
            Song? fromIndex = songs.AllSongs[(int)playFromIndex];

            if (playFromIndex != null)
            {
                this.currentPlayingId = fromIndex.id;
                ImgCarousel.ScrollTo(fromIndex, animate: false);
            }
        }
        else if (ViewModel.Queue != null && ViewModel.Queue.Count() > playFromIndex && songs != null)
        {
            Song? fromIndex = songs.AllSongs[(int)playFromIndex];
            if (playFromIndex != null && fromIndex != null)
            {
                this.currentPlayingId = fromIndex.id;
                ImgCarousel.ScrollTo(fromIndex, animate: false);
            }
        }

        BaseMusicItem? playingCollection = MauiProgram.MediaService.GetCurrentlyPlaying().album;
        if (playingCollection != null)
        {
            string type = playingCollection.GetType().Name;
            ViewModel.PlayingFromCollectionTitle = "Playing from " + type;
            ViewModel.PlayingFromTitle = playingCollection.name;
        }

        // Update Elements
        UpdatePlayButton();
        UpdateFavouriteButton();
        await UpdateBackground();

        // Update time tracking
        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
    }
    public void InsertIntoQueue(Song song)
    {
        // Insert item into viewmodel
        if (MauiProgram.MediaService == null) return;
        if (ViewModel.Queue == null) ViewModel.Queue = new();

        try
        {
            SongGroupCollection sgc = MauiProgram.MediaService.GetQueue();
            int insertInto = sgc.QueueStartIndex + sgc.QueueCount;
            ViewModel.Queue.Insert(insertInto, song);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
        }
    }
    public void UpdatePlayButton(bool? isPlaying = null)
    {
        if (App.Current == null) return;
        if (MauiProgram.MediaService == null) return;

        if (isPlaying == true)
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (isPlaying == false)
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (MauiProgram.MediaService.GetIsPlaying())
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
    }
    public async void UpdateFavouriteButton(bool? syncToServer = false)
    {
        if (App.Current == null) return;
        if (MauiProgram.MediaService == null) return;
        if (ViewModel.Queue == null) ViewModel.Queue = MauiProgram.MediaService.GetQueue().AllSongs;

        int queueIndex = MauiProgram.MediaService.GetQueueIndex();
        Song song = ViewModel.Queue[queueIndex];

        if (song.isFavourite)
        {
            var hasColor = App.Current.Resources.TryGetValue("PrimaryColor", out object primaryColor);
            var hasSource = App.Current.Resources.TryGetValue("HeartIcon", out object imageSource);

            if (hasColor)
            {
                ViewModel.FavButtonColor = (Color)primaryColor;
            }
            if (hasSource)
            {
                ViewModel.FavButtonSource = (string)imageSource;
            }
        }
        else
        {
            var hasColor = App.Current.Resources.TryGetValue("PrimaryTextColor", out object primaryColor);
            var hasSource = App.Current.Resources.TryGetValue("HeartEmptyIcon", out object imageSource);

            if (hasColor)
            {
                ViewModel.FavButtonColor = (Color)primaryColor;
            }
            if (hasSource)
            {
                ViewModel.FavButtonSource = (string)imageSource;
            }
        }

        if (syncToServer == true)
        {
            await Task.Run(async () =>
            {
                await MauiProgram.api.SetFavourite(song.id, song.serverAddress, song.isFavourite);
            });
        }
    }
    public async Task<bool> UpdateBackground()
    {
        if (MauiProgram.MediaService == null) return false;
        MemoryStream? imageDecodeStream = null;
        Song currentSong = MauiProgram.MediaService.GetCurrentlyPlaying();
        if (currentSong.image.blurHash == currentBlurhash) return false; // dont run if hash is the same
        currentBlurhash = currentSong.image.blurHash;
        await Task.Run(async () =>
        {
            // Fuck yeah get the image to move based on gyro
            //Microsoft.Maui.Devices.Sensors.Accelerometer.Start<
            string? base64 = await currentSong.image.BlurhashToBase64Async(100, 100, 0.25f).ConfigureAwait(false);
            if (base64 != null)
            {
                var imageBytes = Convert.FromBase64String(base64);
                imageDecodeStream = new(imageBytes);
            }
        });
        ViewModel.BackgroundImageSource = ImageSource.FromStream(() => imageDecodeStream);
        return true;
    }
    /// <summary>
    /// Function which can be called to update the playing song informations time, and which song is currently playing.
    /// </summary>
    /// <param name="playbackTime">Time Information derived from the media player</param>
    public async void UpdateTimestamp(PlaybackInfo? playbackTime)
    {
        if (playbackTime != null)
        {
            // Check if current song is accurate
            if (ViewModel.Queue != null && ViewModel.Queue.Count > playbackTime.playingIndex)
            {
                if (!playbackTime.currentSong.id.Equals(this.currentPlayingId))
                {
                    ImgCarousel.ScrollTo(ViewModel.Queue[playbackTime.playingIndex], animate: true);
                    await Task.Delay(500);
                    UpdateData(playFromIndex: playbackTime.playingIndex);
                    this.currentPlayingId = playbackTime.currentSong.id;
                }
            }

            // Change playback icon 
            if (isPlaying != playbackTime.isPlaying)
            {
                UpdatePlayButton(playbackTime.isPlaying);
                MauiProgram.MainPage.MainMiniPlayer.UpdatePlayButton(playbackTime.isPlaying);

                isPlaying = playbackTime.isPlaying;
            }

            ViewModel.PlaybackValue = playbackTime.currentDuration;
            ViewModel.PlaybackMaximum = playbackTime.currentSong.duration;

            if (playbackTime.currentSong.duration > 0 &&
                !pauseTimeUpdate &&
                ViewModel.PlaybackTimeValue != playbackTime.currentDurationText &&
                IsOpen)
            {
                ViewModel.PlaybackTimeValue = playbackTime.currentDurationText;
                ViewModel.PlaybackMaximumTimeValue = playbackTime.fullDurationText;

                // Player_DurationSlider_Lbl_DurationTxt.Text = ViewModel.PlaybackMaximumTimeValue;
            }
        }
    }

    private async void Btn_Close_Clicked(object sender, EventArgs e)
    {
        await Close();
    }

    private async void Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();

        var base64Result = await song.image.BlurhashToBase64Async(200, 200);
        string base64 = base64Result == null ? string.Empty : base64Result;

        MauiProgram.MainPage.OpenContextMenu(song, 250, base64);
    }

    /// <summary>
    /// REPEAT button pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Btn_Repeat_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Repeat.Opacity = btnInOpacity;
        Btn_Repeat.Scale = btnInSize;
    }

    /// <summary>
    /// REPEAT button released
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Btn_Repeat_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_Repeat.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Repeat.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_Previous_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Previous.Opacity = btnInOpacity;
        Btn_Previous.Scale = btnInSize;
    }

    private async void Btn_Previous_Released(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MediaService.PreviousTrack();

        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
        MauiProgram.MainPage.MainMediaController.UpdateTimestamp(timeInfo);

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying();
        TimeSpan durationTime = TimeSpan.FromTicks(scrollTo.duration);

        ViewModel.PlaybackValue = 0;
        ViewModel.PlaybackTimeValue = "00:00";
        ViewModel.PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", durationTime.Minutes, durationTime.Seconds);

        ImgCarousel.ScrollTo(item: scrollTo, animate: true);
        ImgCarousel.ScrollTo(item: scrollTo, animate: true);

        int currentIndex = MauiProgram.MediaService.GetQueueIndex();
        MauiProgram.MainPage.MainMiniPlayer.UpdateData(playFromIndex: currentIndex);

        await Task.WhenAll(
            Btn_Previous.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Previous.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
        await UpdateBackground();
    }

    private void Btn_PlayToggle_Pressed(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_PlayToggle.Opacity = btnInOpacity;
        Btn_PlayToggle.Scale = btnInSize;

        if (MauiProgram.MediaService.GetIsPlaying() && App.Current != null)
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (App.Current != null)
        {
            var hasSource = App.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
    }

    private async void Btn_PlayToggle_Released(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        MauiProgram.MediaService.TogglePlay();
        UpdatePlayButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdatePlayButton();

        await Task.WhenAll(
            Btn_PlayToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_PlayToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_Next_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Next.Opacity = btnInOpacity;
        Btn_Next.Scale = btnInSize;
    }
    private async void Btn_Next_Released(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MediaService.NextTrack();

        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
        MauiProgram.MainPage.MainMediaController.UpdateTimestamp(timeInfo);

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying();
        TimeSpan durationTime = TimeSpan.FromTicks(scrollTo.duration);

        ViewModel.PlaybackValue = 0;
        ViewModel.PlaybackTimeValue = "00:00";
        ViewModel.PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", durationTime.Minutes, durationTime.Seconds);

        ImgCarousel.ScrollTo(item: scrollTo, animate: true);

        int currentIndex = MauiProgram.MediaService.GetQueueIndex();
        MauiProgram.MainPage.MainMiniPlayer.UpdateData(playFromIndex: currentIndex);

        await Task.WhenAll(
            Btn_Next.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Next.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
        await UpdateBackground();
    }

    private void Btn_Shuffle_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Shuffle.Opacity = btnInOpacity;
        Btn_Shuffle.Scale = btnInSize;
    }

    private async void Btn_Shuffle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_Shuffle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Shuffle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }

    private async void Btn_FavToggle_Pressed(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = btnInOpacity;
        Btn_FavToggle.Scale = btnInSize;

        if (ViewModel.Queue == null)
        {
            return;
        }

        int queueIndex = MauiProgram.MediaService.GetQueueIndex();
        Song song = ViewModel.Queue[queueIndex];
        bool state = !song.isFavourite;

        ViewModel.Queue[queueIndex].isFavourite = state;

        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdateFavouriteButton(syncToServer: true);
        await Task.Run(() => MauiProgram.api.SetFavourite(song.id, song.serverAddress, state));
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_FavToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_FavToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_ShowQueue_Clicked(object sender, EventArgs e)
    {
        MauiProgram.MainPage.MainMediaQueue.Open(animate: true);
    }

    private void ImgCarousel_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {

    }

    private void ImgCarousel_ScrollToRequested(object sender, ScrollToRequestEventArgs e)
    {

    }

    private void ImgCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {

    }

    private void DurationSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (pauseTimeUpdate)
        {
            long time = (long)DurationSlider.Value;
            TimeSpan passedTime = TimeSpan.FromMilliseconds(time);
            ViewModel.PlaybackTimeValue = string.Format("{0:D2}:{1:D2}", passedTime.Minutes, passedTime.Seconds);
        }
    }

    private void DurationSlider_DragStarted(object sender, EventArgs e)
    {
        pauseTimeUpdate = true;
    }

    private void DurationSlider_DragCompleted(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        pauseTimeUpdate = false;
        long position = (long)DurationSlider.Value;
        MauiProgram.MediaService.SeekToPosition(position);
        UpdateTimestamp(MauiProgram.MediaService.GetPlaybackTimeInfo());
    }

    private void Queue_Btn_Close_Clicked(object sender, EventArgs e)
    {

    }

    private void Queue_Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {

    }

    private async void CollectionLabel_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MainPage.ShowLoadingScreen(true);
        await Close();
        Album? album = MauiProgram.MediaService.GetCurrentlyPlaying().album;
        if (album != null)
        {
            Guid? itemId = album.id;
            if (itemId != null)
            {
                MauiProgram.WebView.NavigateAlbum((Guid)itemId);
            }
        }
    }
}