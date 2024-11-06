using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Spotify
{
    public class SpotifyServerConnector : IMediaServerConnector
    {
        public IMediaServerAlbumConnector Album { get; set; } = null;
        public IMediaServerArtistConnector Artist { get; set; } = null;
        public IMediaServerSongConnector Song { get; set; } = null;
        public IMediaServerPlaylistConnector Playlist { get; set; } = new SpotifyServerPlaylistConnector();
        public IMediaServerGenreConnector Genre { get; set; } = null;
        
        public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; } = new Dictionary<ConnectorDtoTypes, bool>
        {
            { ConnectorDtoTypes.Playlist, true }
        };

        public Dictionary<string, ConnectorProperty> Properties { get; set; } =new Dictionary<string, ConnectorProperty>
        {
            {
                "URL", new ConnectorProperty(
                    label: "Url",
                    description: "The URL of the Spotify api",
                    value: "",
                    protectValue: false)
            },
            {
                "Username", new ConnectorProperty(
                    label: "Username",
                    description: "The username of the Spotify User",
                    value: "",
                    protectValue: false)
            },
            {
                "Password", new ConnectorProperty(
                    label: "Password",
                    description: "The Password of the Spotify User",
                    value: "",
                    protectValue: false)
            }
        };

        public TaskStatus SyncStatus { get; set; }  = TaskStatus.WaitingToRun;
        public Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
        
        public SpotifyServerConnector(string username, string password)
        {
            Properties["Username"].Value = username;
            Properties["Password"].Value = password;
        }
        
        
        public string GetUsername()
        {
            return "";
        }
        
        public string GetPassword()
        {
            return "";
        }
        
        public string GetAddress()
        {
            return "";
        }

        public string GetProfileImageUrl()
        {
            throw new NotImplementedException();
        }

        public UserCredentials GetUserCredentials()
        {
            return new UserCredentials(Properties["Url"].Value.ToString(), Properties["Username"].Value.ToString(), string.Empty, Properties["Password"].Value.ToString(), string.Empty, string.Empty);
        }
        
        public MediaServerConnection GetType()
        {
            return MediaServerConnection.Spotify;
        }

        public SpotifyServerConnector() 
        {
            
        }
    }
}
