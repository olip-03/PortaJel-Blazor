using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using SkiaSharp;
using System.Diagnostics;
using System.Threading;
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
    private bool IsBackgroundMoving = false;

    private bool isPlaying = false;
    private bool pauseTimeUpdate = false;
    private Guid currentPlayingId = Guid.Empty;

    public MediaController()
    {
        ViewModel.HeaderHeightValue = MauiProgram.SystemHeaderHeight;

        InitializeComponent();
        BindingContext = ViewModel;
    }
    public async Task<bool> Open(bool? animate = true)
    {
        UpdatePlayButton();
        UpdateFavouriteButton();
        UpdateRepeatButton();

        ViewModel.HeaderHeightValue = MauiProgram.SystemHeaderHeight;
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

        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.UI);
        }

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
        // Turn off accelerometer
        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.Stop();
            Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
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
                this.currentPlayingId = fromIndex.Id;
                ImgCarousel.ScrollTo(fromIndex, animate: false);
            }
        }
        else if (ViewModel.Queue != null && ViewModel.Queue.Count() > playFromIndex && songs != null)
        {
            Song? fromIndex = songs.AllSongs[(int)playFromIndex];
            if (playFromIndex != null && fromIndex != null)
            {
                this.currentPlayingId = fromIndex.Id;
                ImgCarousel.ScrollTo(fromIndex, animate: false);
            }
        }

        BaseMusicItem? playingCollection = MauiProgram.MediaService.GetPlayingCollection();
        if (playingCollection != null)
        {
            string type = playingCollection.GetType().Name;
            ViewModel.PlayingFromCollectionTitle = "Playing from " + type;
            if(type == "Album")
            {
                ViewModel.PlayingFromTitle = playingCollection.ToAlbum().Name;
            }
            if (type == "Playlist")
            {
                ViewModel.PlayingFromTitle = playingCollection.ToPlaylist().Name;
            }
        }
        else
        {
            ViewModel.PlayingFromCollectionTitle = string.Empty;
            ViewModel.PlayingFromTitle = string.Empty;
        }

        // Update Elements
        UpdatePlayButton();
        UpdateFavouriteButton();
        UpdateRepeatButton();
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

        if (song.IsFavourite)
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
                await MauiProgram.api.SetFavourite(song.Id, song.ServerAddress, song.IsFavourite);
            });
        }
    }

    private void UpdateRepeatButton()
    {
        if (MauiProgram.MediaService == null) return;
        if (App.Current == null) return;
        int repeatMode = MauiProgram.MediaService.GetRepeatMode();
        if (repeatMode == 0)
        { // REPEAT_MODE_OFF = 0
            ImgCarousel.Loop = false;
            var hasSource = App.Current.Resources.TryGetValue("RepeatOffIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.RepeatButtonSource = (string)imageSource;
            }
        }
        if (repeatMode == 1)
        { // REPEAT_MODE_ONE = 1
            ImgCarousel.Loop = false;
            var hasSource = App.Current.Resources.TryGetValue("RepeatOneIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.RepeatButtonSource = (string)imageSource;
            }
        }
        if (repeatMode == 2)
        { // REPEAT_MODE_ALL = 2
            ImgCarousel.Loop = true;
            var hasSource = App.Current.Resources.TryGetValue("RepeatAllIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.RepeatButtonSource = (string)imageSource;
            }
        }
    }

    public async Task<bool> UpdateBackground()
    {
        if (MauiProgram.MediaService == null) return false;
        MemoryStream? imageDecodeStream = null;
        Song currentSong = MauiProgram.MediaService.GetCurrentlyPlaying();
        if (currentSong.ImgBlurhash == currentBlurhash) return false; // dont run if hash is the same
        currentBlurhash = currentSong.ImgBlurhash;
        await Task.WhenAll(Task.Run(async () =>
        {
            // Fuck yeah get the image to move based on gyro
            //Microsoft.Maui.Devices.Sensors.Accelerometer.Start<
            string? base64 = await MusicItemImage.BlurhashToBase64Async(currentSong.ImgBlurhash, 100, 100, 0.3f).ConfigureAwait(false);
            if (base64 != null)
            {
                var imageBytes = Convert.FromBase64String(base64);
                imageDecodeStream = new(imageBytes);
            }
        }), BackgroundImage.FadeTo(0, 1000, Easing.SinOut));
         
        ViewModel.BackgroundImageSource = ImageSource.FromStream(() => imageDecodeStream);
        await BackgroundImage.FadeTo(1, 1000, Easing.SinIn);
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
            if (ViewModel.Queue != null && ViewModel.Queue.Count > playbackTime.PlayingIndex)
            {
                if (!playbackTime.CurrentSong.Id.Equals(this.currentPlayingId))
                {
                    ImgCarousel.ScrollTo(ViewModel.Queue[playbackTime.PlayingIndex], animate: true);
                    await Task.Delay(500);
                    UpdateData(playFromIndex: playbackTime.PlayingIndex);
                    this.currentPlayingId = playbackTime.CurrentSong.Id;
                }
            }

            // Change playback icon 
            if (isPlaying != playbackTime.IsPlaying)
            {
                UpdatePlayButton(playbackTime.IsPlaying);
                MauiProgram.MainPage.MainMiniPlayer.UpdatePlayButton(playbackTime.IsPlaying);

                isPlaying = playbackTime.IsPlaying;
            }

            ViewModel.PlaybackValue = playbackTime.CurrentDuration.TotalMilliseconds;
            ViewModel.PlaybackMaximum = playbackTime.Duration.TotalMilliseconds;

            ViewModel.PlaybackTimeValue = string.Format("{0:D2}:{1:D2}", playbackTime.CurrentDuration.Minutes, playbackTime.CurrentDuration.Seconds);
            ViewModel.PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", playbackTime.Duration.Minutes, playbackTime.Duration.Seconds);
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

        var base64Result = await MusicItemImage.BlurhashToBase64Async(song.ImgBlurhash, 200, 200);
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

        if (MauiProgram.MediaService == null) return;
        MauiProgram.MediaService.ToggleRepeat();
        UpdateRepeatButton();
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

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying();
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

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying(); 
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
        bool state = !song.IsFavourite;

        ViewModel.Queue[queueIndex].SetIsFavourite(state);

        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdateFavouriteButton(syncToServer: true);
        await Task.Run(() => MauiProgram.api.SetFavourite(song.Id, song.ServerAddress, state));
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

    private async void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        await BackgroundImage.TranslateTo((BackgroundImage.Width / 4) * (double)e.Reading.Acceleration.X, 0, 500, Easing.SinOut);
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
        Guid? itemId = MauiProgram.MediaService.GetCurrentlyPlaying().AlbumId;
            if (itemId != null)
        {
            MauiProgram.WebView.NavigateAlbum((Guid)itemId);
            await Close();
        }
        else
        {
            string text = $"Couldn't navigate to album as it was null!";
            var toast = Toast.Make(text, ToastDuration.Short, 14);
            await toast.Show();
            MauiProgram.WebView.NavigateHome();
        }
    }

    private async void ArtistLabel_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MainPage.ShowLoadingScreen(true);
        Guid? itemId = MauiProgram.MediaService.GetCurrentlyPlaying().ArtistIds.FirstOrDefault();
        if (itemId != null)
        {
            MauiProgram.WebView.NavigateArtist((Guid)itemId);
            await Close();
        }
        else
        {
            string text = $"Couldn't navigate to artist as it was null!";
            var toast = Toast.Make(text, ToastDuration.Short, 14);
            await toast.Show();
            MauiProgram.WebView.NavigateHome();
        }
    }
}