﻿using System.Diagnostics;
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

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin
{
    public class JellyfinServerConnector : IMediaServerConnector
    {
        private UserDto _userDto;
        private SessionInfo _sessionInfo;
        private readonly JellyfinSdkSettings _sdkClientSettings;
        private readonly JellyfinApiClient _jellyfinApiClient;
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
            new Dictionary<string, ConnectorProperty>
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
                        label: "Url",
                        description: "The URL of the Jellyfin Server",
                        value: "",
                        protectValue: false)
                },
                {
                    "Password", new ConnectorProperty(
                        label: "Url",
                        description: "The URL of the Jellyfin Server",
                        value: "",
                        protectValue: false)
                }
            };

        public TaskStatus SyncStatus { get; set; } = TaskStatus.WaitingToRun;
        
        public JellyfinServerConnector(string url = "", string username = "", string password = "")
        {
            Properties["URL"].Value = url;
            Properties["Username"].Value = username;
            Properties["Password"].Value = password;

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
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
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
                    return new AuthenticationResponse(); // TODO: Update with legit OK auth message
                }
            }
            catch (TaskCanceledException timeout)
            {
                Trace.WriteLine(timeout.GetType());
                // SetOfflineStatus(true);
            }
            catch
            {
                // SetOfflineStatus(true);
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
    }
}