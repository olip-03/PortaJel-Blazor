using System.Diagnostics;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.ViewModels;

namespace PortaJel_Blazor.Shared.Xaml;

public partial class MediaController : ContentView
{
    private MediaControllerViewModel ViewModel { get; set; } = new();

    public bool IsOpen { get; private set; } = false;
    public double PositionX { get => TranslationX; set => TranslationX = value; }
    public double PositionY { get => TranslationY; set => TranslationY = value; }

    private const double BtnInOpacity = 0.5;
    private const double BtnInSize = 0.8;
    private const uint BtnAnimSpeedMs = 400;
    
    private string _currentBlurhash = string.Empty;
    private bool _isPlaying = false;
    private bool _pauseTimeUpdate = false;
    private Guid _currentPlayingId = Guid.Empty;

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
                this._currentPlayingId = fromIndex.Id;
                ImgCarousel.ScrollTo(fromIndex, animate: false);
            }
        }
        else if (ViewModel.Queue != null && ViewModel.Queue.Count() > playFromIndex && songs != null)
        {
            Song? fromIndex = songs.AllSongs[(int)playFromIndex];
            if (playFromIndex != null && fromIndex != null)
            {
                this._currentPlayingId = fromIndex.Id;
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
        if (Application.Current == null) return;
        if (MauiProgram.MediaService == null) return;

        if (isPlaying == true)
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (isPlaying == false)
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (MauiProgram.MediaService.GetIsPlaying())
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
    }
    
    public async void UpdateFavouriteButton(bool? syncToServer = false)
    {
        if (Application.Current == null) return;
        if (MauiProgram.MediaService == null) return;
        ViewModel.Queue ??= MauiProgram.MediaService.GetQueue().AllSongs;

        int queueIndex = MauiProgram.MediaService.GetQueueIndex();
        Song song = ViewModel.Queue?[queueIndex];

        if (song is { IsFavourite: true })
        {
            var hasColor = Application.Current.Resources.TryGetValue("PrimaryColor", out object primaryColor);
            var hasSource = Application.Current.Resources.TryGetValue("HeartIcon", out object imageSource);

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
            var hasColor = Application.Current.Resources.TryGetValue("PrimaryTextColor", out object primaryColor);
            var hasSource = Application.Current.Resources.TryGetValue("HeartEmptyIcon", out object imageSource);

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
            await Task.Run( () =>
            {
                if (song == null) return;
                var toRun = MauiProgram.Server.SetIsFavourite(song.Id, song.IsFavourite, song.ServerAddress);
                toRun.Wait();
            });
        }
    }

    private void UpdateRepeatButton()
    {
        if (MauiProgram.MediaService == null) return;
        if (Application.Current == null) return;
        int repeatMode = MauiProgram.MediaService.GetRepeatMode();
        switch (repeatMode)
        {
            case 0:
            {
                // REPEAT_MODE_OFF = 0
                ImgCarousel.Loop = false;
                var hasSource = Application.Current.Resources.TryGetValue("RepeatOffIcon", out object imageSource);
                if (hasSource)
                {
                    ViewModel.RepeatButtonSource = (string)imageSource;
                }

                break;
            }
            case 1:
            {
                // REPEAT_MODE_ONE = 1
                ImgCarousel.Loop = false;
                var hasSource = Application.Current.Resources.TryGetValue("RepeatOneIcon", out object imageSource);
                if (hasSource)
                {
                    ViewModel.RepeatButtonSource = (string)imageSource;
                }

                break;
            }
        }

        if (repeatMode != 2) return;
        {
            // REPEAT_MODE_ALL = 2
            ImgCarousel.Loop = true;
            var hasSource = Application.Current.Resources.TryGetValue("RepeatAllIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.RepeatButtonSource = (string)imageSource;
            }
        }
    }

    private async Task UpdateBackground()
    {
        if (MauiProgram.MediaService == null) return;
        MemoryStream imageDecodeStream = null;
        Song currentSong = MauiProgram.MediaService.GetCurrentlyPlaying();
        if (currentSong.ImgBlurhash == _currentBlurhash) return;
        _currentBlurhash = currentSong.ImgBlurhash;
        await Task.WhenAll(Task.Run(async () =>
        {
            // Fuck yeah get the image to move based on gyro
            //Microsoft.Maui.Devices.Sensors.Accelerometer.Start<
            string base64 = await MusicItemImage.BlurhashToBase64Async(currentSong.ImgBlurhash, 100, 100, 0.3f).ConfigureAwait(false);
            if (base64 != null)
            {
                var imageBytes = Convert.FromBase64String(base64);
                imageDecodeStream = new MemoryStream(imageBytes);
            }
        }), BackgroundImage.FadeTo(0, 1000, Easing.SinOut));
         
        ViewModel.BackgroundImageSource = ImageSource.FromStream(() => imageDecodeStream);
        await BackgroundImage.FadeTo(1, 1000, Easing.SinIn);
    }

    /// <summary>
    /// Function which can be called to update the playing song informations time, and which song is currently playing.
    /// </summary>
    /// <param name="playbackTime">Time Information derived from the media player</param>
    public async void UpdateTimestamp(PlaybackInfo playbackTime)
    {
        if (playbackTime != null)
        {
            // Check if current song is accurate
            if (ViewModel.Queue != null && ViewModel.Queue.Count > playbackTime.PlayingIndex)
            {
                if (!playbackTime.CurrentSong.Id.Equals(this._currentPlayingId))
                {
                    ImgCarousel.ScrollTo(ViewModel.Queue[playbackTime.PlayingIndex], animate: true);
                    await Task.Delay(500);
                    UpdateData(playFromIndex: playbackTime.PlayingIndex);
                    this._currentPlayingId = playbackTime.CurrentSong.Id;
                }
            }

            // Change playback icon 
            if (_isPlaying != playbackTime.IsPlaying)
            {
                UpdatePlayButton(playbackTime.IsPlaying);
                MauiProgram.MainPage.MainMiniPlayer.UpdatePlayButton(playbackTime.IsPlaying);

                _isPlaying = playbackTime.IsPlaying;
            }

            ViewModel.PlaybackValue = playbackTime.CurrentDuration.TotalMilliseconds;
            ViewModel.PlaybackMaximum = playbackTime.Duration.TotalMilliseconds;

            ViewModel.PlaybackTimeValue =
                $"{playbackTime.CurrentDuration.Minutes:D2}:{playbackTime.CurrentDuration.Seconds:D2}";
            ViewModel.PlaybackMaximumTimeValue =
                $"{playbackTime.Duration.Minutes:D2}:{playbackTime.Duration.Seconds:D2}";
        }
    }

    private async void Btn_Close_Clicked(object sender, EventArgs e)
    {
        await Close();
    }

    private async void Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;

        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        Song song = (Song)ImgCarousel.CurrentItem;

        var base64Result = await MusicItemImage.BlurhashToBase64Async(song.ImgBlurhash, 20, 20);
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
        Btn_Repeat.Opacity = BtnInOpacity;
        Btn_Repeat.Scale = BtnInSize;

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
            Btn_Repeat.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_Repeat.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_Previous_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Previous.Opacity = BtnInOpacity;
        Btn_Previous.Scale = BtnInSize;

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
            Btn_Previous.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_Previous.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
        await UpdateBackground();
    }

    private void Btn_PlayToggle_Pressed(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_PlayToggle.Opacity = BtnInOpacity;
        Btn_PlayToggle.Scale = BtnInSize;

        if (MauiProgram.MediaService.GetIsPlaying() && Application.Current != null)
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (Application.Current != null)
        {
            var hasSource = Application.Current.Resources.TryGetValue("InversePauseIcon", out object imageSource);
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
            Btn_PlayToggle.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_PlayToggle.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_Next_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Next.Opacity = BtnInOpacity;
        Btn_Next.Scale = BtnInSize;
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
            Btn_Next.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_Next.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
        await UpdateBackground();
    }

    private void Btn_Shuffle_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_Shuffle.Opacity = BtnInOpacity;
        Btn_Shuffle.Scale = BtnInSize;
    }

    private async void Btn_Shuffle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_Shuffle.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_Shuffle.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
    }

    private async void Btn_FavToggle_Pressed(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = BtnInOpacity;
        Btn_FavToggle.Scale = BtnInSize;

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
        await Task.Run(() => MauiProgram.Server.SetIsFavourite(song.Id, state, song.ServerAddress));
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_FavToggle.FadeTo(1, BtnAnimSpeedMs, Easing.SinOut),
            Btn_FavToggle.ScaleTo(1, BtnAnimSpeedMs, Easing.SinOut));
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
        if (!_pauseTimeUpdate) return;
        long time = (long)DurationSlider.Value;
        TimeSpan passedTime = TimeSpan.FromMilliseconds(time);
        ViewModel.PlaybackTimeValue = $"{passedTime.Minutes:D2}:{passedTime.Seconds:D2}";
    }

    private void DurationSlider_DragStarted(object sender, EventArgs e)
    {
        _pauseTimeUpdate = true;
    }

    private void DurationSlider_DragCompleted(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        _pauseTimeUpdate = false;
        long position = (long)DurationSlider.Value;
        MauiProgram.MediaService.SeekToPosition(position);
        UpdateTimestamp(MauiProgram.MediaService.GetPlaybackTimeInfo());
    }

    private async void CollectionLabel_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MainPage.ShowLoadingScreen(true);
        Guid? itemId = MauiProgram.MediaService.GetCurrentlyPlaying().AlbumId;
        MauiProgram.WebView.NavigateAlbum((Guid)itemId);
        await Close();
    }

    private async void ArtistLabel_Clicked(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;
        MauiProgram.MainPage.ShowLoadingScreen(true);
        Guid? itemId = MauiProgram.MediaService.GetCurrentlyPlaying().ArtistIds.FirstOrDefault();
        MauiProgram.WebView.NavigateArtist((Guid)itemId);
        await Close();
    }
}