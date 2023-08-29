using Microsoft.Extensions.Logging;
using PortaJel_Blazor.Classes;

namespace PortaJel_Blazor;

public static class MauiProgram
{
	public static bool isConnected = false;
	public static bool loginPage = true;
	public static ServerConnecter serverConnector;
	public static MauiApp CreateMauiApp()
	{
		serverConnector = new ServerConnecter();

        // Check connection to server
        isConnected = false;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

// https://github.com/villagra/jellyfin-apiclient-dotnet