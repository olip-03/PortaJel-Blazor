using Microsoft.AspNetCore.Components.WebView.Maui;
using Plugin.Maui.Audio;
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
        
        MauiProgram.mediaElement = mediaElement;
		MauiProgram.webView = blazorWebView;

#if ANDROID
		// var mediaController = Android.Widget.MediaController.IMediaPlayerControl;
#endif

  //      mediaElement.Source = "https://media.olisshittyserver.xyz/Audio/8eb18a8f-0cb9-04d2-b97c-a565cf16698f/stream?additionalProp1=string&additionalProp2=string&additionalProp3=string";
		//mediaElement.ShouldShowPlaybackControls = true;
		//mediaElement.Play();
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
