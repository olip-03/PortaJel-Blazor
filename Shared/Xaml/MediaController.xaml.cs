using BlazorAnimate;
using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System;
using System.Diagnostics;
namespace PortaJel_Blazor.Shared;

public partial class MediaController : ContentView
{
    public MediaControllerViewModel ViewModel { get; set; } = new();

    public bool IsOpen { get; private set; } = false;
    public bool IsQueueOpen { get; private set; } = false;

    public double PositionX { get => TranslationX; set => TranslationX = value; }
    public double PositionY { get => TranslationY; set => TranslationY = value; }

    public MediaController()
	{
		InitializeComponent();
        this.BindingContext = ViewModel;
	}

    public async void Open(bool? animate = true)
    {
        UpdatePlayButton();
        UpdateFavouriteButton();

        Opacity = 1;
        TranslationY = MauiProgram.MainPage.ContentHeight;
        IsOpen = true;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut));

                
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = MauiProgram.MainPage.ContentHeight * -1;
            TranslationY = 0; 
        }
    }

    public async void Close(bool? animate = true)
    {
        IsOpen = false;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, 0, 450, Easing.SinOut),
                this.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = 0;
            TranslationY = MauiProgram.MainPage.ContentHeight;
        }

    }

    public void UpdateData(Song[] songs, int? playFromIndex = 0)
    {
        ViewModel.Queue = songs.ToObservableCollection();
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

    public void UpdatePlayButton()
    {
        if (App.Current == null)
        {
            return;
        }

        if (MauiProgram.MediaService.GetIsPlaying())
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

    public void UpdateTimestamp(PlaybackTimeInfo? playbackTime)
    {
        if (playbackTime != null)
        {
            float percentage = (float)playbackTime.currentDuration / (float)playbackTime.fullDuration;

            ViewModel.PlaybackValue = (double)playbackTime.currentDuration;
            ViewModel.PlaybackMaximum = (double)playbackTime.fullDuration;

            TimeSpan passedTime = TimeSpan.FromMilliseconds(playbackTime.currentDuration);
            TimeSpan fullTime = TimeSpan.FromMilliseconds(playbackTime.fullDuration);

            ViewModel.PlaybackTimeValue = string.Format("{0:D2}:{1:D2}", passedTime.Minutes, passedTime.Seconds);
            ViewModel.PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", fullTime.Minutes, fullTime.Seconds);
        }
    }

    private void Player_Btn_Close_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private async void Player_Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
        string base64 = await song.image.BlurhashToBase64Async(200, 200);
        MauiProgram.MainPage.OpenContextMenu(song, 250, base64);
    }

    private void Player_Btn_Repeat_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Previous_Clicked(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        MauiProgram.MediaService.PreviousTrack();
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

    private void Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {
        MauiProgram.MediaService.TogglePlay();

        UpdatePlayButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdatePlayButton();
    }

    private void Player_Btn_Next_Clicked(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        MauiProgram.MediaService.NextTrack();
    }

    private void Player_Btn_Shuffle_Clicked(object sender, EventArgs e)
    {

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

    private void Btn_FavToggle_Clicked(object sender, EventArgs e)
    {
        MauiProgram.MediaService.GetCurrentlyPlaying().isFavourite = !MauiProgram.MediaService.GetCurrentlyPlaying().isFavourite;
        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdateFavouriteButton();
    }

    private void Player_Btn_ShowQueue_Clicked(object sender, EventArgs e)
    {
        
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

    private void Player_DurationSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {

    }

    private void Player_DurationSlider_DragStarted(object sender, EventArgs e)
    {

    }

    private void Player_DurationSlider_DragCompleted(object sender, EventArgs e)
    {

    }

    private void Queue_Btn_Close_Clicked(object sender, EventArgs e)
    {

    }

    private void Queue_Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {

    }
}