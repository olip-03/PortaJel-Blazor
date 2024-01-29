using Microsoft.Maui.Controls;

namespace PortaJel_Blazor;

public partial class App : Application
{
	public App()
	{
		InitializeComponent(); 
        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var windows = base.CreateWindow(activationState);

#if WINDOWS
        windows.Height = 900;
        windows.Width = 500;
#endif

        // Add here your sizing code
        // Add here your positioning code

        return windows;
    }
}
