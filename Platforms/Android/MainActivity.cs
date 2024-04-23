using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Google.Android.Material.Color;

namespace PortaJel_Blazor;

// https://github.com/Baseflow/XamarinMediaManager
// https://stackoverflow.com/questions/76275381/an-easy-way-to-check-and-request-maui-permissions-including-bluetooth
// Dynamic Colours https://developer.android.com/develop/ui/views/theming/dynamic-colors#kotlin

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if(Application != null)
        {
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
                if (MauiProgram.mainPage.StackCount() > 0 && MauiProgram.firstLoginComplete)
                {
                    MauiProgram.mainPage.PopStack();
                    return false;
                }
                else if (!MauiProgram.firstLoginComplete && App.Current != null && App.Current.MainPage != null)
                {
                    // null ref checks make me want to fuckign die 
                    // This is also just to prevent a bug from occuring
                    App.Current.CloseWindow(App.Current.MainPage.Window);
                }

                // If the context menu is open, close it
                if (MauiProgram.mainPage.isContextMenuOpen)
                {
                    MauiProgram.mainPage.CloseContextMenu();
                    return false;
                }                
                // If the music player is open, close it 
                if (MauiProgram.MusicPlayerIsOpen)
                {
                    if (MauiProgram.MusicPlayerIsQueueOpen)
                    {
                        MauiProgram.mainPage.CloseMusicQueue();
                        return false;
                    }
                    else
                    {
                        MauiProgram.mainPage.CloseMusicController();
                        return false;
                    }
                }
                else
                {
                    // Navigate back request
                    // Check if we should actually be doing this
                    MauiProgram.mainPage.ShowLoadingScreen(true);
                    MauiProgram.webView.isLoading = true;
                }
            }
            return base.DispatchKeyEvent(e);
        }
        return base.DispatchKeyEvent(e);
    }
}
