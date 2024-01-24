using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MediaManager;
using PortaJel_Blazor.Classes;

namespace PortaJel_Blazor;

//https://github.com/Baseflow/XamarinMediaManager

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CrossMediaManager.Current.Init(this);
    }
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        // TODO: Handle app opened from notification tap.
    }
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Down)
            {
                // If the context menu is open, close it
                if (MauiProgram.ContextMenuIsOpen)
                {
                    MauiProgram.mainLayout.CloseContextMenu();
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
                    MauiProgram.mainLayout.isLoading = true;
                }
            }
            return base.DispatchKeyEvent(e);
        }
        return base.DispatchKeyEvent(e);
    }
}
