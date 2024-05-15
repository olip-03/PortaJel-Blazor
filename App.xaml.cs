using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using PortaJel_Blazor.Resources.Themes;

namespace PortaJel_Blazor;

public partial class App : Application
{
	public App()
	{
		InitializeComponent(); 
        MainPage = new MainPage();

        MauiProgram.MainPage = (MainPage)MainPage;
        MauiProgram.MainPage.Initialize();

        Resources.MergedDictionaries.Add(new DarkTheme());
        Resources = new DarkTheme();
    }
}
