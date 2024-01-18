using Microsoft.AspNetCore.Components.WebView.Maui;
using PortaJel_Blazor.Pages;

#if ANDROID
using Android;
#endif

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
        //NavigationPage.SetHasNavigationBar(this, false);

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

    #region Interactions
    private void btn_navnar_home_click(object sender, EventArgs e)
    {
        MauiProgram.mainLayout.NavigateHome();
    }
    private void btn_navnar_search_click(object sender, EventArgs e)
    {
        MauiProgram.mainLayout.NavigateSearch();
    }
    private void btn_navnar_library_click(object sender, EventArgs e)
    {
        MauiProgram.mainLayout.NavigateLibrary();
    }
    private void btn_navnar_favourite_click(object sender, EventArgs e)
    {
        MauiProgram.mainLayout.NavigateFavourites();
    }
    #endregion
}
