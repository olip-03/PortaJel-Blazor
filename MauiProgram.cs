using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Jellyfin.Sdk;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using PortaJel_Blazor.Shared;
using System.Collections.Generic;
using System.Text.Json;
using PortaJel_Blazor.Pages;
using System.Threading;
using System.Diagnostics;
using MatBlazor;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Connectors.Database;

namespace PortaJel_Blazor;

// Notes for me to go over when I can be bothered to fix things
// https://github.com/villagra/jellyfin-apiclient-dotnet
// https://www.sharpnado.com/xamarin-forms-maui-collectionview-performance-the-10-golden-rule/

public static class MauiProgram
{
    public static string ApplicationName { get; private set; } = "PortaJel";
    public static string ApplicationClientVersion { get; private set; } = "0.0.1";

    private static readonly string MainDir = FileSystem.Current.AppDataDirectory;
    private const string FileName = "usrdata.bin";
    private static readonly string FilePath = System.IO.Path.Combine(MainDir, FileName);
    public static bool DataLoadFinished = false;

    public static StyleSettings styleSettings = new();

    public static bool debugMode = true;
    public static bool isConnected = false;
    public static bool firstLoginComplete = false;
    public static bool webViewInitalized = false;
	public static bool showContent = false;

    public static bool contentPage = false;
    public static string contentTitle = "";

    public static MainLayout WebView = new();
    public static MainPage MainPage = new(initialize: false);
    public static double SystemHeaderHeight = 48;
    public static double systemWidth = 0;
    public static Action? ViewHeaderCloseSelectCallback = null;

    public static ServerConnector Server { get; private set; } = new();
    public static DatabaseConnector Database { get; private set; } = new();
    public static IMediaInterface MediaService { get; set; } = null;
    public static DownloadService DownloadManager { get; set; } = new();

    public static Guid currentAlbumGuid = Guid.Empty;
    public static Guid currentSongGuid = Guid.Empty;

    // Data for context menu
    public static List<ContextMenuItem> ContextMenuTaskList = new List<ContextMenuItem>();

    // Data for MusicPlayer
    public static bool MusicPlayerIsOpen = false;
    public static bool MusicPlayerIsQueueOpen = false;

    // Data for connections 
    public static List<NotAServerConnecter> servers = new List<NotAServerConnecter>();
    public static DataConnector api = new();
    
    // Direct access to the media element
    // public static BlazorWebView webView = null;

    // Stored data for Home Page 
    public static HomeCache homeCache = HomeCache.Empty;

    // Stored data for Search Page
    public static Dictionary<Guid, BaseMusicItem> recentSearchResults = new();

    // Stored data for library page
    public static CancellationTokenSource? libraryDataCToken = null;
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
    
    public static MauiApp CreateMauiApp()
	{
        Console.WriteLine("CreateMauiApp(): Beginning load data");

        Console.WriteLine("CreateMauiApp(): Building application");
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Check connection to server
        isConnected = false;
        
        builder.Services.AddMatBlazor();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<JsInteropClasses2, JsInteropClasses2>();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
	}

    /// <summary>
    /// Thread-safe method of updating debug messages avalialbe on the UI 
    /// </summary>
    /// <param name="message">Debug message to display</param>
    public static void UpdateDebugMessage(string message)
    {
        if (!debugMode) return;
        Trace.WriteLine(message);
        if (Application.Current != null)
        {
            Application.Current.Dispatcher.Dispatch(() => MainPage.UpdateDebugText(message));
        }
    }

	/// <summary>
	/// Adds a notAServer to the list of avaliable servers, and saves it to the device.
	/// </summary>
	/// <param name="notAServer"></param>
	public static async void AddServer(NotAServerConnecter notAServer)
	{
        // Saves the notAServer to file
        servers.Add(notAServer); // TODO: depreciate
        api.AddServer(notAServer);

        await SaveData();
    }
    public static async void RemoveServer(NotAServerConnecter notAServer)
    {
        // Saves the notAServer to file
        servers.Remove(notAServer); // TODO: depreciate
        api.RemoveServer(notAServer);

        await SaveData();
    }
    public static async void RemoveServer(string server)
    {
        NotAServerConnecter[] enumerate = servers.ToArray();
        // Saves the server to file
        foreach (NotAServerConnecter srv in enumerate)
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
        MauiProgram.UpdateDebugMessage("Starting data load");

        servers = new List<NotAServerConnecter>();
        api = new DataConnector();

        if (File.Exists(FilePath))
        {
            try
            {
                using (BinaryReader binReader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
                {
                    while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                    {
                        // LOAD SERVER COUNT
                        int srvCount = binReader.ReadInt32();

                        // LOAD CONNECTIONS
                        for (int i = 0; i < srvCount; i++)
                        {

                            string url = binReader.ReadString();
                            string user = binReader.ReadString();
                            string pass = binReader.ReadString();

                            MauiProgram.UpdateDebugMessage($"Loaded info for {url}");

                            NotAServerConnecter notAServerConnector = new NotAServerConnecter(url, user, pass);

                            servers.Add(notAServerConnector);
                            api.AddServer(notAServerConnector);
                            firstLoginComplete = true;
                        }

                        // LOAD FIRSTLOGIN DATA
                        firstLoginComplete = binReader.ReadBoolean();
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(FilePath);
            }
        }

        // Validate connection data and log into servers
        await Parallel.ForEachAsync(api.GetServers(), async (server, ct) => {
            firstLoginComplete = true; // double-check set true if any servers exist 
            try
            {
                bool UserPassed = await server.AuthenticateServerAsync();
                if (UserPassed)
                {
                    MauiProgram.UpdateDebugMessage($"Successfully logged into {server}");
                }
                else
                {
                    MauiProgram.UpdateDebugMessage($"Login failed for {server}");
                }
            }
            catch (Exception)
            {
                MauiProgram.UpdateDebugMessage($"Login failed for {server}");
            }
        }).ConfigureAwait(false);

        MauiProgram.UpdateDebugMessage($"Completed data load");
        DataLoadFinished = true;
        return true;
    }
    public static Task<bool> SaveData()
    {
        // TODO: Change to json file type
        using (BinaryWriter binWriter = new BinaryWriter(File.Open(FilePath, FileMode.Create)))
        {
            // SAVE SERVER COUNT
            binWriter.Write(api.GetServers().Count());

            // SAVE CONNECTIONS
            foreach (NotAServerConnecter srv in api.GetServers())
            {
                // Write string
                binWriter.Write(srv.GetBaseAddress());
                binWriter.Write(srv.GetUsername());
                binWriter.Write(srv.GetPassword());
            }
            // SAVE FIRSTLOGIN DATA
            binWriter.Write(firstLoginComplete);
        }
        return Task.FromResult(true);
    }
}