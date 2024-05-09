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

    public void UpdateTimestamp(PlaybackInfo? playbackTime)
    {
        if(playbackTime != null)
        {
            TimeSpan passedTime = TimeSpan.FromMilliseconds(playbackTime.currentDuration);
            TimeSpan fullTime = TimeSpan.FromTicks(playbackTime.currentSong.duration);

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
        if (App.Current == null)
        {
            return;
        }

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
            await MauiProgram.api.SetFavourite(song, song.isFavourite);
        }
    }

    public void UpdatePlayButton()
    {
        if(App.Current == null)
        {
            return;
        }

        if (MauiProgram.MediaService.GetIsPlaying())
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

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                this.TranslationY = this.TranslationY + e.TotalY;

                MauiProgram.MainPage.MainMediaController.Opacity = 1;
                MauiProgram.MainPage.MainMediaController.PositionY = MauiProgram.MainPage.MainMediaController.PositionY + e.TotalY;
                break;

            case GestureStatus.Completed:                
                if(this.TranslationY < (MauiProgram.MainPage.ContentHeight / 3) * -1)
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                        MauiProgram.MainPage.MainMediaController.TranslateTo(0, 0, 450, Easing.SinOut));
                    MauiProgram.MainPage.MainMediaController.Open(animate: false);
                }
                else
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, 0, 450, Easing.SinOut),
                        MauiProgram.MainPage.MainMediaController.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
                    MauiProgram.MainPage.MainMediaController.Close(animate: false);
                }
                break;
        }
    }

    private void MiniPlayer_Clicked(object sender, TappedEventArgs e)
    {
        MauiProgram.MainPage.MainMediaController.Open();
    }

    private void MiniPlayer_Swiped(object sender, SwipedEventArgs e)
    {

    }

    private void Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {
        MauiProgram.MediaService.TogglePlay();

        UpdatePlayButton();
        MauiProgram.MainPage.MainMediaController.UpdatePlayButton();
    }

    private void Btn_PlayToggle_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_PlayToggle.Opacity = 0.8;
    }

    private async void Btn_PlayToggle_Released(object sender, EventArgs e)
    {
        await Btn_PlayToggle.FadeTo(1, 400, Easing.SinOut);
    }

    private void Btn_FavToggle_Clicked(object sender, EventArgs e)
    {
        MauiProgram.MediaService.GetCurrentlyPlaying().isFavourite = !MauiProgram.MediaService.GetCurrentlyPlaying().isFavourite;

        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMediaController.UpdateFavouriteButton();
    }

    private void Btn_FavToggle_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = 0.8;
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Btn_FavToggle.FadeTo(1, 400, Easing.SinOut);
    }
}