using BlazorAnimate;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls;
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

    private double btnInOpacity = 0.5;
    private double btnInSize = 0.8;
    private uint btnAnimSpeedMs = 400;

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
    public void UpdateData(Song[]? songs = null, int? playFromIndex = 0)
    { 
        if(songs != null)
        {
            ViewModel.Queue = songs.ToObservableCollection();
            if (playFromIndex != null)
            {
                ImgCarousel.ScrollTo(songs[(int)playFromIndex], animate: false);
            }
        }
        else if(ViewModel.Queue != null && ViewModel.Queue.Count() > playFromIndex)
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

    public async void UpdateFavouriteButton(bool? syncToServer = false)
    {
        if (App.Current == null || ViewModel.Queue == null)
        {
            return;
        }

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
                await MauiProgram.api.SetFavourite(song, song.isFavourite);
            });
        }
    }

    public void UpdateTimestamp(PlaybackTimeInfo? playbackTime)
    {
        if (playbackTime != null)
        {
            float percentage = (float)playbackTime.currentDuration / (float)playbackTime.fullDuration;

            if(playbackTime.fullDuration > 0)
            {
                ViewModel.PlaybackValue = (double)playbackTime.currentDuration;
                ViewModel.PlaybackMaximum = (double)playbackTime.fullDuration;

                TimeSpan passedTime = TimeSpan.FromMilliseconds(playbackTime.currentDuration);
                TimeSpan fullTime = TimeSpan.FromMilliseconds(playbackTime.fullDuration);

                ViewModel.PlaybackTimeValue = string.Format("{0:D2}:{1:D2}", passedTime.Minutes, passedTime.Seconds);
                ViewModel.PlaybackMaximumTimeValue = string.Format("{0:D2}:{1:D2}", fullTime.Minutes, fullTime.Seconds);
            }
        }
    }

    private void Btn_Close_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private async void Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        Song song = MauiProgram.MediaService.GetCurrentlyPlaying();
        string base64 = await song.image.BlurhashToBase64Async(200, 200);
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
        MauiProgram.MediaService.PreviousTrack();

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying();
        ImgCarousel.ScrollTo(item: scrollTo, animate: true);

        int currentIndex = MauiProgram.MediaService.GetQueueIndex();
        MauiProgram.MainPage.MainMiniPlayer.UpdateData(playFromIndex: currentIndex);

        await Task.WhenAll(
            Btn_Previous.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Previous.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
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
        else if(App.Current != null)
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
        MauiProgram.MediaService.NextTrack();

        Song scrollTo = MauiProgram.MediaService.GetCurrentlyPlaying();
        ImgCarousel.ScrollTo(item: scrollTo, animate: true);

        int currentIndex = MauiProgram.MediaService.GetQueueIndex();
        MauiProgram.MainPage.MainMiniPlayer.UpdateData(playFromIndex: currentIndex);

        await Task.WhenAll(
            Btn_Next.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_Next.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
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

    private void Btn_FavToggle_Pressed(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        Btn_FavToggle.Opacity = btnInOpacity;
        Btn_FavToggle.Scale = btnInSize;

        if (ViewModel.Queue == null)
        {
            return;
        }

        int queueIndex = MauiProgram.MediaService.GetQueueIndex();
        ViewModel.Queue[queueIndex].isFavourite = !ViewModel.Queue[queueIndex].isFavourite;
        
        UpdateFavouriteButton();
        MauiProgram.MainPage.MainMiniPlayer.UpdateFavouriteButton(syncToServer: true);
    }

    private async void Btn_FavToggle_Released(object sender, EventArgs e)
    {
        await Task.WhenAll(
            Btn_FavToggle.FadeTo(1, btnAnimSpeedMs, Easing.SinOut),
            Btn_FavToggle.ScaleTo(1, btnAnimSpeedMs, Easing.SinOut));
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