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
}
