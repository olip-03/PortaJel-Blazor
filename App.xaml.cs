using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using PortaJel_Blazor.Pages.Connection;
using PortaJel_Blazor.Resources.Themes;

namespace PortaJel_Blazor;

public partial class App : Application
{
	[Obsolete("Obsolete")]
	public App()
	{
		InitializeComponent();
		Resources.MergedDictionaries.Add(new DarkTheme());
		Resources = new DarkTheme();
		if (MauiProgram.Server.GetServers().Length > 0)
		{
			MainPage = new MainPage();
		}
		else
		{
			MainPage = new AddServerView();
		}
    }
}
