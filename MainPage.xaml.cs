using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Pages;
using System.Windows.Input;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Pages.Xaml;
using static Microsoft.Maui.ApplicationModel.Permissions;

#if ANDROID
using Android;
#endif

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
    private Album? contextMenuAlbum = null;
    private bool IsRefreshing = false;
    public event EventHandler CanExecuteChanged;
    private bool isClosing = false;
    private double screenHeight = 0;
    private bool musicControlsFirstOpen = true;
    private List<Song> queue = new List<Song>();

    public MainPage()
	{
        InitializeComponent();
        MauiProgram.mainPage = this;

        for (int i = 0; i < 20; i++)
        {
            Song newSong = new();
            newSong.name = "Song " + i;
            newSong.artist = "Demonstration " + i;
            newSong.imageSrc = "emptyalbum.png";

            queue.Add(newSong);
        }
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
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
		// Disabled overscroll 'stretch' effect that I fucking hate.
		// CLEAR giveaway this app uses webview lolz

        #if ANDROID
				        var blazorView = this.blazorWebView;
				        var platformView = (Android.Webkit.WebView)blazorView.Handler.PlatformView;
				        platformView.OverScrollMode = Android.Views.OverScrollMode.Never;
        #endif
    }
    public interface IRelayCommand : ICommand
    {
        void UpdateCanExecuteState();
    }
    public void UpdateCanExecuteState()
    {
        IsRefreshing = true;
        if (CanExecuteChanged != null)
            CanExecuteChanged(this, new EventArgs());
        IsRefreshing = false;
    }
    #region Methods
    public async void ShowNavbar()
    {
        Navbar.IsVisible = true;
        Navbar.Opacity = 0;
        await Navbar.FadeTo(1, 1000);
    }
    public async void CloseMusicQueue()
    {
        screenHeight = AllContent.Height;

        MauiProgram.MusicPlayerIsQueueOpen = false;
        MediaController_Player.IsVisible = true;

        MediaController_Queue.TranslationY = 0;
        await MediaController_Queue.TranslateTo(0, screenHeight, 500, Easing.SinIn);

        // MediaController_Queue.IsVisible = false;
    }
    public async void OpenMusicQueue()
    {
        screenHeight = AllContent.Height;

        MauiProgram.MusicPlayerIsQueueOpen = true;
        MediaController_Queue.IsVisible = true;
        
        MediaController_Queue.TranslationY = screenHeight;
        await MediaController_Queue.TranslateTo(0, 0, 500, Easing.SinOut);

        MediaController_Player.IsVisible = false;
    }
    public async void CloseMusicController()
    {
        MauiProgram.MusicPlayerIsOpen = false;

        await Task.WhenAny<bool>
        (
            Player.TranslateTo(0, 0, 500, Easing.SinIn),
            MediaController.TranslateTo(0, screenHeight, 500, Easing.SinIn),
            MediaController.FadeTo(0, 500, Easing.SinIn)
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
            Player.TranslateTo(0, screenHeight * -1, 500, Easing.SinOut),
            MediaController.TranslateTo(0, 0, 500, Easing.SinOut),
            MediaController.FadeTo(1, 500, Easing.SinOut)
        );

        musicControlsFirstOpen = false;
    }
    public async void CloseContextMenu()
    {
        isClosing = true;
        MauiProgram.ContextMenuIsOpen = false;

        double h = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
        await ContextMenu.TranslateTo(ContextMenu.X, ContextMenu.Y + h, 500, Easing.SinIn);
        ContextMenu.IsVisible = false;
        isClosing = false;
    }
    public async void ShowContextMenu()
    {
        ContextMenu_imagecontainer.IsVisible = MauiProgram.ShowContextMenuImage;
        ContextMenu_MainText.Text = MauiProgram.ContextMenuMainText;
        ContextMenu_SubText.Text = MauiProgram.ContextMenuSubText;
        ContextMenu_image.Source = MauiProgram.ContextMenuImage;
        ContextMenu_List.ItemsSource = null;

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
        MauiProgram.ContextMenuIsOpen = true;
        await ContextMenu.TranslateTo(ContextMenu.X, 0, 500, Easing.SinOut);
    }
    #endregion

    #region Interactions
    private void Player_Btn_FavToggle_Clicked(object sender, EventArgs e)
    {

    }
    private void Player_Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {

    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        CloseMusicController();
    }
    private void MiniPlayer_Clicked(object sender, EventArgs e)
    {
        ShowMusicController(); 
    }
    private void ContextMenu_SelectedItemChanged(object sender, EventArgs e)
    {
        if(ContextMenu_List.SelectedItem == null)
        {
            return;
        }
        ContextMenuItem? selected = (ContextMenuItem)ContextMenu_List.SelectedItem;
        if(selected != null)
        {
            if(selected.job != null)
            {
                try
                {
                    selected.job.RunSynchronously();
                }
                catch (Exception)
                {
                    CloseContextMenu();
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
        MauiProgram.mainLayout.NavigateFavourites();
        btn_navnar_favourites.Scale = 0.6;
        btn_navnar_favourites.Opacity = 0;
        await Task.WhenAny<bool>
        (
            btn_navnar_favourites.FadeTo(1, 250),
            btn_navnar_favourites.ScaleTo(1, 250)
        );
    }
    private async void MediaController_Btn_ContextMenu(object sender, EventArgs args)
    {
        ShowContextMenu();
    }
    double distance = 0;
    private async void Navbar_PinchUpdated(object sender, PanUpdatedEventArgs e)
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
