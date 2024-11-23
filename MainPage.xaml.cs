using PortaJel_Blazor.Classes;
using System.Windows.Input;
using Microsoft.Maui.Platform;
using System;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
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
using Microsoft.Maui.Animations;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Pages;
using PortaJel_Blazor.Pages.Connection;
using PortaJel_Blazor.Shared.Xaml;

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

    public MediaController MainMediaController { get => this.MediaControl; private set { } }
    public MediaQueue MainMediaQueue { get => this.Queue; private set { } }
    public MiniPlayer MainMiniPlayer { get => this.MiniPlayer; private set { } }
    public ContextMenu MainContextMenu { get => this.ContextMenu; private set { } }

    public double HeaderHeightValue { get => MauiProgram.SystemHeaderHeight; private set { } }

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
        
        // Authenticate and begin sync
        _ = Task.Run(() =>
            MauiProgram.Server.AuthenticateAsync()).ContinueWith(
            _ => new Thread(MauiProgram.Server.BeginSyncAsync().Wait),
            TaskContinuationOptions.ExecuteSynchronously
        );
        // TODO: Cancellation Token
        // MauiProgram.webView = blazorWebView;
    }

    public void UpdateDebugText(string updateTo)
    {
        LoadingBlockout_DebugText.Text = updateTo;
    }

    protected override void OnHandlerChanged()
    {
        // Disabled overscroll 'stretch' effect that I fucking hate.
        // I'd actually be okay enabling this but only once the headers become native elements
        #if ANDROID
        var blazorview = this.blazorWebView;
        if (blazorview.Handler != null && blazorview.Handler.PlatformView != null)
        {
            var platformview = (Android.Webkit.WebView)blazorview.Handler.PlatformView;
            platformview.OverScrollMode = Android.Views.OverScrollMode.Never;
        }
        #endif
        base.OnHandlerChanged();
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
        if (animate == true)
        {
            await Navigation.PushModalAsync(page, true);
            return;
        }
        await Navigation.PushModalAsync(page, false);
    }
    public async void DeselectMenuItems()
    {
        await Task.WhenAll(
            btn_navbar_home_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_search_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_lib_iconframe.BackgroundColorTo(Colors.Transparent)
        );
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

    public async void ShowStatusIndicator(string message)
    {
        //if (statusIndicator == null) return;
#if !WINDOWS
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

        var toast = Toast.Make(message, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
#endif
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


    /// <summary>
    ///  Responsible for refresing the main page of the music controller
    /// </summary>
    /// <returns></returns>
    public void RefreshPlayer()
    {
        if (MauiProgram.MediaService == null) return;
        canSkipCarousel = false;
        RefreshQueue();

        // Update the song that is currently 
        if (MauiProgram.MainPage.MainMiniPlayer.SongCount > 0)
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
    }
    public void RefreshQueue()
    {
        if (MauiProgram.MediaService == null) return;

        canSkipCarousel = false;
        int playingIndex = MauiProgram.MediaService.GetQueueIndex();

        SongGroupCollection songGroupCollection = MauiProgram.MediaService.GetQueue();

        MiniPlayer.UpdateData(songGroupCollection.AllSongs.ToArray(), playingIndex);
        MainMediaController.UpdateData(songGroupCollection, playingIndex);
        MainMediaQueue.UpdateData(songGroupCollection, playingIndex);
    }

    public async Task<bool> AwaitContextMenuClose()
    {
        while (ContextMenu.isOpen)
        {
            await Task.Delay(100);
        }
        return true;
    }
    public async Task<bool> AwaitContextMenuOpen()
    {
        while (!ContextMenu.isOpen)
        {
            await Task.Delay(100);
        }
        return true;
    }
    public async void ShowLoadingScreen(bool value)
    {
        try
        {
            if (value)
            {
                // If we're already visible, do nothin'
                LoadingBlockout.IsVisible = true;
                LoadingBlockout.InputTransparent = false;
                LoadingBlockout.Opacity = 1;
            }
            else if (LoadingBlockout.Opacity >= 1)
            {
                LoadingBlockout.InputTransparent = true;
                LoadingBlockout.Opacity = 1; // make fully visible

                await LoadingBlockout.FadeTo(0, 500, Easing.SinOut);
                LoadingBlockout.IsVisible = false;
            }
        }
        catch
        {
            LoadingBlockout.Opacity = 0; // make fully visible
        }
    }
    
    public async void UpdateKeyboardLocation()
    {
        var keyboardHeight = AllContent.Height - AllContent.Bounds.Bottom + AllContent.Bounds.Top;

        var toast = Toast.Make($"Keyboard height {keyboardHeight}", ToastDuration.Long, 14);
        await toast.Show();
    }
    
    private bool mediaCntrollerSliderDragging = false;
    
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
                MauiProgram.MediaService.NextTrack();
            }
            else
            { // Prev song is requested
                MauiProgram.MediaService.PreviousTrack();
            }
        }
    }
    private void MainPlayer_ImgCarousel_ScrollToRequested(object sender, ScrollToRequestEventArgs e)
    {
        MauiProgram.MediaService.SeekToIndex(e.Index);
    }
    #endregion

    #region Interactions
    private bool homeBtnReleased = true;
    private async void btn_navnar_home_Released(object sender, TappedEventArgs e)
    {
        ShowLoadingScreen(true);
        await Task.WhenAll(
            btn_navbar_home_iconframe.BackgroundColorTo(Colors.Gray),
            btn_navbar_search_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_lib_iconframe.BackgroundColorTo(Colors.Transparent)
        );
        MauiProgram.WebView.NavigateHome();
    }
    
    private async void btn_navnar_search_released(object sender, TappedEventArgs e)
    {
        ShowLoadingScreen(true);
        await Task.WhenAll(
            btn_navbar_home_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_search_iconframe.BackgroundColorTo(Colors.Gray),
            btn_navbar_lib_iconframe.BackgroundColorTo(Colors.Transparent)
        );
        MauiProgram.WebView.NavigateSearch();
    }
    
    private async void btn_navnar_library_released(object sender, TappedEventArgs e)
    {
        ShowLoadingScreen(true);
        await Task.WhenAll(
            btn_navbar_home_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_search_iconframe.BackgroundColorTo(Colors.Transparent),
            btn_navbar_lib_iconframe.BackgroundColorTo(Colors.Gray)
        );
        MauiProgram.WebView.NavigateLibrary();
    }
    
    private async void MiniPlayer_Swiped(object sender, SwipedEventArgs e)
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
                await MainMediaController.Open();
                break;
            case SwipeDirection.Down:
                // Handle the swipe
                break;
        }
    }
    double distance = 0;
    private async void Navbar_PinchUpdated(object sender, PanUpdatedEventArgs e)
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
                    await MainMediaController.Open();
                }
                else
                {
                    await MauiProgram.MainPage.MainMediaController.Close();
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
        MauiProgram.MediaService.SeekToPosition(lastSeekPosition);
    }
    private void MediaCntroller_Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        long value = (long)e.NewValue;
        lastSeekPosition = value;
        // MediaCntroller_Slider_PositionTxt.Text = ConvertToTimeFormat(value);
    }

    #endregion
}
