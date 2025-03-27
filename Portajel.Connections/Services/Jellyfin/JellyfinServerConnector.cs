using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Enum;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Portajel.Connections.Services;
using Portajel.Connections.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PortaJel_Blazor.Classes;
using Portajel.Connections.Services.Database;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;

namespace Portajel.Connections.Services.Jellyfin
{
    public class JellyfinServerConnector : IMediaServerConnector
    {
        private DatabaseConnector _database;
        private UserDto? _userDto;
        private SessionInfoDto? _sessionInfo;
        private JellyfinSdkSettings? _sdkClientSettings;
        private JellyfinApiClient? _jellyfinApiClient;
        public IMediaDataConnector Album { get; set; } = null!;
        public IMediaDataConnector Artist { get; set; } = null!;
        public IMediaDataConnector Song { get; set; } = null!;
        public IMediaDataConnector Playlist { get; set; } = null!;
        public IMediaDataConnector Genre { get; set; } = null!;
        public Dictionary<string, IMediaDataConnector> GetDataConnectors() => new()
        {
            { "Album", Album },
            { "Artist", Artist },
            { "Song", Song },
            { "Playlist", Playlist },
            { "Genre", Genre }
        };
        public Dictionary<MediaTypes, bool> SupportedReturnTypes { get; set; } =
            new()
            {
                { MediaTypes.Album, true },
                { MediaTypes.Artist, true },
                { MediaTypes.Song, true },
                { MediaTypes.Playlist, true },
                { MediaTypes.Genre, true },
            };
        public string Name { get; } = "JellyFin";
        public string Description { get; } = "Enables connections to the Jellyfin Media Server.";
        public string Image { get; } = "icon_jellyfin.png";
        public Dictionary<string, ConnectorProperty> Properties { get; set; } = new();
        public SyncStatusInfo SyncStatus { get; set; } = new();
        public JellyfinServerConnector(
            DatabaseConnector database,
            string url = "", 
            string username = "", 
            string password = "",
            string appName = "",
            string appVerison = "",
            string deviceName = "",
            string deviceId = "")
        {
            _database = database;
            Properties =
                new()
                {
                    {
                        "AppName", new ConnectorProperty(
                            label: "App Name",
                            description: "The name of the Jellyfin Client Application.",
                            value: appName,
                            protectValue: false,
                            userVisible: true
                            )
                    },
                    {
                        "URL", new ConnectorProperty(
                            label: "Url",
                            description: "The URL of the Jellyfin Server",
                            value: url,
                            protectValue: false,
                            userVisible: true)
                    },
                    {
                        "Username", new ConnectorProperty(
                            label: "Username",
                            description: "Username for data at Url.",
                            value: username,
                            protectValue: false,
                            userVisible: true)
                    },
                    {
                        "Password", new ConnectorProperty(
                            label: "Password",
                            description: "User password for data at Url.",
                            value: password,
                            protectValue: true,
                            userVisible: true)
                    },

                    {
                        "AppVersion", new ConnectorProperty(
                            label: "App Version",
                            description: "The version of the Jellyfin Client Application.",
                            value: appVerison,
                            protectValue: true,
                            userVisible: false)
                    },
                    {
                        "DeviceName", new ConnectorProperty(
                            label: "Device Name",
                            description: "The name of the device running this Jellyfin Client Application.",
                            value: deviceName,
                            protectValue: true,
                            userVisible: false)
                    },
                    {
                        "DeviceID", new ConnectorProperty(
                            label: "Device Name",
                            description: "The name of the device running this Jellyfin Client Application.",
                            value: deviceId,
                            protectValue: true,
                            userVisible: false)
                    },
                    {
                        "LastSync", new ConnectorProperty(
                            label: "Last Sync",
                            description: "The last time a full sync ran for this data.",
                            value: url,
                            protectValue: false,
                            userVisible: false)
                    },
                };
        }

        public async Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            // At the beginning of AuthenticateAsync method, add validation
            if (Properties["AppName"].Value == null ||
                Properties["AppVersion"].Value == null ||
                Properties["DeviceName"].Value == null ||
                Properties["DeviceID"].Value == null ||
                Properties["URL"].Value == null ||
                Properties["Username"].Value == null ||
                Properties["Password"].Value == null)
            {
                return new AuthResponse()
                {
                    IsSuccess = false,
                    Message = "Missing required properties for authentication"
                };
            }
            try
            {
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddHttpClient("Default", c =>
                {
                    c.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(
                            (string)Properties["AppName"].Value,
                            (string)Properties["AppVersion"].Value));
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
                    (string)Properties["AppName"].Value,
                    (string)Properties["AppVersion"].Value,
                    (string)Properties["DeviceName"].Value,
                    (string)Properties["DeviceID"].Value);

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
            }
            catch (ApiException apiEx)
            {
                // More detailed API exception handling
                Trace.WriteLine($"Error: {apiEx.Message}");
                Trace.WriteLine($"Status code: {apiEx.ResponseStatusCode}");
                Trace.WriteLine($"Source: {apiEx.Source}");

                return new AuthResponse()
                {
                    IsSuccess = false,
                    Message = $"API Error: {apiEx.Message} (Status: {apiEx.ResponseStatusCode})"
                };
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.StackTrace);
                return new AuthResponse()
                {
                    IsSuccess = false,
                    Message = $"{ex.Message}"
                };
            }

