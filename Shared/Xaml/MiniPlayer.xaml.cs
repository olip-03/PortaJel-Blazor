using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.ViewModels;

namespace PortaJel_Blazor.Shared.Xaml;

public partial class MiniPlayer : ContentView
{
    public MiniPlayerViewModel ViewModel { get; set; } = new();
    public double PositionX { get => TranslationX; private set { } }
    public double PositionY { get => PositionY; private set { } }
    public int SongCount { get => ViewModel.Queue.Count; private set { } }
    public bool IsOpen { get; private set; } = false;
    private Guid lastUpdateTrackId = Guid.Empty;
    private Guid currentPlayingId = Guid.Empty;
    private string currentBlurhash = string.Empty;

    private double btnInOpacity = 0.5;
    private double btnInSize = 0.8;
    private uint btnAnimSpeedMs = 400;

    public MiniPlayer()
	{
		InitializeComponent(); 
        this.BindingContext = ViewModel;
    }

    public async void Show()
    {
        this.Opacity = 0;
        this.TranslationY = 120;
        IsVisible = true;
        IsOpen = true;

        // Update Play Button
        UpdatePlayButton();

        // Update Favourite Button
        UpdateFavouriteButton();

        await Task.WhenAny(
                this.FadeTo(1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut));
    }

    public async void Hide()
    {
        IsOpen = false;
        await Task.WhenAny(
            this.FadeTo(0, 450, Easing.SinOut),
            this.TranslateTo(0, 120, 450, Easing.SinOut));
        IsVisible = false;
    }

    public async void UpdateTimestamp(PlaybackInfo? playbackTime)
    {
        if(playbackTime != null)
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

            TimeSpan passedTime = playbackTime.CurrentDuration;
            TimeSpan fullTime = playbackTime.Duration;

            float percentage = (float)passedTime.Ticks / (float)fullTime.Ticks;

            if (playbackTime.CurrentSong.Id == lastUpdateTrackId)
            {
                lastUpdateTrackId = playbackTime.CurrentSong.Id;
                Progress.Progress = percentage;
            }
            else
            {
                Progress.CancelAnimations();

                lastUpdateTrackId = playbackTime.CurrentSong.Id;
                Progress.Progress = percentage;
            }
        }
    }

    public async void UpdateData(Song[]? songs = null, int? playFromIndex = 0)
    {
        if (MauiProgram.MediaService == null) return;
        if (songs != null)
        {
            ViewModel.Queue = songs.ToObservableCollection();
            if (playFromIndex != null)
            {
                ImgCarousel.ScrollTo(songs[(int)playFromIndex], animate: false);
            }
        }
        else if (ViewModel.Queue != null && ViewModel.Queue.Count() > playFromIndex)
        {
            if (playFromIndex != null)
            {
                ImgCarousel.ScrollTo(ViewModel.Queue[(int)playFromIndex], animate: false);
            }
        }

        // Update Elements
        UpdatePlayButton();
        UpdateFavouriteButton();
        await UpdateBackground();

        // Update time tracking
        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
    }

    public async void UpdateFavouriteButton(bool? syncToServer = false)
    {
        if (App.Current == null) return;
        if(MauiProgram.MediaService == null) return;

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
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
            await MauiProgram.api.SetFavourite(song.Id, song.ServerAddress, song.IsFavourite);
        }
    }

    public void UpdatePlayButton(bool? isPlaying = null)
    {
        if (App.Current == null) return;
        if (MauiProgram.MediaService == null) return;

        if (isPlaying == true)
        {
            var hasSource = App.Current.Resources.TryGetValue("PauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else if (MauiProgram.MediaService.GetIsPlaying())
        {
            var hasSource = App.Current.Resources.TryGetValue("PauseIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
            }
        }
        else
        {
            var hasSource = App.Current.Resources.TryGetValue("PlayIcon", out object imageSource);
            if (hasSource)
            {
                ViewModel.PlayButtonSource = (string)imageSource;
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

        if (!IsOpen)
        {
            Show();
        }
    }

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!IsOpen)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                this.TranslationY = this.TranslationY + e.TotalY;
                MauiProgram.MainPage.MainMediaController.IsVisible = true;
                MauiProgram.MainPage.MainMediaController.Opacity = 1;
                MauiProgram.MainPage.MainMediaController.PositionY = MauiProgram.MainPage.MainMediaController.PositionY + e.TotalY;
                break;

            case GestureStatus.Completed:                
                if(this.TranslationY < (MauiProgram.MainPage.ContentHeight / 3) * -1)
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                        MauiProgram.MainPage.MainMediaController.TranslateTo(0, 0, 450, Easing.SinOut));
                    await MauiProgram.MainPage.MainMediaController.Open(animate: false);
                }
                else
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, 0, 450, Easing.SinOut),
                        MauiProgram.MainPage.MainMediaController.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
                    await MauiProgram.MainPage.MainMediaController.Close(animate: false);
                }
                break;
        }
    }

    private async void MiniPlayer_Clicked(object sender, TappedEventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await MauiProgram.MainPage.MainMediaController.Open();
    }

    private void MiniPlayer_Swiped(object sender, SwipedEventArgs e)
    {

    }

    private void Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {

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

        MauiProgram.MediaService.TogglePlay();

        UpdatePlayButton();
        MauiProgram.MainPage.MainMediaController.UpdatePlayButton();
        await Task.WhenAll(
            Btn_PlayToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_PlayToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }

    private void Btn_FavToggle_Clicked(object sender, EventArgs e)
    {
        
    }

    private async void Btn_FavToggle_Pressed(object sender, EventArgs e)
    {
        if (MauiProgram.MediaService == null) return;

        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = btnInOpacity;
        Btn_FavToggle.Scale = btnInSize;

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
        bool state = !song.IsFavourite;

        MauiProgram.MediaService.GetCurrentlyPlaying().SetIsFavourite(state);
        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMediaController.UpdateFavouriteButton();

        await Task.Run(() => MauiProgram.api.SetFavourite(song.Id, song.ServerAddress, state));
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_FavToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_FavToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }
}