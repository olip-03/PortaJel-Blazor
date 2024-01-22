using MediaManager.Library;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Pages;

#if ANDROID
using Android;
#endif

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
    private Album? contextMenuAlbum = null;
    private List<ContextMenuItem> contextMenuItems = new();
    public MainPage()
	{
        InitializeComponent();
        MauiProgram.mainPage = this;

        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {

        })));
        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {

        })));
        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {

        })));
        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {

        })));
        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {

        })));
        contextMenuItems.Add(new ContextMenuItem("View Artist", "Favourite.png", new Task(() =>
        {
           
        })));

        ContextMenu_List.ItemsSource = contextMenuItems;

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

    #region Methods
    public async void ShowNavbar()
    {
        Navbar.IsVisible = true;
        Navbar.Opacity = 0;
        await Navbar.FadeTo(1, 500);
    }
    #endregion

    #region Interactions
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
    private async void Navbar_PointerEnter(object sender, EventArgs e)
    {

    }
    private async void Navbar_PointerExit(object sender, EventArgs e)
    {

    }
    private async void Navbar_PointerMoved(object sender, EventArgs e)
    {

    }
    private async void Navbar_PinchUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                Player.TranslationY = Player.TranslationY + e.TotalY;
                break;

            case GestureStatus.Completed:
                await Player.TranslateTo(0, 0, 500, Easing.SinInOut);
                break;
        }
    }
    private async void Navbar_Touched(object sender, EventArgs e)
    {

    }
    #endregion
}
