using PortaJel_Blazor.Pages;

namespace PortaJel_Blazor;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
        //NavigationPage.SetHasNavigationBar(this, false);
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
