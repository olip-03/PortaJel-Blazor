using Jellyfin.Sdk;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
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

    public static MainLayout mainLayout = new();
    public static MainPage mainPage = new();

    public static MediaService mediaService = new MediaService();

    // Global Data Cache
    public static Dictionary<Guid, Song> songDictionary = new Dictionary<Guid, Song>();
    public static Dictionary<Guid, Album> albumDictionary = new Dictionary<Guid, Album>();
    public static Dictionary<Guid, Artist> artistDictionary = new Dictionary<Guid, Artist>();

    // Data for context menu
    public static List<ContextMenuItem> ContextMenuTaskList = new List<ContextMenuItem>();
    public static bool ShowContextMenuImage = true;
    public static bool ContextMenuIsOpen = false;
    public static string ContextMenuImage = String.Empty;
    public static string ContextMenuMainText = String.Empty;
    public static string ContextMenuSubText = String.Empty;

    // Data for MusicPlayer
    public static bool MusicPlayerIsOpen = false;
    public static bool MusicPlayerIsQueueOpen = false;

    // Data for connections 
    public static List<ServerConnecter> servers = new List<ServerConnecter>();
    public static DataHandler? dataHandler = null;

    // Direct access to the media element
    public static BlazorWebView webView = null;

    // Stored data for library page
    public static int librarySortMethod = 0;
    public static int libraryItemView = 0;
    public static bool libraryShowGrid = false;

    // Stored data for favourites page
    public static int favouritesSortMethod = 0;
    public static int favouritesItemView = 0;
    public static bool favouritesShowGrid = false;
    public static bool hideM3u = true;
    public static List<Album> favouritePlaylists = new();
    public static List<Album> favouriteAlbums = new();
    public static List<Album> favouriteArtist = new();
    public static List<Album> favouriteSongs = new();

    // Index page cached data
    public static Album[]? favouritesPlayData { get; set; } = null;
    public static Album[]? recentPlayData { get; set; } = null;
    public static Album[]? mostPlayData { get; set; } = null;
    public static Album[]? recentAddedData { get; set; } = null;

    public static MauiApp CreateMauiApp()
	{
        if(dataHandler == null)
        {
            dataHandler = new DataHandler();
        }

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // File.Delete(filePath);
        // Load servers from file
        if (File.Exists(filePath))
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
                    dataHandler.AddServer(serverConnector);
                }
            }
        }

        // Check connection to server
        isConnected = false;

		builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<JsInteropClasses2, JsInteropClasses2>();
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
        
        return true;
    }

}

// https://github.com/villagra/jellyfin-apiclient-dotnet