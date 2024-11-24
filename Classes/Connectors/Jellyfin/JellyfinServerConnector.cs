using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using PortaJel_Blazor.Classes.Connectors.Database;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin
{
    public class JellyfinServerConnector : IMediaServerConnector
    {
        private UserDto _userDto;
        private SessionInfoDto _sessionInfo;
        private JellyfinSdkSettings _sdkClientSettings;
        private JellyfinApiClient _jellyfinApiClient;
        public IMediaDataConnector Album { get; set; }
        public IMediaDataConnector Artist { get; set; }
        public IMediaDataConnector Song { get; set; }
        public IMediaDataConnector Playlist { get; set; }
        public IMediaDataConnector Genre { get; set; }
        public Dictionary<string, IMediaDataConnector> GetDataConnectors() => new()
        {
            { "Album", Album },
            { "Artist", Artist },
            { "Song", Song },
            { "Playlist", Playlist },
            { "Genre", Genre }
        };

        public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; } =
            new()
            {
                { ConnectorDtoTypes.Album, true },
                { ConnectorDtoTypes.Artist, true },
                { ConnectorDtoTypes.Song, true },
                { ConnectorDtoTypes.Playlist, true },
                { ConnectorDtoTypes.Genre, true },
            };

        public Dictionary<string, ConnectorProperty> Properties { get; set; } =
            new()
            {
                {
                    "URL", new ConnectorProperty(
                        label: "Url",
                        description: "The URL of the Jellyfin Server",
                        value: "",
                        protectValue: false)
                },
                {
                    "Username", new ConnectorProperty(
                        label: "Username",
                        description: "Username for server at Url.",
                        value: "",
                        protectValue: false)
                },
                {
                    "Password", new ConnectorProperty(
                        label: "Password",
                        description: "User password for server at Url.",
                        value: "",
                        protectValue: true)
                }
            };

        public SyncStatusInfo SyncStatus { get; set; } = new();

        public JellyfinServerConnector(string url = "", string username = "", string password = "")
        {
            Properties["URL"].Value = url;
            Properties["Username"].Value = username;
            Properties["Password"].Value = password;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddHttpClient("Default", c =>
                {
                    c.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(
                            MauiProgram.ApplicationName,
                            MauiProgram.ApplicationClientVersion));
                    c.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0));
                    c.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                })
                .ConfigurePrimaryHttpMessageHandler(_ => new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
                });

            // Add Jellyfin SDK services.
            // include support for session.SupportsRemoteControl
            // See lines 326 for what Jellyfin-Web wants from clients, for remote functionality https://github.com/jellyfin/jellyfin-web/blob/e5df4dd56bc180dfa24a52a99c718459a4074d56/src/controllers/dashboard/dashboard.js#L324 
            serviceCollection.AddSingleton<JellyfinSdkSettings>();
            serviceCollection.AddSingleton<IAuthenticationProvider, JellyfinAuthenticationProvider>();
            serviceCollection.AddScoped<IRequestAdapter, JellyfinRequestAdapter>(s => new JellyfinRequestAdapter(
                s.GetRequiredService<IAuthenticationProvider>(),
                s.GetRequiredService<JellyfinSdkSettings>(),
                s.GetRequiredService<IHttpClientFactory>().CreateClient("Default")));
            serviceCollection.AddScoped<JellyfinApiClient>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            _jellyfinApiClient = serviceProvider.GetRequiredService<JellyfinApiClient>();
            _sdkClientSettings = serviceProvider.GetRequiredService<JellyfinSdkSettings>();
            _sdkClientSettings.SetServerUrl(Properties["URL"].Value.ToString());
            _sdkClientSettings.Initialize(
                MauiProgram.ApplicationName,
                MauiProgram.ApplicationClientVersion,
                DeviceInfo.Current.Name,
                DeviceInfo.Current.Idiom.ToString());

            var authenticationResult = await _jellyfinApiClient.Users.AuthenticateByName.PostAsync(
                new AuthenticateUserByName
                {
                    Username = Properties["Username"].Value.ToString(),
                    Pw = Properties["Password"].Value.ToString()
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (authenticationResult != null)
            {
                _sdkClientSettings.SetAccessToken(authenticationResult.AccessToken);
                _userDto = authenticationResult.User;
                _sessionInfo = authenticationResult.SessionInfo;
            }

            Album = new JellyfinServerAlbumConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Artist = new JellyfinServerArtistConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Song = new JellyfinServerSongConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Playlist = new JellyfinServerPlaylistConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Genre = new JellyfinServerGenreConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);

            return AuthenticationResponse.Ok();
        }

        public async Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
        {
            if (MauiProgram.Database.Album is not DatabaseAlbumConnector albumDb) return false;
            if (MauiProgram.Database.Artist is not DatabaseArtistConnector artistDb) return false;
            if (MauiProgram.Database.Song is not DatabaseSongConnector songDb) return false;
            if (MauiProgram.Database.Playlist is not DatabasePlaylistConnector playlistDb) return false;
            
            int albumDbT = await albumDb.GetTotalCountAsync(cancellationToken: cancellationToken);
            int albumT = await Album.GetTotalCountAsync(cancellationToken: cancellationToken);

            int artistDbT = await artistDb.GetTotalCountAsync(cancellationToken: cancellationToken);
            int artistT = await Artist.GetTotalCountAsync(cancellationToken: cancellationToken);

            int songDbT = await songDb.GetTotalCountAsync(cancellationToken: cancellationToken);
            int songT = await Song.GetTotalCountAsync(cancellationToken: cancellationToken);

            int playlistDbT = await playlistDb.GetTotalCountAsync(cancellationToken: cancellationToken);
            int playlistT = await Playlist.GetTotalCountAsync(cancellationToken: cancellationToken);

            int passCount = 0;
            if (albumDbT == albumT) passCount += 1;
            if (artistDbT == artistT) passCount += 1;
            if (songDbT == songT) passCount += 1;
            if (playlistDbT == playlistT) passCount += 1;
            
            // If sync date is less than 18hrs do not sync! 
            string oauthToken = await SecureStorage.Default.GetAsync("syncdate");
            if (oauthToken != null && passCount >= 1)
            {
                if (DateTime.TryParse(oauthToken, out var lastSyncDate))
                {
                    TimeSpan timeSinceLastSync = DateTime.Now - lastSyncDate;
                    if (timeSinceLastSync.TotalHours < 18)
                    {
                        Trace.Write("Last sync occured too recently. Cancelling sync.");
                        foreach (var data in GetDataConnectors())
                        {
                            data.Value.SetSyncStatusInfo(TaskStatus.RanToCompletion, 100);
                        }
                        return true;
                    }
                }
            }
            
            if (albumDbT != albumT) return false;
            if (artistDbT != artistT) return false;
            if (songDbT != songT) return false;
            if (playlistDbT != playlistT) return false;

            return true;
        }
        
        // Todo: Stupid as fuck idea, but, I think I should try it anyway...
        // Say we want to get all the items on the server? Fortunately, the JellyFin api allows us to specify IDs we 
        // want to collect, and IDs we want to exclude... What if we sent a request excluding EVERY ID we already have 
        // on file. If we're still calling get-all, technically that should return us with all the items that... we 
        // don't already have !!!! Excellent! Give that a shot tonight, see if you can come up with something that 
        // works. 
        public async Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default)
        {
            bool upToDate = await IsUpToDateAsync(cancellationToken);
            if (upToDate) return true;
            try
            {
                if (MauiProgram.Database.Album is not DatabaseAlbumConnector albumDb) return false;
                if (MauiProgram.Database.Artist is not DatabaseArtistConnector artistDb) return false;
                if (MauiProgram.Database.Song is not DatabaseSongConnector songDb) return false;
                if (MauiProgram.Database.Playlist is not DatabasePlaylistConnector playlistDb) return false;
                if (MauiProgram.Database.Genre is not DatabaseGenreConnector genreDb) return false;
                // if (MauiProgram.Database.Genre is not DatabaseGenreConnector genreDb) return false;
                var tasks = GetDataConnectors().Select(data => Task.Run(() =>
                {
                    IMediaDataConnector dbConnector = null;
                    int batchSize = 100;
                    int currentFetch = 0;
                    int currentItem = 0;
                    
                    switch (data.Key)
                    {
                        case "Album":
                            batchSize = 100;
                            dbConnector = albumDb;
                            break;
                        case "Artist":
                            batchSize = 10;
                            dbConnector = artistDb;
                            break;
                        case "Song":
                            batchSize = 1000;
                            dbConnector = songDb;
                            break;
                        case "Playlist":
                            batchSize = 10;
                            dbConnector = playlistDb;
                            break;
                        case "Genre":
                            batchSize = 100;
                            dbConnector = genreDb;
                            break;
                    }

                    if (dbConnector == null) return;
                    
                    // Start the stopwatch
                    var stopwatch = Stopwatch.StartNew();
                    Trace.WriteLine($"Jellyfin {data.Key} Sync Started");
                    try
                    {
                        data.Value.SetSyncStatusInfo(TaskStatus.Running, 0);
                        var totalTask = data.Value.GetTotalCountAsync(cancellationToken: cancellationToken);
                        totalTask.Wait(cancellationToken);
                        var albums = albumDb.GetAllAsync(cancellationToken: cancellationToken).Result.ToList();
                        List<BaseMusicItem> serverItems = [];
                        
                        int totalItem = totalTask.Result;
                        while (true)
                        {
                            // Calculate and display progress
                            int progress = (int)((double)currentFetch / totalItem * 100);
                            data.Value.SetSyncStatusInfo(TaskStatus.Running, progress);

                            // Get items
                            var itemTask = data.Value.GetAllAsync(limit: batchSize, startIndex: currentItem,
                                setSortTypes: ItemSortBy.Name, setSortOrder: SortOrder.Ascending,
                                cancellationToken: cancellationToken);
                            itemTask.Wait(cancellationToken);
                            
                            currentFetch += itemTask.Result.Length;
                            currentItem += batchSize;

                            try
                            {
                                dbConnector.AddRange(serverItems.ToArray(), cancellationToken).Wait(cancellationToken);
                                serverItems.AddRange(itemTask.Result);
                            
                                if (itemTask.Result.Length < batchSize - 1)
                                {
                                    break; // Exit when no more data
                                }
                            }
                            catch (Exception e)
                            {
                                Trace.WriteLine(e);
                            }
                        }
                        
                        // Find albums that are not in newAlbums (itemTask.Result)
                        var albumsToDelete = albums
                            .Where(existingAlbum => serverItems.All(newAlbum => newAlbum.Id != existingAlbum.Id))
                            .ToList();
                        // Delete the albums not present in newAlbums
                        foreach (var album in albumsToDelete)
                        {
                            if (album is Album a)
                            {
                                albumDb.DeleteAsync(a.Id, cancellationToken: cancellationToken)
                                    .Wait(cancellationToken);
                            }
                        }

                        if (albumsToDelete.Count > 0)
                        {
                            Trace.WriteLine(
                                $"Deleted {albumsToDelete.Count} {data.Key} that were not in the new album list.\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        data.Value.SetSyncStatusInfo(TaskStatus.Faulted, 100);
                        Trace.WriteLine($"Error during Jellyfin {data.Key} Sync: {ex.Message}\n");
                         throw;
                    }
                    finally
                    {
                        // Stop the stopwatch and log the elapsed time
                        data.Value.SetSyncStatusInfo(TaskStatus.RanToCompletion, 100);
                        stopwatch.Stop();
                        Trace.WriteLine($"Jellyfin {data.Key} Sync finished in {stopwatch.ElapsedMilliseconds} ms\n");
                    }
                }, cancellationToken));

                await Task.WhenAll(tasks);
                Trace.WriteLine("Jellyfin Sync Finished! Saving last date!");
                await SecureStorage.Default.SetAsync("syncdate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return false;
            }
        }


        public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            await Task.Delay(10);
            return false;
        }

        public Task<BaseMusicItem[]> SearchAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<BaseMusicItem>());
        }

        public string GetUsername()
        {
            return (string)Properties["Username"].Value;
        }

        public string GetPassword()
        {
            return (string)Properties["Password"].Value;
        }

        public string GetAddress()
        {
            return (string)Properties["URL"].Value;
        }

        public string GetProfileImageUrl()
        {
            return "";
        }

        public UserCredentials GetUserCredentials()
        {
            return new UserCredentials(_sdkClientSettings.ServerUrl, (string)Properties["Username"].Value,
                _userDto.Id.ToString(), (string)Properties["Password"].Value, _sessionInfo.Id,
                _sdkClientSettings.AccessToken);
        }

        public MediaServerConnection GetConnectionType()
        {
            return MediaServerConnection.Jellyfin;
        }
    }
}