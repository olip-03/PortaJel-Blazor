using PortaJel_Blazor.Classes;
using System.Windows.Input;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Pages.Xaml;
using Microsoft.Maui.Platform;
using System;
using Microsoft.Maui.Controls.Shapes;
using Jellyfin.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SkiaSharp;

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

    private List<Song> queue = new List<Song>();
    private List<Song> queueNextUp = new List<Song>();
    private Song currentlyPlaying = Song.Empty;
    private bool hideMidiPlayer = true;

    private uint animationSpeed = 550;

    public MainPage(bool? initalize = true)
	{
        if (initalize == false)
        {
            return;
        }

        InitializeComponent();
        Initialize();
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
        MauiProgram.webView = blazorWebView;
    }
    private async void Initialize()
    {
        await MauiProgram.LoadData();

        await Task.Run(() =>
        {
            while (!MauiProgram.webViewInitalized)
            {

            }
        });

        if (MauiProgram.api.GetServers().Count() <= 0)
        {
            AddServerView addServerView = new();
            await MauiProgram.mainPage.PushModalAsync(addServerView, false);
            await Task.Run(() => addServerView.AwaitClose(addServerView));
            MauiProgram.firstLoginComplete = true;
        }

        // MainPlayer_PreviousMainImage
        double spacing = (AllContent.Width - 350) / 2;

        MainPlayer_PreviousContainer.TranslationX = MainPlayer_PreviousContainer.TranslationX - (350 + spacing);
        MainPlayer_NextContainer.TranslationX = MainPlayer_NextContainer.TranslationX + (350 + spacing);

        ShowNavbar();
        MauiProgram.mediaService.Initalize();
        MauiProgram.mainLayout.NavigateHome();
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

        MediaController_Player_PlayingFromInfo.IsVisible = MauiProgram.mediaService.nextUpIsAvaliable;

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
    private Song songLastRefresh = Song.Empty;
    public async Task<bool> RefreshPlayer()
    {
        RefreshQueue();

        // Queue takes priority. If there are items in the queue
        // ALWAYS pick from the top for the currently playing track
        if (MauiProgram.mediaService.songQueue.Count() > 0)
        {
            currentlyPlaying = MauiProgram.mediaService.songQueue.GetQueue().ToList().First();
            hideMidiPlayer = false;
            MauiProgram.MiniPlayerIsOpen = true;
        }
        else if(MauiProgram.mediaService.nextUpQueue.Count() > 0)
        {
            currentlyPlaying = MauiProgram.mediaService.nextUpQueue.GetQueue().ToList().First();
            hideMidiPlayer = false;
            MauiProgram.MiniPlayerIsOpen = true;
        }
        else
        {
            MauiProgram.MiniPlayerIsOpen = false;
            hideMidiPlayer = true;
            return false;
        }

        // Set main player source
        if(MainPlayer_MainImage.Source.ToString() != currentlyPlaying.image.source) 
        {
            // TODO: Fix blurhash working 
            //using (var stream = currentlyPlaying.image.BlurHashToStream(20, 20))
            //{
            //    MainPlayer_MainImage.Source = ImageSource.FromStream(() => stream);
            //}
            MainPlayer_MainImage.Source = currentlyPlaying.image.source;
            MainPlayer_NextMainImage.Source = currentlyPlaying.image.source;
        }
        MainPlayer_SongTitle.Text = currentlyPlaying.name;
        MainPlayer_ArtistTitle.Text = currentlyPlaying.artistCongregate;
        MainPlayer_NextSongTitle.Text = currentlyPlaying.name;
        MainPlayer_NextArtistTitle.Text = currentlyPlaying.artistCongregate;

        if (Player_Img.Source.ToString() != currentlyPlaying.image.source) 
        {
            // TODO: Fix blurhash working 
            //using (var stream = currentlyPlaying.image.BlurHashToStream(20, 20))
            //{
            //    Player_Img.Source = ImageSource.FromStream(() => stream);
            //}
            Player_Img.Source = currentlyPlaying.image.source; 
        }
        Player_Txt_Title.Text = currentlyPlaying.name;
        Player_Txt_Artist.Text = currentlyPlaying.artistCongregate;

        if (!hideMidiPlayer)
        {
            MiniPlayer.IsEnabled = !hideMidiPlayer;
        }

        if(currentlyPlaying != songLastRefresh)
        {
            Player_Txt_Title.Opacity = 0;
            Player_Txt_Artist.Opacity = 0;
            Player_Img.Opacity = 0;
            MiniPlayer.TranslationY = 120;

            await Task.WhenAll(
                Player_Txt_Title.FadeTo(1, animationSpeed, Easing.SinOut),
                Player_Txt_Artist.FadeTo(1, animationSpeed, Easing.SinOut),
                Player_Img.FadeTo(1, animationSpeed, Easing.SinOut),
                MiniPlayer.FadeTo(1, animationSpeed, Easing.SinOut),
                MiniPlayer.TranslateTo(0, 0, animationSpeed, Easing.SinOut)
            );
        }


        songLastRefresh = currentlyPlaying;
        return true;
    }
    public void RefreshQueue()
    {
        queue.Clear();
        queue = MauiProgram.mediaService.songQueue.GetQueue().ToList();
        queueNextUp = MauiProgram.mediaService.nextUpQueue.GetQueue().ToList();

        if (queue.Count > 0)
        {
            queue.RemoveAt(0);

            MediaController_Queue_Header.IsVisible = true;
            MediaController_Queue_List.IsVisible = true;
            currentlyPlaying = queue.First();
            if(queueNextUp.Count <= 0)
            {
                MediaController_NextUp_Header.IsVisible = false;
                MediaController_NextUp_List.IsVisible = false;
            }
        }
        else if(queueNextUp.Count > 0)
        {
            queueNextUp.RemoveAt(0);

            MediaController_NextUp_Header.IsVisible = true;
            MediaController_NextUp_List.IsVisible = true;

            MediaController_Queue_Header.IsVisible = false;
            MediaController_Queue_List.IsVisible = false;
            currentlyPlaying = queueNextUp.First();
        }
        else
        {
            return;
        }

        queue_currentlyplaying_img.Source = currentlyPlaying.image.source;
        queue_currentlyplaying_title_lbl.Text = currentlyPlaying.name;
        queue_currentlyplaying_artisttitle_lbl.Text = currentlyPlaying.artistCongregate;

        MediaController_Queue_List.ItemsSource = queue;
        MediaController_NextUp_List.ItemsSource = queueNextUp;
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
    public async void ShowMusicController()
    {
        MauiProgram.MusicPlayerIsOpen = true;

        screenHeight = AllContent.Height;

        if (MauiProgram.mediaService.nextUpIsAvaliable && MauiProgram.mediaService.nextUpItem != null)
        {
            MediaController_Player_PlayingFromInfo.IsVisible = MauiProgram.mediaService.nextUpIsAvaliable;
            MediaController_Player_PlayingFromInfo_PlayingFromText.Text = MauiProgram.mediaService.nextUpItem.name;
            if (MauiProgram.mediaService.nextUpItem is Album)
            {
                MediaController_Player_PlayingFromInfo_PlayingFromType.Text = "Playing from Album";
            }
            else if (MauiProgram.mediaService.nextUpItem is Playlist)
            {
                MediaController_Player_PlayingFromInfo_PlayingFromType.Text = "Playing from Playlist";
            }
        }

        MediaController.IsVisible = true;
        if (musicControlsFirstOpen)
        {
            MediaController.TranslationY = screenHeight;
        }

        // MainPlayer_ImgContainer.Spacing = ;
        // MainPlayer_ImgContainer.TranslationX = translation; // center this 

        await Task.WhenAny<bool>
        (
            Player.TranslateTo(0, screenHeight * -1, animationSpeed, Easing.SinOut),
            MediaController.TranslateTo(0, 0, animationSpeed, Easing.SinOut),
            MediaController.FadeTo(1, animationSpeed, Easing.SinOut)
        );

        RefreshQueue();
        musicControlsFirstOpen = false;
    }
    public async Task CloseContextMenu()
    {
        isClosing = true;

        double h = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
        await ContextMenu.TranslateTo(ContextMenu.X, ContextMenu.Y + h, animationSpeed, Easing.SinIn);
        MauiProgram.ContextMenuIsOpen = false;
        ContextMenu.IsVisible = false;
        isClosing = false;
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
    public async Task ShowContextMenu()
    {
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
            MauiProgram.ContextMenuTaskList.Add(new ContextMenuItem("Close", "light_close.png", new Task(async () =>
            {
                await MauiProgram.mainPage.CloseContextMenu();
            })));
        }

        ContextMenu_List.ItemsSource = MauiProgram.ContextMenuTaskList;
        ContextMenu_List.SelectedItem = null;
        ContextMenu.IsVisible = true;
        ContextMenu.TranslationY = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
        await ContextMenu.TranslateTo(ContextMenu.X, 0, animationSpeed, Easing.SinOut);
        MauiProgram.ContextMenuIsOpen = true;
    }
    public async void ShowLoadingScreen(bool value)
    {
        if (value == true && LoadingBlockout.IsVisible)
        { // If we're already visible, do nothin'
            return;
        }
        if (value == false && !LoadingBlockout.IsVisible)
        { // If we're not already visible, do nothin'
            return;
        }


        if (value == true)
        {
            LoadingBlockout.Opacity = 1;
        }

        if(LoadingBlockout.IsVisible && value == false)
        {
            await LoadingBlockout.FadeTo(0, 500, Easing.SinOut);
        }

        LoadingBlockout.IsVisible = value;
        LoadingBlockout.IsEnabled = value;
    }
    #endregion
    #region Interactions
    private void Player_Btn_FavToggle_Clicked(object sender, EventArgs e)
    {
        // MauiProgram.mediaService.Pause();
    }
    private async void Player_Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {
        MauiProgram.mediaService.TogglePlay();
        RefreshPlayState();
    }
    private async void MediaCntroller_Player_Repeat_btn_Clicked(object sender, EventArgs e)
    {
        MauiProgram.mediaService.ToggleRepeat();
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
    private async void MediaCntroller_Player_Shuffle_btn_Clicked(object sender, EventArgs e)
    {
        MauiProgram.mediaService.ToggleShuffle();
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
    bool isSkipping = false;
    private async void MediaCntroller_Player_Previous_btn_Clicked(object sender, EventArgs e)
    {
        if (isSkipping)
        {
            return;
        }
        isSkipping = true;
        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        MauiProgram.mediaService.NextTrack();
        MediaCntroller_Player_Previous_btn.Opacity = 0;

        double spacing = (AllContent.Width - 350) / 2;
        await Task.WhenAny<bool>
        (
             MainPlayer_ImgContainer.TranslateTo(350 + (spacing * 2), 0, animationSpeed / 2, Easing.SinIn),
             MediaCntroller_Player_Previous_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut)
        );

        await Task.Delay(100);
        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        RefreshQueue();
        isSkipping = false;
    }
    private async void MediaCntroller_Player_Next_btn_Clicked(object sender, EventArgs e)
    {
        if (isSkipping)
        {
            return;
        }
        isSkipping = true;

        Song nextTune = Song.Empty;

        //TODO Change to main queue source instead of having a local one. We have it for a reason
        if (MauiProgram.mediaService.songQueue.Count() > 1)
        {
            nextTune = MauiProgram.mediaService.songQueue.DequeueSong();
        }
        else if(MauiProgram.mediaService.nextUpQueue.Count() > 0)
        {
            nextTune = MauiProgram.mediaService.nextUpQueue.DequeueSong();
        }
        else
        {
            await MediaCntroller_Player_Next_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut);
            isSkipping = false;
            return;
        }

        // TODO: Change this to be updated when the main image would be (on refresh i believe)?
        MainPlayer_NextMainImage.Source = nextTune.image.source;
        MainPlayer_NextSongTitle.Text = nextTune.name;
        MainPlayer_NextArtistTitle.Text = nextTune.artistCongregate;

        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        MauiProgram.mediaService.PreviousTrack();
        MediaCntroller_Player_Next_btn.Opacity = 0;

        // Play transform animations on images
        double spacing = (AllContent.Width - 350) / 2;
        await Task.WhenAny<bool>
        (
             MainPlayer_ImgContainer.TranslateTo(-350 - (spacing * 2), 0, animationSpeed / 2, Easing.SinIn),
             MediaCntroller_Player_Next_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut)
        );

        MainPlayer_MainImage.Source = nextTune.image.source;
        MainPlayer_SongTitle.Text = nextTune.name;
        MainPlayer_ArtistTitle.Text = nextTune.artistCongregate;

        await Task.Delay(100);
        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        RefreshQueue();
        isSkipping = false;
    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        CloseMusicController();
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
            if(selected.job != null)
            {
                try
                {
                    selected.job.RunSynchronously();
                }
                catch (Exception)
                {
                    await CloseContextMenu();
                }
            }
        }

        if (!isClosing)
        {
            await CloseContextMenu();
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
        if (waitingForPageLoad)
        { // If we're waiting before the requested page is loading, another task is still running and needs to be canned
            MauiProgram.mainLayout.CancelLoading();

        }
        MauiProgram.mainLayout.isLoading = true;
        waitingForPageLoad = true;
        MauiProgram.mainLayout.NavigateHome();
        btn_navnar_home.Scale = 0.6;
        btn_navnar_home.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_home.FadeTo(1, 250),
            btn_navnar_home.ScaleTo(1, 250)
        );
    }
    private async void btn_navnar_search_click(object sender, EventArgs e)
    {
        if (MauiProgram.mainLayout.isLoading)
        { // If we're waiting before the requested page is loading, another task is still running and needs to be canned
            MauiProgram.mainLayout.CancelLoading();
        }
        MauiProgram.mainLayout.isLoading = true;
        waitingForPageLoad = true;
        MauiProgram.mainLayout.NavigateSearch();
        btn_navnar_search.Scale = 0.6;
        btn_navnar_search.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_search.FadeTo(1, 250),
            btn_navnar_search.ScaleTo(1, 250)
        );
    }
    private async void btn_navnar_library_click(object sender, EventArgs e)
    {
        if (waitingForPageLoad)
        { // If we're waiting before the requested page is loading, another task is still running and needs to be canned
            // Call Cancellation token
            // Recreate cancellation token
        }
        MauiProgram.mainLayout.isLoading = true;
        waitingForPageLoad = true;
        MauiProgram.mainLayout.NavigateLibrary();
        btn_navnar_library.Scale = 0.6;
        btn_navnar_library.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_library.FadeTo(1, 250),
            btn_navnar_library.ScaleTo(1, 250)
        );
    }
    private async void btn_navnar_favourite_click(object sender, EventArgs e)
    {
        if (waitingForPageLoad)
        { // If we're waiting before the requested page is loading, another task is still running and needs to be canned
            // Call Cancellation token
            // Recreate cancellation token
        }
        MauiProgram.mainLayout.isLoading = true;
        waitingForPageLoad = true;
        MauiProgram.mainLayout.NavigateFavourites();
        btn_navnar_favourites.Scale = 0.6;
        btn_navnar_favourites.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_favourites.FadeTo(1, 250),
            btn_navnar_favourites.ScaleTo(1, 250)
        );
    }
    private void MediaController_Btn_ContextMenu(object sender, EventArgs args)
    {
        ShowContextMenu();
    }
    private async void MainPlayer_ImgContainer_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
        #if !WINDOWS
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                double h = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;

                MainPlayer_ImgContainer.TranslationX = MainPlayer_ImgContainer.TranslationX + e.TotalX;
                distance = Player.TranslationX;
                break;

            case GestureStatus.Completed:
                await MainPlayer_ImgContainer.TranslateTo(0, 0, animationSpeed, Easing.SinOut);
                //if (distance < 0) { distance *= -1; }
                //uint speed = (uint)distance;
                //lockUpDown = false;

                //screenHeight = AllContent.Height;
                //if (distance > screenHeight / 3)
                //{
                //    musicControlsFirstOpen = false;
                //    ShowMusicController();
                //}
                //else
                //{
                //    CloseMusicController();
                //}
                break;
        }
        #endif
    }

    bool upDown = true; bool lockUpDown = false;
    double distance = 0;
    double lastX = 0;
    double lastY = 0;
    private void Navbar_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
#if !WINDOWS
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                double h = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
                MediaController.IsVisible = true;

                double xDiff = lastX - e.TotalX;
                double yDiff = lastY - e.TotalY;
                double angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

                if (angle < 0) { angle += 360; }

                var directions = new string[] { "LR", "UD", "UD", "UD" };
                var index = (int)Math.Round((angle % 360) / 45) % 4;

                if(directions[index] == "LR" && !lockUpDown)
                {
                    upDown = false;
                    lockUpDown = true;
                }
                else if(!lockUpDown)
                {
                    upDown = true;
                    lockUpDown = true;
                }

                lastX = e.TotalX;
                lastY = e.TotalY;

                if (upDown)
                {
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
                }
                else
                {
                    // handle song skip animation
                }

                //Player_Txt_Title.Text = distance.ToString();
                break;

            case GestureStatus.Completed:
                if (distance < 0) { distance *= -1; }
                uint speed = (uint)distance;
                lockUpDown = false;

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
    #endregion
}
