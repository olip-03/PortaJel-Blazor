using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.Color;
using Google.Android.Material.Internal;

namespace PortaJel_Blazor;

// https://github.com/Baseflow/XamarinMediaManager
// https://stackoverflow.com/questions/76275381/an-easy-way-to-check-and-request-maui-permissions-including-bluetooth
// Dynamic Colours https://developer.android.com/develop/ui/views/theming/dynamic-colors#kotlin

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if (Application != null && Window != null && App.Current != null && Theme != null)
        {
            // Fit contants to whole screen and set header to be transparent
            WindowCompat.SetDecorFitsSystemWindows(Window, false);
            Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);

            // Gets the headers height so we can space things correctly 
            if (Resources != null)
            {
                double statBarHeight = Resources.GetDimensionPixelSize(Resources.GetIdentifier("status_bar_height", "dimen", "android"));

                DisplayMetrics displayMetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                double androidheight = displayMetrics.HeightPixels;
                double mauiheight = DeviceDisplay.MainDisplayInfo.Height;
                double diff = androidheight / mauiheight;
                if(androidheight > mauiheight) { diff =  mauiheight / androidheight; }
                if(androidheight < mauiheight) { diff = androidheight / mauiheight; }

                statBarHeight /= DeviceDisplay.Current.MainDisplayInfo.Density;
                statBarHeight *= diff;

                MauiProgram.SystemHeaderHeight = statBarHeight;
            }
            
            // Themeing Settings
            var hasSource = App.Current.Resources.TryGetValue("PageBackgroundColor", out object imageSource);
            if (hasSource)
            {
                Color color = (Color)imageSource;
                Android.Graphics.Color newColor = Android.Graphics.Color.ParseColor(color.ToHex());
                Window.SetNavigationBarColor(newColor);
            }

            // Apply dynamic colors TODO: Implement dynamic colors
            DynamicColors.ApplyToActivitiesIfAvailable(Application);
        }
        
        base.OnCreate(savedInstanceState);
    }
    protected override void OnDestroy()
    {
        if (MauiProgram.MediaService != null) MauiProgram.MediaService.Destroy();
        // Task.Run(async () => await MauiProgram.api.Logout());
    }
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        // TODO: Handle app opened from notification tap.
    }
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if(e == null)
        {
            return false;
        }

        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Down)
            {
                // If there are any modals open, close them
                // Additional check for the firstLoginComplete. This is to ensure we cant close
                // the modal for logging in before the app has actually allowed the user to sign in :)
                if (MauiProgram.MainPage.StackCount() > 0 && MauiProgram.firstLoginComplete)
                {
                    MauiProgram.MainPage.PopStack();
                    return false;
                }
                else if (!MauiProgram.firstLoginComplete && App.Current != null && App.Current.MainPage != null)
                {
                    // null ref checks make me want to fuckign die 
                    // This is also just to prevent a bug from occuring
                    App.Current.CloseWindow(App.Current.MainPage.Window);
                }

                // If the context menu is open, close it
                if (MauiProgram.MainPage.isContextMenuOpen)
                {
                    CloseContextMenu();
                    return false;
                }                
                // If the music player is open, close it 
                if (MauiProgram.MainPage.MainMediaController.IsOpen)
                {
                    if (MauiProgram.MainPage.MainMediaQueue.IsOpen)
                    {
                        // TODO: Update queue close endpoint 
                        MauiProgram.MainPage.MainMediaQueue.Close();
                        return false;
                    }
                    else
                    {
                        MauiProgram.MainPage.MainMediaController.Close().ConfigureAwait(false);
                        return false;
                    }
                }
                else if(MauiProgram.ViewHeaderCloseSelectCallback != null)
                {
                    MauiProgram.ViewHeaderCloseSelectCallback.Invoke();
                    MauiProgram.ViewHeaderCloseSelectCallback = null;
                    return false;
                }
                else
                {
                    // Navigate back request
                    // Check if we should actually be doing this
                    MauiProgram.MainPage.ShowLoadingScreen(true);
                    MauiProgram.WebView.isLoading = true;
                }
            }
            return base.DispatchKeyEvent(e);
        }
        return base.DispatchKeyEvent(e);
    }
    private async void CloseContextMenu()
    {
        await MauiProgram.MainPage.MainContextMenu.Close();
    }
}
