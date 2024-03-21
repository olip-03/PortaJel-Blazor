using Jellyfin.Sdk;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Shared;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

namespace PortaJel_Blazor;

public static class MauiProgram
{
    public static string applicationName { get; private set; } = "PortaJel";
    public static string applicationClientVersion { get; private set; } = "0.0.1";

    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "usrdata.bin";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);
    public static bool dataLoadFinished = false;

    public static StyleSettings styleSettings = new();

    public static bool isConnected = false;
    public static bool firstLoginComplete = false;
    public static bool webViewInitalized = false;
	public static bool showContent = false;

    public static bool contentPage = false;
    public static string contentTitle = "";

    public static MainLayout mainLayout = new();
    public static MainPage mainPage = new(initalize: false);

    public static MediaService mediaService = new MediaService();

    // Global Data Cache
    // TODO: Move these to the ServerConnectors
    public static Dictionary<Guid, Song> songDictionary = new Dictionary<Guid, Song>();
    public static Dictionary<Guid, Album> albumDictionary = new Dictionary<Guid, Album>();
    public static Dictionary<Guid, Artist> artistDictionary = new Dictionary<Guid, Artist>();
    public static Dictionary<Guid, Playlist> playlistDictionary = new Dictionary<Guid, Playlist>();

    public static Guid currentAlbumGuid = Guid.Empty;
    public static Guid currentSongGuid = Guid.Empty;

    // Data for context menu
    public static List<ContextMenuItem> ContextMenuTaskList = new List<ContextMenuItem>();
    public static bool ShowContextMenuImage = true;
    public static bool ContextMenuIsOpen = false;
    public static string ContextMenuImage = String.Empty;
    public static string ContextMenuMainText = String.Empty;
    public static string ContextMenuSubText = String.Empty;
    public static bool ContextMenuRoundImage = false;

    // Data for MusicPlayer
    public static bool MiniPlayerIsOpen = false;
    public static bool MusicPlayerIsOpen = false;
    public static bool MusicPlayerIsQueueOpen = false;

    // Data for connections 
    public static List<ServerConnecter> servers = new List<ServerConnecter>();
    public static DataConnector api = new();

    // Direct access to the media element
    public static BlazorWebView webView = null;

    // Stored data for Search Page
    public static List<BaseMusicItem> recentSearchResults = new();

    // Stored data for library page
    public static int librarySortMethod = 0;
    public static int libraryItemView = 0;
    public static bool libraryShowGrid = false;

    // Stored data for favourites page
    public static int favouritesSortMethod = 0;
    public static int favouritesItemView = 0;
    public static bool favouritesShowGrid = false;
    public static bool hideM3u = true;
    public static List<Playlist> favouritePlaylists = new();
    public static List<Album> favouriteAlbums = new();
    public static List<Artist> favouriteArtist = new();
    public static List<Song> favouriteSongs = new();

    // Index page cached data
    public static BaseMusicItem[] favouritesPlayData { get; set; } = new BaseMusicItem[0];
    public static BaseMusicItem[] recentPlayData { get; set; } = new BaseMusicItem[0];
    public static BaseMusicItem[] mostPlayData { get; set; } = new BaseMusicItem[0];
    public static BaseMusicItem[] recentAddedData { get; set; } = new BaseMusicItem[0];

    public static MauiApp CreateMauiApp()
	{
        Task.Run(() => MauiProgram.LoadData());

        if (api == null)
        {
            api = new DataConnector();
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
	public static async void AddServer(ServerConnecter server)
	{
        // Saves the server to file
        servers.Add(server); // TODO: depreciate
        api.AddServer(server);

        await SaveData();
    }
    public static async void RemoveServer(ServerConnecter server)
    {
        // Saves the server to file
        servers.Remove(server); // TODO: depreciate
        api.RemoveServer(server);

        await SaveData();
    }
    public static async void RemoveServer(string server)
    {
        ServerConnecter[] enumerate = servers.ToArray();
        // Saves the server to file
        foreach (ServerConnecter srv in enumerate)
        {
            if(srv.GetBaseAddress() == server)
            {
                servers.Remove(srv); // TODO: depreciate
            }
        }
        api.RemoveServer(server);
        await SaveData();
    }
    /// <summary>
    /// Loads aaalll the fucking data
    /// </summary>
    public static async Task<bool> LoadData()
    {
        servers = new List<ServerConnecter>();
        api = new DataConnector();

        if (File.Exists(filePath))
        {
            try
            {
                using (BinaryReader binReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                    {
                        // LOAD SERVER COUNT
                        int srvCount = binReader.ReadInt32();

                        // LOAD CONNECTIONS
                        for (int i = 0; i < srvCount; i++)
                        {
                            ServerConnecter serverConnector = new ServerConnecter();

                            string url = binReader.ReadString();
                            string user = binReader.ReadString();
                            string pass = binReader.ReadString();

                            serverConnector.SetBaseAddress(url);
                            serverConnector.SetUserDetails(user, pass);

                            servers.Add(serverConnector);
                            api.AddServer(serverConnector);
                            firstLoginComplete = true;
                        }

                        // LOAD FIRSTLOGIN DATA
                        firstLoginComplete = binReader.ReadBoolean();
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(filePath);
            }
        }

        // Validate connection data and log into servers
        await Parallel.ForEachAsync(api.GetServers(), async (server, ct) => {
            firstLoginComplete = true; // double-check set true if any servers exist 
            server.isOffline = !await server.AuthenticateAddressAsync();
            if (!server.isOffline)
            {
                bool UserPassed = await server.AuthenticateUser();
            }
        });

        dataLoadFinished = true;
        return true;
    }
    public static async Task<bool> SaveData()
    {
        // TODO: Change to json file type
        using (BinaryWriter binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            // SAVE SERVER COUNT
            binWriter.Write(api.GetServers().Count());

            // SAVE CONNECTIONS
            foreach (ServerConnecter srv in api.GetServers())
            {
                // Write string
                binWriter.Write(srv.GetBaseAddress());
                binWriter.Write(srv.GetUsername());
                binWriter.Write(srv.GetPassword());
            }
            // SAVE FIRSTLOGIN DATA
            binWriter.Write(firstLoginComplete);
        }
        return true;
    }
}

// https://github.com/villagra/jellyfin-apiclient-dotnet