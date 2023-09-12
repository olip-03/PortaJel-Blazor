using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using Jellyfin.Sdk;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Shared;
using System.Text.Json;

namespace PortaJel_Blazor;

public static class MauiProgram
{
    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "usrdata.bin";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);

    public static bool isConnected = false;
	public static bool loginPage = true;

    public static bool contentPage = false;
    public static string contentTitle = "";

    public static MainLayout mainLayout = null;

    public static List<ServerConnecter> servers = new List<ServerConnecter>();

    public static MediaElement mediaElement = null;

    // Index page cached data
    public static Album[] favouritesPlayData { get; set; }
    public static Album[] recentPlayData { get; set; }
    public static Album[] mostPlayData { get; set; }
    public static Album[] recentAddedData { get; set; }

    public static MauiApp CreateMauiApp()
	{
        // Load servers from file
        if(File.Exists(filePath))
        {
            using (BinaryReader binReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                {
                    ServerConnecter serverConnector = new ServerConnecter(Microsoft.Maui.Devices.DeviceInfo.Current.Name, Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString(), "PortaJel", "0.0.1");

                    string url = binReader.ReadString();
                    string user = binReader.ReadString();
                    string pass = binReader.ReadString();

                    serverConnector.SetBaseAddress(url);
                    serverConnector.SetUserDetails(user, pass);

                    servers.Add(serverConnector);
                }
            }
        }

        // Check connection to server
        isConnected = false;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
        builder.Services.AddTransient<GoBack>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AppSharedState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	/// <summary>
	/// Adds a server to the list of avaliable servers, and saves it to the device.
	/// </summary>
	/// <param name="server"></param>
	public static void AddServer(ServerConnecter server)
	{
        // Saves the server to file
        servers.Add(server);

        using (BinaryWriter binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            foreach (var item in servers)
            {
                // Write string
                binWriter.Write(item.GetBaseAddress());
                binWriter.Write(item.GetUsername());
                binWriter.Write(item.GetPassword());
            }
        }
    }
    /// <summary>
    /// Loads aaalll the fucking data
    /// </summary>
    public static async Task<bool> LoadAllData()
    {
        await Task.Run(() =>
        {
            Thread.Sleep(2000);
        });
        return true;
    }
}

// https://github.com/villagra/jellyfin-apiclient-dotnet