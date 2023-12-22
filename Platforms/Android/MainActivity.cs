using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using PortaJel_Blazor.Classes;

namespace PortaJel_Blazor;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool DispatchKeyEvent(KeyEvent e)
    {
        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Down)
            {
                // If the music player is open, close it 
                if (MauiProgram.mainLayout.musicPlayerContainer.isOpen)
                {
                    if (MauiProgram.MusicPlayerIsQueueOpen)
                    {
                        MauiProgram.mainLayout.musicPlayerContainer.ReturnToPlayer();
                        return false;
                    }
                    else
                    {
                        MauiProgram.mainLayout.ClosePlayer();
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
