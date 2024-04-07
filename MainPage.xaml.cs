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
#if ANDROID
using Android;
#endif

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
    public event EventHandler? CanExecuteChanged;
    private bool isClosing = false;
    private double screenHeight = 0;
    private bool musicControlsFirstOpen = true;
    private bool waitingForPageLoad = false;

    private ObservableCollection<Song> queue = new ObservableCollection<Song>();
    private ObservableCollection<Song> queueNextUp = new ObservableCollection<Song>();
    private ObservableCollection<Song> queueCongregate = new ObservableCollection<Song>();
    private Song? currentlyPlaying = Song.Empty;
    private bool hideMidiPlayer = true;
    long lastSeekPosition = 0;

    private uint animationSpeed = 550;

    public MainPage(bool? initalize = true)
	{
        if (initalize == false)
        {
            return;
        }

        InitializeComponent();
        MauiProgram.mainPage = this;
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
            await MauiProgram.mainPage.PushModalAsync(addServerView, false);
            await Task.Run(() => addServerView.AwaitClose(addServerView));
            MauiProgram.firstLoginComplete = true;
        }

        double spacing = (AllContent.Width - 350) / 2;

        ShowNavbar();
        MauiProgram.mediaService.Initalize();
        MauiProgram.webView.NavigateHome();
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
		var blazorView = this.blazorWebView;
        if(blazorView.Handler != null && blazorView.Handler.PlatformView != null)
        {
            var platformView = (Android.Webkit.WebView)blazorView.Handler.PlatformView;
            platformView.OverScrollMode = Android.Views.OverScrollMode.Never;
        }	        
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

    public void ShowNavbar()
    {
        Navbar.IsVisible = true;
        Navbar.Opacity = 1;
    }

    public async void CloseMusicQueue()
    {
        screenHeight = AllContent.Height;

        MauiProgram.MusicPlayerIsQueueOpen = false;
        MediaController_Player.IsVisible = true;

        //MediaController_Player_PlayingFromInfo.IsVisible = MauiProgram.mediaService.nextUpIsAvaliable;

        MediaController_Queue.TranslationY = 0;
        await MediaController_Queue.TranslateTo(0, screenHeight, animationSpeed, Easing.SinIn);
    }

    public async void OpenMusicQueue()
    {
        screenHeight = AllContent.Height;

        MauiProgram.MusicPlayerIsQueueOpen = true;
        MediaController_Queue.IsVisible = true;
        
        MediaController_Queue.TranslationY = screenHeight;
        await MediaController_Queue.TranslateTo(0, 0, animationSpeed, Easing.SinOut);

        MediaController_Player.IsVisible = false;
    }

    /// <summary>
    ///  Responsible for refresing the main page of the music controller
    /// </summary>
    /// <returns></returns>
    public async void RefreshPlayer()
    {
        queue = MauiProgram.mediaService.songQueue.GetQueue().ToObservableCollection();
        queueNextUp = MauiProgram.mediaService.nextUpQueue.GetQueue().ToObservableCollection();
        queueCongregate = [.. queue, .. queueNextUp];

        MediaController_Queue_List.ItemsSource = queueCongregate;
        MainPlayer_ImgCarousel.ItemsSource = queueCongregate;
        MiniPlayer_ImgCarousel.ItemsSource = queueCongregate;

        if (!MauiProgram.MiniPlayerIsOpen)
        {
            MiniPlayer.TranslationY = 120;
        }

        async void PlayAnimations()
        {
            MiniPlayer.TranslationY = 120;

            await Task.WhenAny(
                    MiniPlayer.FadeTo(1, animationSpeed, Easing.SinOut),
                    MiniPlayer.TranslateTo(0, 0, animationSpeed, Easing.SinOut));
            hideMidiPlayer = false;
            MauiProgram.MiniPlayerIsOpen = true;
        }

        // Update the song that is currently playing
        if (MauiProgram.mediaService.songQueue.Count() > 0)
        {
            if (!MauiProgram.MiniPlayerIsOpen)
            {
                PlayAnimations();
            }
        }
        else if(MauiProgram.mediaService.nextUpQueue.Count() > 0)
        {
            if (!MauiProgram.MiniPlayerIsOpen)
            {
                PlayAnimations();
            }
        }
        else
        {
            await Task.WhenAny(
                MiniPlayer.FadeTo(0, animationSpeed, Easing.SinOut),
                MiniPlayer.TranslateTo(0, 120, animationSpeed, Easing.SinOut));

            MauiProgram.MiniPlayerIsOpen = false;
            hideMidiPlayer = true;
        }

        if (!hideMidiPlayer)
        {
            MiniPlayer.IsEnabled = !hideMidiPlayer;
        }
    }

    public async void RefreshPlayState(bool? animate = true)
    {
        Player_Btn_PlayToggle.Opacity = 0;
        MediaController_Player_Play_btn.Opacity = 0;
        if (MauiProgram.mediaService.isPlaying)
        {
            MediaController_Player_Play_btn.Source = "pause.png";
            Player_Btn_PlayToggle.Source = "pause.png";
        }
        else
        {
            MediaController_Player_Play_btn.Source = "play.png";
            Player_Btn_PlayToggle.Source = "play.png";
        }
        if (animate == true)
        {
            await Task.WhenAll(
                Player_Btn_PlayToggle.FadeTo(1, 500, Easing.SinOut),
                MediaController_Player_Play_btn.FadeTo(1, 500, Easing.SinOut));
        }
    }
    public async void ShowMusicController()
    {
        MauiProgram.MusicPlayerIsOpen = true;

        screenHeight = AllContent.Height;

        MediaController.IsVisible = true;
        if (musicControlsFirstOpen)
        {
            MediaController.TranslationY = screenHeight;
        }

        await Task.WhenAny<bool>
        (
            Player.TranslateTo(0, screenHeight * -1, animationSpeed, Easing.SinOut),
            MediaController.TranslateTo(0, 0, animationSpeed, Easing.SinOut),
            MediaController.FadeTo(1, animationSpeed, Easing.SinOut)
        );

        musicControlsFirstOpen = false;
                RefreshPlayer();
    }
    public Task AwaitContextMenuClose()
    {
        while (MauiProgram.ContextMenuIsOpen)
        {

        }
        return Task.CompletedTask;
    }
    public Task AwaitContextMenuOpen()
    {
        while (!MauiProgram.ContextMenuIsOpen)
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
    public async void UpdatePlaystate(long duration, long position)
    {
        if (!mediaCntrollerSliderDragging)
        {
            MediaCntroller_Slider.Maximum = duration;
            MediaCntroller_Slider.Value = position;

            double progressPercent = ((double)position / duration);
            Miniplayer_Progress.Progress = progressPercent;

            MediaCntroller_Slider_DurationTxt.Text = ConvertToTimeFormat(duration);
            MediaCntroller_Slider_PositionTxt.Text = ConvertToTimeFormat(position);
        }
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
    public async Task<bool> OpenContextMenu(BaseMusicItem baseMusicItem, int imgResolution)
    {
        MauiProgram.ContextMenuTaskList.Clear();
        MauiProgram.ShowContextMenuImage = true;

        if (baseMusicItem is Album)
        {
            Album album = (Album)baseMusicItem;
            album.image.soureResolution = imgResolution;

            MauiProgram.ContextMenuMainText = album.name;
            MauiProgram.ContextMenuSubText = album.artistCongregate;
            MauiProgram.ContextMenuImage = album.image.sourceAtResolution;
            MauiProgram.ContextMenuTaskList = album.GetContextMenuItems();
            MauiProgram.ContextMenuRoundImage = false;
        }
        else if (baseMusicItem is Artist)
        {
            Artist artist = (Artist)baseMusicItem;
            artist.image.soureResolution = imgResolution;

            MauiProgram.ContextMenuTaskList = artist.GetContextMenuItems();
            MauiProgram.ContextMenuMainText = artist.name;
            MauiProgram.ContextMenuSubText = String.Empty;
            MauiProgram.ContextMenuImage = artist.image.sourceAtResolution;
            MauiProgram.ContextMenuRoundImage = true;
        }
        else if (baseMusicItem is Playlist)
        {
            Playlist playlist = (Playlist)baseMusicItem;
            MauiProgram.ContextMenuTaskList = playlist.GetContextMenuItems();
            MauiProgram.ContextMenuImage = playlist.image.SourceAtResolution(imgResolution);
            MauiProgram.ContextMenuSubText = String.Empty;
            MauiProgram.ContextMenuMainText = playlist.name;
            MauiProgram.ContextMenuRoundImage = false;
        }
        else if (baseMusicItem is Song)
        {
            Song song = (Song)baseMusicItem;
            MauiProgram.ContextMenuTaskList = song.GetContextMenuItems();
            MauiProgram.ContextMenuImage = song.image.SourceAtResolution(imgResolution);
            MauiProgram.ContextMenuSubText = song.artistCongregate;
            MauiProgram.ContextMenuMainText = song.name;
            MauiProgram.ContextMenuRoundImage = false;
        }
        else if (baseMusicItem != null)
        {
            MauiProgram.ContextMenuMainText = baseMusicItem.name;
            MauiProgram.ContextMenuImage = baseMusicItem.image.SourceAtResolution(imgResolution);
            MauiProgram.ContextMenuRoundImage = false;
        }

        ContextMenu_imagecontainer.IsVisible = MauiProgram.ShowContextMenuImage;
        ContextMenu_MainText.Text = MauiProgram.ContextMenuMainText;
        ContextMenu_SubText.Text = MauiProgram.ContextMenuSubText;
        ContextMenu_image.Source = MauiProgram.ContextMenuImage;
        ContextMenu_List.ItemsSource = null;
        if (MauiProgram.ContextMenuRoundImage)
        {
            ContextMenu_imageBorder.StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(125, 125, 125, 125)
            };
        }
        else
        {
            ContextMenu_imageBorder.StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(5, 5, 5, 5)
            };
        }

        if (MauiProgram.ContextMenuTaskList.Count <= 0)
        {
            MauiProgram.ContextMenuTaskList.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.mainPage.CloseContextMenu();
            })));
        }

        ContextMenu_List.ItemsSource = MauiProgram.ContextMenuTaskList;
        ContextMenu_List.SelectedItem = null;
        ContextMenu.IsVisible = true;
        ContextMenu.TranslationY = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
        await ContextMenu.TranslateTo(ContextMenu.X, 0, animationSpeed, Easing.SinOut);
        MauiProgram.ContextMenuIsOpen = true;

        return true;
    }
    public async void CloseContextMenu()
    {
        isClosing = true;

        double h = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
        await ContextMenu.TranslateTo(ContextMenu.X, ContextMenu.Y + h, animationSpeed, Easing.SinIn);
        MauiProgram.ContextMenuIsOpen = false;
        ContextMenu.IsVisible = false;
        isClosing = false;
    }
    #endregion

    #region MediaController

    /// <summary>
    /// Button Close
    /// Interaction method called when the Close Button is pressed
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void MediaController_Button_Close_Clicked(object sender, EventArgs e)
    {
        CloseMusicController();
    }

    /// <summary>
    /// Public method for closing the media controller
    /// </summary>
    public async void CloseMusicController()
    {
        MauiProgram.MusicPlayerIsOpen = false;

        await Task.WhenAny<bool>
        (
            Player.TranslateTo(0, 0, animationSpeed, Easing.SinIn),
            MediaController.TranslateTo(0, screenHeight, animationSpeed, Easing.SinIn),
            MediaController.FadeTo(0, animationSpeed, Easing.SinIn)
        );
    }

    /// <summary>
    /// Player Previous Button Clicked
    /// Interaction method called when the Previous Song button is pressed in the Media Controller
    /// </summary>
    bool isSkipping = false;
    private void MediaCntroller_Player_Previous_btn_Clicked(object sender, EventArgs e)
    {
        Song? currentItem = (Song)MainPlayer_ImgCarousel.CurrentItem;
        if (currentItem != null)
        {
            int index = queueCongregate.IndexOf(currentItem);
            int prevIndex = index - 1;
            if (prevIndex >= 0)
            {
                MainPlayer_ImgCarousel.ScrollTo(prevIndex);
                MiniPlayer_ImgCarousel.ScrollTo(prevIndex);

                MainPlayer_ImgCarousel.CurrentItem = queueCongregate[prevIndex];
                MiniPlayer_ImgCarousel.CurrentItem = queueCongregate[prevIndex];

                MauiProgram.mediaService.PreviousTrack();
            }
        }
    }

    /// <summary>
    /// Player Next Button Clicked
    /// Interaction method called when the Next Song button is pressed in the Media Controller
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void MediaCntroller_Player_Next_btn_Clicked(object sender, EventArgs e)
    {
        Song? currentItem = (Song)MainPlayer_ImgCarousel.CurrentItem;
        if(currentItem != null)
        {
            int index = queueCongregate.IndexOf(currentItem);
            int nextIndex = index + 1;
            if(nextIndex < queueCongregate.Count())
            {
                MainPlayer_ImgCarousel.ScrollTo(nextIndex);
                MiniPlayer_ImgCarousel.ScrollTo(nextIndex);
                
                MainPlayer_ImgCarousel.CurrentItem = queueCongregate[nextIndex];
                MiniPlayer_ImgCarousel.CurrentItem = queueCongregate[nextIndex];

                MauiProgram.mediaService.NextTrack();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void MediaCntroller_SkipToIndex(int index)
    {

    }

    /// <summary>
    /// Image Carousel Current Item Changed
    /// Interaction method called when the Image Carousel changegs items
    /// </summary>
    private void MainPlayer_ImgCarousel_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        Song? currentItem = (Song)e.CurrentItem;
        Song? lastItem = (Song)e.PreviousItem;
        if (currentItem != null && lastItem != null)
        {
            int currentIndex = queueCongregate.IndexOf(currentItem);
            int lastIndex = queueCongregate.IndexOf(lastItem);
            if(currentIndex > lastIndex)
            { // Next song is requested
                MauiProgram.mediaService.NextTrack();
            }
            else
            { // Prev song is requested
                MauiProgram.mediaService.PreviousTrack();
            }
        }
    }

    /// <summary>
    /// Media Controller Play Button Clicked
    /// Interaction method called when the Play button is pressed.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void MediaController_Player_Play_btn_Clicked(object sender, EventArgs e)
    {
        MauiProgram.mediaService.TogglePlay();

        // Apply animations and update icon
        MediaController_Player_Play_btn.Opacity = 0;
        if (MauiProgram.mediaService.isPlaying)
        {
            MediaController_Player_Play_btn.Source = "pause.png";
        }
        else
        {
            MediaController_Player_Play_btn.Source = "play.png";
        }

        await Task.WhenAll(MediaController_Player_Play_btn.FadeTo(1, 500, Easing.SinOut));
    }

    /// <summary>
    /// Media Controller Repeat Button Clicked
    /// Interaction method called when the Shuffle button is pressed
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void MediaCntroller_Player_Repeat_btn_Clicked(object sender, EventArgs e)
    {
        // Call method in MediaService
        MauiProgram.mediaService.ToggleRepeat();

        // Apply animations and update icon
        MediaCntroller_Player_Repeat_btn.Opacity = 0;
        switch (MauiProgram.mediaService.repeatMode)
        {
            case 2:
                MediaCntroller_Player_Repeat_btn.Source = "repeat_on_single.png";
                break;
            case 1:
                MediaCntroller_Player_Repeat_btn.Source = "repeat_on.png";
                break;
            case 0:
                MediaCntroller_Player_Repeat_btn.Source = "repeat_off.png";
                break;
        }

        await Task.WhenAll(MediaCntroller_Player_Repeat_btn.FadeTo(1, 500, Easing.SinOut));
    }

    /// <summary>
    /// Media Controller Shuffle Button Clicked.
    /// Interaction method called when the Shuffle button is pressed.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void MediaCntroller_Player_Shuffle_btn_Clicked(object sender, EventArgs e)
    {
        // Call method in MediaService
        MauiProgram.mediaService.ToggleShuffle();

        // Apply animations and update icon
        MediaCntroller_Player_Shuffle_btn.Opacity = 0;
        if (MauiProgram.mediaService.shuffleOn)
        {
            MediaCntroller_Player_Shuffle_btn.Source = "shuffle_on.png";
        }
        else
        {
            MediaCntroller_Player_Shuffle_btn.Source = "shuffle.png";
        }
        await MediaCntroller_Player_Shuffle_btn.FadeTo(1, 500, Easing.SinOut);
    }

    /// <summary>
    /// Media Controller Favourite Button Clicked.
    /// Interaction method called when the Favourite button is pressed.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void MediaCntroller_Player_Fav_btn_Clicked(object sender, EventArgs e)
    {

    }
    #endregion

    #region Interactions
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void MiniPlayer_Btn_FavToggle_Clicked(object sender, EventArgs e)
    {
        // MauiProgram.mediaService.Pause();
    }
    private void MiniPlayer_Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {
        // MauiProgram.mediaService.Pause();
    }

    private void MiniPlayer_Clicked(object sender, EventArgs e)
    {
        ShowMusicController(); 
    }
    private async void ContextMenu_SelectedItemChanged(object sender, EventArgs e)
    {
        if(ContextMenu_List.SelectedItem == null)
        {
            return;
        }
        ContextMenuItem? selected = (ContextMenuItem)ContextMenu_List.SelectedItem;

        if (selected != null)
        {
            if(selected.action != null)
            {
                try
                {
                    selected.action.Start(TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    CloseContextMenu();
                    await DisplayAlert("Error!", ex.Message, "Close");
                }
            }
        }

        if (!isClosing)
        {
            CloseContextMenu();
        }
    }
    private void MediaController_Queue_Show(object sender, EventArgs e)
    {
        OpenMusicQueue();
    }
    private void MediaController_Queue_Hide(object sender, EventArgs e)
    {
        CloseMusicQueue();
    }
    private async void btn_navnar_home_click(object sender, EventArgs e)
    {
        waitingForPageLoad = true;
        ShowLoadingScreen(true);
        btn_navnar_home.Scale = 0.6;
        btn_navnar_home.Opacity = 0;
        await Task.WhenAll<bool>
        (
            btn_navnar_home.FadeTo(1, 250),
            btn_navnar_home.ScaleTo(1, 250)
        );
        MauiProgram.webView.NavigateHome();
    }
    private async void btn_navnar_search_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_search.Scale = 0.6;
        btn_navnar_search.Opacity = 0;
        await Task.WhenAll<bool>
        (
            btn_navnar_search.FadeTo(1, 250),
            btn_navnar_search.ScaleTo(1, 250)
        );
        MauiProgram.webView.NavigateSearch();
    }
    private async void btn_navnar_library_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_library.Scale = 0.6;
        btn_navnar_library.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_library.FadeTo(1, 250),
            btn_navnar_library.ScaleTo(1, 250)
        );
        MauiProgram.webView.NavigateLibrary();
    }
    private async void btn_navnar_favourite_click(object sender, EventArgs e)
    {
        ShowLoadingScreen(true);
        btn_navnar_favourites.Scale = 0.6;
        btn_navnar_favourites.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_favourites.FadeTo(1, 250),
            btn_navnar_favourites.ScaleTo(1, 250)
        );
        MauiProgram.webView.NavigateFavourites();
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
                double h = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
                MediaController.IsVisible = true;

                Player.TranslationY = Player.TranslationY + e.TotalY;
                MediaController.TranslationY = Player.TranslationY + MediaController.Height - 50;

                double vis = 0;
                if (Player.TranslationY < 0)
                {
                    double check = ((Player.TranslationY * -1) + 60) / MediaController.Height;
                    if (check > 0)
                    {
                        vis = check;
                    }
                }

                distance = Player.TranslationY;
                MediaController.Opacity = vis;

                //Player_Txt_Title.Text = distance.ToString();
                break;

            case GestureStatus.Completed:
                if (distance < 0) { distance *= -1; }
                uint speed = (uint)distance;

                screenHeight = AllContent.Height;
                if (distance > screenHeight / 3)
                {
                    musicControlsFirstOpen = false;
                    ShowMusicController();
                }
                else
                {
                    CloseMusicController();
                }
                break;
        }
#endif
    }
    private async void Queue_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
        #if !WINDOWS
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                double newLocation = ContextMenu.TranslationY + e.TotalY;
                if(newLocation >= 0)
                {
                    ContextMenu.TranslationY = newLocation;
                }
                break;

            case GestureStatus.Completed:
                await ContextMenu.TranslateTo(0, 0, animationSpeed, Easing.SinOut);
                break;
        }
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
        MauiProgram.mediaService.SeekTo(lastSeekPosition);
    }
    private void MediaCntroller_Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        long value = (long)e.NewValue;
        lastSeekPosition = value;
        MediaCntroller_Slider_PositionTxt.Text = ConvertToTimeFormat(value);
    }
    #endregion
}
