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
    private Song? previouslyPlaying = Song.Empty;
    private Song? currentlyPlaying = Song.Empty;
    private Song? nextPlaying = Song.Empty;
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

        // MainPlayer_PreviousMainImage
        double spacing = (AllContent.Width - 350) / 2;

        MainPlayer_PreviousContainer.TranslationX = MainPlayer_PreviousContainer.TranslationX - (350 + spacing);
        MainPlayer_NextContainer.TranslationX = MainPlayer_NextContainer.TranslationX + (350 + spacing);

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
    private Song songLastRefresh = Song.Empty;

    /// <summary>
    ///  Responsible for refresing the main page of the music controller
    /// </summary>
    /// <returns></returns>
    private string lastImageSource = String.Empty;
    public async Task<bool> RefreshPlayer()
    {
        List<Task> animations = new List<Task>{
                Player_Txt_Title.FadeTo(1, animationSpeed, Easing.SinOut),
                Player_Txt_Artist.FadeTo(1, animationSpeed, Easing.SinOut),
                Player_Img.FadeTo(1, animationSpeed, Easing.SinOut)
            };
        if (!MauiProgram.MiniPlayerIsOpen)
        {
            MiniPlayer.TranslationY = 120;

            animations.Add(MiniPlayer.FadeTo(1, animationSpeed, Easing.SinOut));
            animations.Add(MiniPlayer.TranslateTo(0, 0, animationSpeed, Easing.SinOut));
        }

        // Update the song that is currently playing
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

        // Update the song that will be playing next
        if (MauiProgram.mediaService.songQueue.Count() > 1)
        {
            nextPlaying = MauiProgram.mediaService.songQueue.GetQueue().ToList()[1];
        }
        else if (MauiProgram.mediaService.nextUpQueue.Count() > 1)
        {
            nextPlaying = MauiProgram.mediaService.nextUpQueue.GetQueue().ToList()[1];
        }
        else
        {
            nextPlaying = null;
        }

        // Update the song that played last, and the UI
        previouslyPlaying = MauiProgram.mediaService.PeekPreviousSong();
        if (previouslyPlaying != null)
        {
            MainPlayer_PreviousSongTitle.Text = previouslyPlaying.name;
            MainPlayer_PreviousArtistTitle.Text = previouslyPlaying.artistCongregate;
            if (previouslyPlaying.image.source != lastImageSource)
            {
                MainPlayer_PreviousMainImage.Source = previouslyPlaying.image.source;
            }
        }
        else
        {
            MainPlayer_PreviousContainer.IsVisible = false;
        }

        // Update the UI for the song that is currently playing
        if (currentlyPlaying != null)
        {
            if (lastImageSource != currentlyPlaying.image.source)
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
        }
        // Update the UI for the song that is playing next
        if(nextPlaying != null)
        {
            MainPlayer_NextSongTitle.Text = nextPlaying.name;
            MainPlayer_NextArtistTitle.Text = nextPlaying.artistCongregate;
            if (nextPlaying.image.source != currentlyPlaying.image.source)
            {
                MainPlayer_NextMainImage.Source = nextPlaying.image.source;
            }
        }
        else
        {
            MainPlayer_NextContainer.IsVisible = false;
        }

        if (!hideMidiPlayer)
        {
            MiniPlayer.IsEnabled = !hideMidiPlayer;
        }

        if(currentlyPlaying != songLastRefresh)
        {
            Player_Txt_Title.Opacity = 0;
            Player_Txt_Artist.Opacity = 0;
            Player_Img.Opacity = 0;

            await Task.WhenAll(animations);
        }

        RefreshQueue();
        lastImageSource = currentlyPlaying.image.source;
        songLastRefresh = currentlyPlaying;
        return true;
    }
    /// <summary>
    ///  Responsible for Refreshing the queue 
    /// </summary>
    public void RefreshQueue()
    {
        queue.Clear();
        queue = MauiProgram.mediaService.songQueue.GetQueue().ToObservableCollection();
        queueNextUp = MauiProgram.mediaService.nextUpQueue.GetQueue().ToObservableCollection();

        if (queue.Count > 0)
        {
            queue.RemoveAt(0);

            MediaController_Queue_Header.IsVisible = true;
            MediaController_Queue_List.IsVisible = true;
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
            if (queueNextUp.Count <= 0)
            {
                MediaController_NextUp_Header.IsVisible = false;
                MediaController_NextUp_List.IsVisible = false;
            }
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

        //if (MauiProgram.mediaService.nextUpQueue.Count() > 0)
        //{
        //    MediaController_Player_PlayingFromInfo.IsVisible = true;
        //    MediaController_Player_PlayingFromInfo_PlayingFromText.Text = MauiProgram.mediaService.nextUpQueue.FirstOrDefault().name;
        //    if (MauiProgram.mediaService.nextUpItem is Album)
        //    {
        //        MediaController_Player_PlayingFromInfo_PlayingFromType.Text = "Playing from Album";
        //    }
        //    else if (MauiProgram.mediaService.nextUpItem is Playlist)
        //    {
        //        MediaController_Player_PlayingFromInfo_PlayingFromType.Text = "Playing from Playlist";
        //    }
        //}

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
    public async void SkipTrack(bool? informExoPlayer = false)
    {
        MediaCntroller_Player_Next_btn.Opacity = 0;
        // Get and dequeue the next song from the media service
        if (nextPlaying == null ^ currentlyPlaying == null)
        {
            await Task.WhenAll(
                MediaCntroller_Player_Next_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut),
                MainPlayer_ImgContainer.TranslateTo(0, 0, animationSpeed, Easing.SinOut)
                );
            isSkipping = false;
            return;
        }

        // Reset position of images
        await MainPlayer_ImgContainer.TranslateTo(imgDragDistance, 0, 0);

        // this updates the media service queue
        currentlyPlaying = MauiProgram.mediaService.nextSong;
        Song? nextSongFromQueue = MauiProgram.mediaService.PeekNextSong();
        // Tell the media service to start playing the next song
        MauiProgram.mediaService.NextTrack();

        // Updating the current to appear to have 'skipped' desipite snapping back 
        if (nextSongFromQueue != null && currentlyPlaying != null)
        {
            MainPlayer_NextContainer.IsVisible = true;
            MainPlayer_NextContainer.IsEnabled = true;
            MainPlayer_NextContainer.Opacity = 1;

            // Play transform animations on images, this moves the next item to the current
            double spacing = (AllContent.Width - 350) / 2;
            await Task.WhenAny<bool>
            (
                 MainPlayer_ImgContainer.TranslateTo(-350 - (spacing * 2), 0, animationSpeed / 2, Easing.SinOut),
                 MediaCntroller_Player_Next_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut)
            );

            if (currentlyPlaying.image.source != nextSongFromQueue.image.source)
            {
                MainPlayer_MainImage.Source = nextSongFromQueue.image.source;
            }
            MainPlayer_SongTitle.Text = nextSongFromQueue.name;
            MainPlayer_ArtistTitle.Text = nextSongFromQueue.artistCongregate;
        }

        // Reset the position after updating the UI 
        // await Task.Delay(100);
        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Dispatch(async () => await RefreshPlayer());
        });
    }
    public async void PrevTrack(bool? informExoPlayer = false)
    {
        MediaCntroller_Player_Previous_btn.Opacity = 0;
        if (previouslyPlaying == null || currentlyPlaying == null)
        {
            await MediaCntroller_Player_Previous_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut);
            isSkipping = false;
            return;
        }

        // Reset position of images
        await MainPlayer_ImgContainer.TranslateTo(imgDragDistance, 0, 0);

        // this updates the media service queue
        Song lastSong = currentlyPlaying;
        currentlyPlaying = MauiProgram.mediaService.previousSong;
        // Tell the media service to start playing the last song
        MauiProgram.mediaService.PreviousTrack();

        // Updating the current to appear to have 'skipped' desipite snapping back 
        if (currentlyPlaying != null)
        {
            MainPlayer_PreviousContainer.IsVisible = true;
            MainPlayer_PreviousContainer.IsEnabled = true;
            MainPlayer_PreviousContainer.Opacity = 1;

            // Play transform animations on images
            double spacing = (AllContent.Width - 350) / 2;
            await Task.WhenAny<bool>
            (
                 MainPlayer_ImgContainer.TranslateTo(350 + (spacing * 2), 0, animationSpeed / 2, Easing.SinOut),
                 MediaCntroller_Player_Previous_btn.FadeTo(1, animationSpeed / 2, Easing.SinOut)
            );

            if (currentlyPlaying.image.source != lastSong.image.source)
            {
                MainPlayer_MainImage.Source = currentlyPlaying.image.source;
            }
            MainPlayer_SongTitle.Text = currentlyPlaying.name;
            MainPlayer_ArtistTitle.Text = currentlyPlaying.artistCongregate;
        }

        await MainPlayer_ImgContainer.TranslateTo(0, 0, 0);

        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Dispatch(async () => await RefreshPlayer());
        });
    }
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
        imgDragDistance = 0;
        isSkipping = true;

        PrevTrack();

        isSkipping = false;
    }
    private async void MediaCntroller_Player_Next_btn_Clicked(object sender, EventArgs e)
    {
        if (isSkipping)
        {
            return;
        }
        imgDragDistance = 0;
        isSkipping = true;

        SkipTrack();

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
    private async void MainPlayer_ImgContainer_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
        #if !WINDOWS
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                double h = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;

                MainPlayer_ImgContainer.TranslationX = MainPlayer_ImgContainer.TranslationX + e.TotalX;
                // distance = Player.TranslationX;

                imgDragDistance = MainPlayer_ImgContainer.TranslationX;
                break;

            case GestureStatus.Completed:
                // 
                if (imgDragDistance < -60) 
                {
                    SkipTrack();
                }
                else if (imgDragDistance > 60)
                {
                    PrevTrack();
                }
                else
                {
                    await MainPlayer_ImgContainer.TranslateTo(0, 0, animationSpeed, Easing.SinInOut);
                }
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
