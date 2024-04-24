using PortaJel_Blazor.Classes;
using System.Windows.Input;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Pages.Xaml;
using Microsoft.Maui.Platform;
using System;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Alerts;
using Jellyfin.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SkiaSharp;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using System.Diagnostics;
using Microsoft.Maui.Layouts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Internals;
using PortaJel_Blazor.Shared;
#if ANDROID
using Android;
#endif

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
    public event EventHandler? CanExecuteChanged;
    public bool isContextMenuOpen => ContextMenu.isOpen;
    private double screenHeight = 0;
    public bool IsMiniPlayerOpen => MiniPlayer.IsOpen;

    public double ContentHeight { get => AllContent.Height; private set { } }
    public double ContentWidth { get => AllContent.Width; private set { } }

    public MediaController MediaPlayer { get => this.MediaControl; private set { } }
    public MiniPlayer MiniPlayerController { get => this.MiniPlayer; private set { } }


    private ObservableCollection<Song> queue = new ObservableCollection<Song>();
    private ObservableCollection<Song> playingQueue = new ObservableCollection<Song>();
    private bool canSkipCarousel = false;
    private bool hideMidiPlayer = true;
    long lastSeekPosition = 0;

    private uint animationSpeed = 550;

    public MainPage(bool? initialize = true)
	{
        if (initialize == false)
        {
            return;
        }

        InitializeComponent();
        MauiProgram.MainPage = this;
        #if ANDROID
        btn_navnar_home.HeightRequest = 30;
        btn_navnar_home.WidthRequest = 30;

        btn_navnar_search.HeightRequest = 30;
        btn_navnar_search.WidthRequest = 30;

        btn_navnar_library.HeightRequest = 30;
        btn_navnar_library.WidthRequest = 30;

        btn_navnar_favourites.HeightRequest = 30;
        btn_navnar_favourites.WidthRequest = 30;
#endif
        // MauiProgram.webView = blazorWebView;
    }

    public async void Initialize()
    {
        if (MauiProgram.api.GetServers().Count() <= 0)
        {
            AddServerView addServerView = new();
            await MauiProgram.MainPage.PushModalAsync(addServerView, false);
            await Task.Run(() => addServerView.AwaitClose(addServerView));
            MauiProgram.firstLoginComplete = true;
        }

        double spacing = (AllContent.Width - 350) / 2;
        MediaPlayer.PositionY = AllContent.Height;

        MauiProgram.mediaService.Initalize();
        MauiProgram.WebView.NavigateHome();
    }

    public void UpdateDebugText(string updateTo)
    {
        LoadingBlockout_DebugText.Text = updateTo;
    }

    private void Bwv_BlazorWebViewInitialized(object sender, Microsoft.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
               e.WebView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
#endif
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
		// Disabled overscroll 'stretch' effect that I fucking hate.
		// CLEAR giveaway this app uses webview lolz

#if ANDROID
		//var blazorView = this.blazorWebView;
  //      if(blazorView.Handler != null && blazorView.Handler.PlatformView != null)
  //      {
  //          var platformView = (Android.Webkit.WebView)blazorView.Handler.PlatformView;
  //          platformView.OverScrollMode = Android.Views.OverScrollMode.Never;
  //      }	        
#endif
    }

    public interface IRelayCommand : ICommand
    {
        void UpdateCanExecuteState();
    }

    public void UpdateCanExecuteState()
    {
        if (CanExecuteChanged != null)
            CanExecuteChanged(this, new EventArgs());
    }
    
    #region Methods
    public async Task PushModalAsync(Page page, bool? animate = true)
    {
        if(animate == true)
        {
            await Navigation.PushModalAsync(page, true);
            return;
        }
        await Navigation.PushModalAsync(page, false);
    }

    public async Task NavigateToPlaylistEdit(Guid PlaylistId)
    {
        await Navigation.PushModalAsync(new PlaylistViewEditor(PlaylistId));
    }

    public async Task AddServerView()
    {
        AddServerView viewer = new();
        await Navigation.PushModalAsync(viewer);
    }

    public async Task NavigateToPlaylistEdit(Playlist setPlaylist)
    {
        await Navigation.PushModalAsync(new PlaylistViewEditor(setPlaylist));
    }

    public INavigation GetNavigation()
    {
        return Navigation;
    }

    public void PopStack()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            Navigation.PopModalAsync();
        }
    }

    public int StackCount()
    {
        return Navigation.ModalStack.Count;
    }

    public void SetNavbarVisibility(bool visibility)
    {
        Navbar.IsVisible = visibility;
    }

    public void CloseMusicQueue()
    {

    }

    public void OpenMusicQueue()
    {

    }

    /// <summary>
    ///  Responsible for refresing the main page of the music controller
    /// </summary>
    /// <returns></returns>
    public void RefreshPlayer()
    {
        canSkipCarousel = false;
        RefreshQueue();

        // Update the song that is currently playing
        

        if (playingQueue.Count() > 0)
        {
            if (!MiniPlayer.IsOpen)
            {
                MiniPlayer.Show();
            }
        }
        else
        { // Hides the player 
            MiniPlayer.Hide();
        }

        if (!hideMidiPlayer)
        {
            // MiniPlayer.IsEnabled = !hideMidiPlayer;
        }
    }
    public void RefreshQueue()
    {
        canSkipCarousel = false;
        int playingIndex = MauiProgram.mediaService.GetQueueIndex();
        List<Song> mediaQueue = MauiProgram.mediaService.GetQueue().ToList();

        queue = mediaQueue.ToObservableCollection();
        mediaQueue.RemoveRange(0, playingIndex);
        playingQueue = mediaQueue.ToObservableCollection();

        //MediaController_Queue_List.ItemsSource = playingQueue;

        //MainPlayer_ImgCarousel.ItemsSource = queue;
        MiniPlayer.UpdateDate(MauiProgram.mediaService.GetQueue());

        //MainPlayer_ImgCarousel.ScrollTo(playingIndex, animate: false);
        //MiniPlayer_ImgCarousel.ScrollTo(playingIndex, animate: false);
    }

    public void ShowMusicController()
    {

    }
    public Task AwaitContextMenuClose()
    {
        while (ContextMenu.isOpen)
        {

        }
        return Task.CompletedTask;
    }
    public Task AwaitContextMenuOpen()
    {
        while (!ContextMenu.isOpen)
        {

        }
        return Task.CompletedTask;
    }
    public async void ShowLoadingScreen(bool value)
    {
        LoadingBlockout.IsVisible = value;
        if (value == true)
        { // If we're already visible, do nothin'
            LoadingBlockout.InputTransparent = false;
            LoadingBlockout.Opacity = 1; 
        }
        else
        {
            LoadingBlockout.InputTransparent = true;
            LoadingBlockout.Opacity = 1; // make fully visible
            await LoadingBlockout.FadeTo(0, 500, Easing.SinOut);
        }
    }
    public async void UpdateKeyboardLocation()
    {
        var keyboardHeight = AllContent.Height - AllContent.Bounds.Bottom + AllContent.Bounds.Top;

        var toast = Toast.Make($"Keyboard height {keyboardHeight}", ToastDuration.Long, 14);
        await toast.Show();
    }
    private bool mediaCntrollerSliderDragging = false;
    public void UpdatePlaystate(long duration, long position)
    {

    }
    private string ConvertToTimeFormat(long milliseconds)
    {
        // Convert milliseconds to seconds
        long totalSeconds = milliseconds / 1000;

        // Calculate minutes and seconds
        long minutes = totalSeconds / 60;
        long seconds = totalSeconds % 60;

        // Format the time as mm:ss
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    /// <summary>
    /// Skips to the next song
    /// </summary>
    #endregion

    #region ContextMenuMethods
    public bool OpenContextMenu(BaseMusicItem baseMusicItem, int imgResolution, string? setBlurBase64 = null)
    {
        ContextMenu.UpdateData(baseMusicItem, blurBase64: setBlurBase64, opacity: 0);
        ContextMenu.Show();
        return true;
    }
    public void CloseContextMenu()
    {
        ContextMenu.Close();
    }
    #endregion

    #region MediaController

    /// <summary>
    /// Player Previous Button Clicked
    /// Interaction method called when the Previous Song button is pressed in the Media Controller
    /// </summary>
    bool isSkipping = false;
    private void MediaCntroller_Player_Previous_btn_Clicked(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Player Next Button Clicked
    /// Interaction method called when the Next Song button is pressed in the Media Controller
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void MediaCntroller_Player_Next_btn_Clicked(object sender, EventArgs e)
    {

    }

    private void MediaCntroller_SkipToIndex(int index)
    {

    }

    private void MainPlayer_ImgCarousel_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        CarouselView? carousel = sender as CarouselView;
        if (carousel != null)
        {
            if (carousel.IsDragging)
            {
                canSkipCarousel = true;
            }
        }
    }

    private void MiniPlayer_ImgCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        if (canSkipCarousel)
        {
            if (e.CurrentPosition > e.PreviousPosition)
            { // Next song is requested
                MauiProgram.mediaService.NextTrack();
            }
            else
            { // Prev song is requested
                MauiProgram.mediaService.PreviousTrack();
            }
        }
    }
    private void MainPlayer_ImgCarousel_ScrollToRequested(object sender, ScrollToRequestEventArgs e)
    {
        MauiProgram.mediaService.SeekToIndex(e.Index);
    }
    #endregion

    #region Interactions
    private bool homeBtnReleased = true;
    private void btn_navnar_home_Pressed(object sender, EventArgs e)
    {
        homeBtnReleased = false;
        btn_navnar_home.Opacity = 0;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    }
    private void btn_navnar_home_Released(object sender, EventArgs e)
    {
        if(homeBtnReleased == false)
        {
            btn_navnar_home_click(sender, e);
        }
    }
    private async void btn_navnar_home_click(object sender, EventArgs e)
    {
        homeBtnReleased = true;
        ShowLoadingScreen(true);

        await Task.WhenAll<bool>
        (
            btn_navnar_home.FadeTo(1, 250)
        );
        MauiProgram.WebView.NavigateHome();
    }
    private async void btn_navnar_search_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_search.Scale = 0.6;
        btn_navnar_search.Opacity = 0;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await Task.WhenAll<bool>
        (
            btn_navnar_search.FadeTo(1, 250),
            btn_navnar_search.ScaleTo(1, 250)
        );
        MauiProgram.WebView.NavigateSearch();
    }
    private async void btn_navnar_library_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_library.Scale = 0.6;
        btn_navnar_library.Opacity = 0;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await Task.WhenAny<bool>
        (
            btn_navnar_library.FadeTo(1, 250),
            btn_navnar_library.ScaleTo(1, 250)
        );
        MauiProgram.WebView.NavigateLibrary();
    }
    private async void btn_navnar_favourite_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_favourites.Scale = 0.6;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await Task.WhenAny<bool>
        (
            btn_navnar_favourites.FadeTo(1, 250),
            btn_navnar_favourites.ScaleTo(1, 250)
        );
        MauiProgram.WebView.NavigateFavourites();
    }
    private void MediaController_Btn_ContextMenu(object sender, EventArgs args)
    {
        // TODO: Update this to show current item
        // OpenContextMenu();
    }
    private double imgDragDistance = 0;

    void MiniPlayer_Swiped(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Left:
                MediaCntroller_Player_Previous_btn_Clicked(sender, e);
                break;
            case SwipeDirection.Right:
                MediaCntroller_Player_Next_btn_Clicked(sender, e);
                break;
            case SwipeDirection.Up:
                ShowMusicController();
                break;
            case SwipeDirection.Down:
                // Handle the swipe
                break;
        }
    }
    double distance = 0;
    private void Navbar_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
