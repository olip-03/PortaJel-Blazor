using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Diagnostics;
using System.Security.Cryptography;

namespace PortaJel_Blazor.Shared;

public partial class MiniPlayer : ContentView
{
    public MiniPlayerViewModel ViewModel { get; set; } = new();
    public double PositionX { get => TranslationX; private set { } }
    public double PositionY { get => PositionY; private set { } }
    public bool IsOpen { get; private set; } = false;
    private Guid lastUpdateTrackId = Guid.Empty;
    private Guid currentPlayingId = Guid.Empty;

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
    }

    public async void UpdateTimestamp(PlaybackInfo? playbackTime)
    {
        if(playbackTime != null)
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

            TimeSpan passedTime = TimeSpan.FromMilliseconds(playbackTime.currentDuration);
            TimeSpan fullTime = TimeSpan.FromMilliseconds(playbackTime.currentSong.duration);

            float percentage = (float)passedTime.Ticks / (float)fullTime.Ticks;

            if (playbackTime.currentSong.id == lastUpdateTrackId)
            {
                lastUpdateTrackId = playbackTime.currentSong.id;
                Progress.Progress = percentage;
            }
            else
            {
                Progress.CancelAnimations();

                lastUpdateTrackId = playbackTime.currentSong.id;
                Progress.Progress = percentage;
            }
        }
    }

    public void UpdateData(Song[]? songs = null, int? playFromIndex = 0)
    {
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

        // Update Play Button
        UpdatePlayButton();

        // Update Favourite Button
        UpdateFavouriteButton();

        // Update time tracking
        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
    }

    public async void UpdateFavouriteButton(bool? syncToServer = false)
    {
        if (App.Current == null) return;
        if(MauiProgram.MediaService == null) return;

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
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
            await MauiProgram.api.SetFavourite(song.id, song.serverAddress, song.isFavourite);
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
    public void InsertIntoQueue(Song song)
    {
        // Insert item into viewmodel
        if (MauiProgram.MediaService == null) return;
        if (ViewModel.Queue == null) ViewModel.Queue = new();

        try
        {
            SongGroupCollection sgc = MauiProgram.MediaService.GetQueue();
            int insertInto = sgc.QueueStartIndex + (sgc.QueueCount - 1);
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
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = btnInOpacity;
        Btn_FavToggle.Scale = btnInSize;

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
        bool state = !song.isFavourite;

        MauiProgram.MediaService.GetCurrentlyPlaying().isFavourite = state;
        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMediaController.UpdateFavouriteButton();

        await Task.Run(() => MauiProgram.api.SetFavourite(song.id, song.serverAddress, state));
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_FavToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_FavToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
    }
}