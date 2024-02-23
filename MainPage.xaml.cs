using PortaJel_Blazor.Classes;
using System.Windows.Input;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Pages.Xaml;
using Microsoft.Maui.Platform;
using System;
using Microsoft.Maui.Controls.Shapes;

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
        MediaController_Queue_List.ItemsSource = queue;
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
        Player_Btn_PlayToggle.Opacity = 0;
        if (MauiProgram.mediaService.isPlaying)
        {
            Player_Btn_PlayToggle.Source = "pause.png";
        }
        else
        {
            Player_Btn_PlayToggle.Source = "play.png";
        }
        await Player_Btn_PlayToggle.FadeTo(1, 500, Easing.SinOut);
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
                    double check = ((Player.TranslationY * -1)+60) / MediaController.Height;
                    if(check > 0)
                    {
                        vis = check;
                    }
                }

                distance = Player.TranslationY;
                //Player_Txt_Title.Text = distance.ToString();
                MediaController.Opacity = vis;
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
    #endregion
}
