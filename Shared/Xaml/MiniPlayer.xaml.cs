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

    public async void UpdateTimestamp(PlaybackTimeInfo? playbackTime)
    {
        if(playbackTime != null)
        {
            float percentage = (float)playbackTime.currentDuration / (float)playbackTime.fullDuration;

            if (playbackTime.currentTrackGuid == lastUpdateTrackId)
            {
                lastUpdateTrackId = playbackTime.currentTrackGuid;
                await Progress.ProgressTo(percentage, 1000, Easing.Linear);
            }
            else
            {
                Progress.CancelAnimations();

                lastUpdateTrackId = playbackTime.currentTrackGuid;
                Progress.Progress = percentage;
            }
        }
    }

    public async void UpdateData(Song[] songs, int? playFromIndex = 0)
    {
        ViewModel.Queue = songs.ToObservableCollection();
        Song viewModelItem = (Song)ImgCarousel.CurrentItem;
        
        // Update Carousel 
        if(playFromIndex != null)
        {
            ImgCarousel.ScrollTo(songs[(int)playFromIndex], animate: false);
        }

        // Update Play Button
        UpdatePlayButton();

        // Update Favourite Button
        UpdateFavouriteButton();

        // Update time tracking
        PlaybackTimeInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
    }

    public async void UpdateFavouriteButton(bool? syncToServer = true)
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
            await MauiProgram.servers[0].FavouriteItem(song.id, song.isFavourite);
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
        Btn_PlayToggle.Opacity = 0;
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
        Btn_FavToggle.Opacity = 0;
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Btn_FavToggle.FadeTo(1, 400, Easing.SinOut);
    }
}