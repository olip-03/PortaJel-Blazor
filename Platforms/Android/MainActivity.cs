using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
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
            // TODO: Fix style to make application fullscreen :3 
            // WindowCompat.SetDecorFitsSystemWindows(Window, false);

            TypedArray styledAttributes = Theme.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ActionBarSize });
            var height = (int)styledAttributes.GetDimension(0, 0);
            MauiProgram.SystemHeaderHeight = height;
            styledAttributes.Recycle();

            var hasSource = App.Current.Resources.TryGetValue("PageBackgroundColor", out object imageSource);

            if (hasSource)
            {
                Color color = (Color)imageSource;
                Android.Graphics.Color newColor = Android.Graphics.Color.ParseColor(color.ToHex());
                Window.SetNavigationBarColor(newColor);
            }

            DynamicColors.ApplyToActivitiesIfAvailable(Application);
        }
        
        base.OnCreate(savedInstanceState);
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
                    MauiProgram.MainPage.CloseContextMenu();
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
}
