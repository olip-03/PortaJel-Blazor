using System.Diagnostics;
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
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin
{
    public class JellyfinServerConnector : IMediaServerConnector
    {
        private UserDto _userDto;
        private SessionInfoDto _sessionInfo;
        private JellyfinSdkSettings _sdkClientSettings;
        private JellyfinApiClient _jellyfinApiClient;
        public IMediaServerAlbumConnector Album { get; set; }
        public IMediaServerArtistConnector Artist { get; set; }
        public IMediaServerSongConnector Song { get; set; }
        public IMediaServerPlaylistConnector Playlist { get; set; }
        public IMediaServerGenreConnector Genre { get; set; }
		
        public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; } =
            new Dictionary<ConnectorDtoTypes, bool>
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

        public TaskStatus SyncStatus { get; set; } = TaskStatus.WaitingToRun;
        
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
                Microsoft.Maui.Devices.DeviceInfo.Current.Name,
                Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString());

            Album = new JellyfinServerAlbumConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Artist = new JellyfinServerArtistConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Song = new JellyfinServerSongConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Playlist = new JellyfinServerPlaylistConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            Genre = new JellyfinServerGenreConnector(_jellyfinApiClient, _sdkClientSettings, _userDto);
            
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
                return AuthenticationResponse.Ok();
            }
            return new AuthenticationResponse(); // TODO: Update with legit FAIL auth message
        }
        
        public Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        public Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            throw new NotImplementedException();
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
            return new UserCredentials(_sdkClientSettings.ServerUrl,  (string)Properties["Username"].Value, _userDto.Id.ToString(), (string)Properties["Password"].Value, _sessionInfo.Id, _sdkClientSettings.AccessToken);
        }
            
        public MediaServerConnection GetType()
        {
            return MediaServerConnection.Jellyfin;
        }
    }
}