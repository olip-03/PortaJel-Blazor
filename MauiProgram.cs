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
using System.Security.Cryptography;
using System.Text;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Connectors.Database;

namespace PortaJel_Blazor;

// Notes for me to go over when I can be bothered to fix things
// https://github.com/villagra/jellyfin-Serverclient-dotnet
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
    public static bool hideM3u = true;
    
    public static MauiApp CreateMauiApp()
	{
        Console.WriteLine("CreateMauiApp(): Beginning load data");
        LoadData();
            
        Console.WriteLine("CreateMauiApp(): Building application");
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto");
                fonts.AddFont("Roboto-Medium.ttf", "RobotoRegular");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
            });

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
    /// Loads aaalll the fucking data
    /// </summary>
    public static bool LoadData()
    {
        UpdateDebugMessage("Starting data load");
        
        Task<string> t =  SecureStorage.Default.GetAsync(GetOAuth());
        t.Wait();
        string oauthToken = t.Result;

        if (oauthToken == null)
        {
            Trace.TraceError("Failed to read SaveData(): oauthToken is null!");
            return false;
        }
        
        ServerConnectorSettings settings = new(oauthToken);
        Server = settings.ServerConnector;

        UpdateDebugMessage($"Completed data load");
        DataLoadFinished = true;
        return true;
    }
    public static async Task<bool> SaveData()
    {
        try
        {
            string toSave = Server.GetSettings().ToJson();
            await SecureStorage.Default.SetAsync(GetOAuth(), toSave);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"SaveData(): {e.Message}");
            return false;
        }
        return true;
    }

    private static string GetOAuth()
    {
        using MD5 md5Hash = MD5.Create();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes($"{DeviceInfo.Current.Model}{DeviceInfo.Current.Manufacturer}{DeviceInfo.Current.Name}"));
        StringBuilder sBuilder = new StringBuilder();

        foreach (var t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        
        return sBuilder.ToString();
    }
}