            return AuthResponse.Ok();
        }

        public async Task<bool> UpdateDb(CancellationToken cancellationToken = default)
        {
            await UpdateSyncStatus(cancellationToken);
            var tasks = GetDataConnectors().Values.Select(data => Task.Run(async () =>
            {
                int retrieve = 50;
                int checkCount = 3;
                var dbItems = await GetDb(data).Value.GetAllAsync(
                    limit: retrieve * checkCount, 
                    setSortTypes: ItemSortBy.DateCreated);
                var dbIds = dbItems.Select(item => item.Id).ToArray();

                data.SyncStatusInfo.TaskStatus = TaskStatus.Running;
                while (data.SyncStatusInfo.TaskStatus is TaskStatus.Running)
                {
                    var items = await data.GetAllAsync(
                            limit: retrieve,
                            startIndex: data.SyncStatusInfo.ServerItemCount,
                            setSortOrder: SortOrder.Descending,
                            setSortTypes: ItemSortBy.DateCreated,
                            cancellationToken: cancellationToken
                        );

                    foreach (var item in items)
                    {
                        if (dbIds.Contains(item.Id))
                        {
                            data.SetSyncStatusInfo(DbFoundTotal: data.SyncStatusInfo.DbFoundTotal + 1);
                        }
                    }

                    data.SetSyncStatusInfo(serverItemCount: items.Length);
                    if (data.SyncStatusInfo.ServerItemCount < retrieve || 
                        data.SyncStatusInfo.DbFoundTotal > retrieve * checkCount)
                    {
                        data.SetSyncStatusInfo(status: TaskStatus.RanToCompletion);
                    }
                }
            }, cancellationToken)).ToList();
            Task t = Task.WhenAll(tasks);
            try
            {
                await t;
            }
            catch
            {
                // ignored
            }
            return true;
        }
        public async Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
        {
            int maxTasks = 500;
            await UpdateSyncStatus(cancellationToken);
            var tasks = GetDataConnectors().Values.Select(data => Task.Run(async () =>
            {
                int workers = GetDataConnectors().Values.Where(d => d.SyncStatusInfo.TaskStatus == TaskStatus.Running).Count();
                int retrieve = maxTasks / workers;
                int checkCount = 3;
                var dbItems = await GetDb(data).Value.GetAllAsync(
                    limit: retrieve * checkCount,
                    setSortTypes: ItemSortBy.DateCreated);
                var dbIds = dbItems.Select(item => item.Id).ToArray();

                data.SyncStatusInfo.TaskStatus = TaskStatus.Running;
                while (data.SyncStatusInfo.TaskStatus is TaskStatus.Running)
                {
                    var items = await data.GetAllAsync(
                            limit: retrieve,
                            startIndex: data.SyncStatusInfo.ServerItemCount,
                            setSortOrder: SortOrder.Descending,
                            setSortTypes: ItemSortBy.DateCreated,
                            cancellationToken: cancellationToken
                        );

                    int newTotal = data.SyncStatusInfo.ServerItemCount + items.Length;
                    double newPercent = ((double)newTotal / data.SyncStatusInfo.ServerItemTotal) * 100;

                    data.SetSyncStatusInfo(serverItemCount: newTotal, percentage: (int)newPercent);
                    if (items.Length < retrieve ||
                        data.SyncStatusInfo.DbFoundTotal > retrieve * checkCount)
                    {
                        data.SetSyncStatusInfo(status: TaskStatus.RanToCompletion);
                    }
                    GetDb(data).Value.InsertRangeAsync(items, cancellationToken).Wait(cancellationToken);
                    workers = GetDataConnectors().Values.Where(d => d.SyncStatusInfo.TaskStatus == TaskStatus.Running).Count();
                    retrieve = maxTasks / workers;
                }
            }, cancellationToken)).ToList();
            Task t = Task.WhenAll(tasks);
            try
            {
                await t;
                Properties["LastSync"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                // ignored
            }
            return true;
        }
        public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            await Task.Delay(10);
            return false;
        }
        public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default)
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
        private async Task<bool> UpdateSyncStatus(CancellationToken cancellationToken = default)
        {
            var tasks = GetDataConnectors().Values.Select(data => Task.Run(async () =>
            {
                data.SetSyncStatusInfo(
                    TaskStatus.Running,
                    0,
                    await data.GetTotalCountAsync(),
                    0,
                    0);
                data.GetTotalCountAsync(cancellationToken: cancellationToken).Wait(cancellationToken);
            }, cancellationToken))
            .ToList();
            Task t = Task.WhenAll(tasks);
            try
            {
                await t;
            }
            catch
            {
                // ignored
            }
            return true;
        }
        private KeyValuePair<string, IDbItemConnector> GetDb(IMediaDataConnector mediaDataConnector)
        {
            switch (mediaDataConnector.MediaType)
            {
                case MediaTypes.Album:
                    return new KeyValuePair<string, IDbItemConnector>("Album", _database.Album);
                case MediaTypes.Artist:
                    return new KeyValuePair<string, IDbItemConnector>("Artist", _database.Artist);
                case MediaTypes.Song:
                    return new KeyValuePair<string, IDbItemConnector>("Song", _database.Song);
                case MediaTypes.Playlist:
                    return new KeyValuePair<string, IDbItemConnector>("Playlist", _database.Playlist);
                case MediaTypes.Genre:
                    return new KeyValuePair<string, IDbItemConnector>("Genre", _database.Genre);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}