#if !WINDOWS
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                //double h = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
                //MediaController.IsVisible = true;

                //Player.TranslationY = Player.TranslationY + e.TotalY;
                //MediaController.TranslationY = Player.TranslationY + MediaController.Height - 50;

                //double vis = 0;
                //if (Player.TranslationY < 0)
                //{
                //    double check = ((Player.TranslationY * -1) + 60) / MediaController.Height;
                //    if (check > 0)
                //    {
                //        vis = check;
                //    }
                //}

                //distance = Player.TranslationY;
                //MediaController.Opacity = vis;

                //Player_Txt_Title.Text = distance.ToString();
                break;

            case GestureStatus.Completed:
                if (distance < 0) { distance *= -1; }
                uint speed = (uint)distance;

                screenHeight = AllContent.Height;
                if (distance > screenHeight / 3)
                {
                    ShowMusicController();
                }
                else
                {
                    MauiProgram.MainPage.MediaPlayer.Close();
                }
                break;
        }
#endif
    }
    private async void Queue_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
        #if !WINDOWS
        //switch (e.StatusType)
        //{
        //    case GestureStatus.Running:
        //        double newLocation = ContextMenu.TranslationY + e.TotalY;
        //        if(newLocation >= 0)
        //        {
        //            ContextMenu.TranslationY = newLocation;
        //        }
        //        break;

        //    case GestureStatus.Completed:
        //        await ContextMenu.TranslateTo(0, 0, animationSpeed, Easing.SinOut);
        //        break;
        //}
        #endif
    }
    private void MediaCntroller_Slider_DragStarted(object sender, EventArgs e)
    {
        mediaCntrollerSliderDragging = true;
        // MediaCntroller_Slider_PositionTxt.Text = ConvertToTimeFormat(position);
    }
    private void MediaCntroller_Slider_DragCompleted(object sender, EventArgs e)
    {
        mediaCntrollerSliderDragging = false;
        MauiProgram.mediaService.SeekToPosition(lastSeekPosition);
    }
    private void MediaCntroller_Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        long value = (long)e.NewValue;
        lastSeekPosition = value;
        // MediaCntroller_Slider_PositionTxt.Text = ConvertToTimeFormat(value);
    }

    #endregion
}
