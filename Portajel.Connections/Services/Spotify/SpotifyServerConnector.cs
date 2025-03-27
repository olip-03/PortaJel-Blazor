using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Enum;

namespace Portajel.Connections.Services.Spotify
{
    public class SpotifyServerConnector : IMediaServerConnector
    {
        public IMediaDataConnector Album { get; set; } = null;
        public IMediaDataConnector Artist { get; set; } = null;
        public IMediaDataConnector Song { get; set; } = null;
        public IMediaDataConnector Playlist { get; set; } = new SpotifyServerPlaylistConnector();
        public IMediaDataConnector Genre { get; set; } = null;

        public Dictionary<string, IMediaDataConnector> GetDataConnectors() => new()
        {
            { "Album", Album },
            { "Artist", Artist },
            { "Song", Song },
            { "Playlist", Playlist },
            { "Genre", Genre }
        };

        public Dictionary<MediaTypes, bool> SupportedReturnTypes { get; set; } = new Dictionary<MediaTypes, bool>
        {
            { MediaTypes.Playlist, true }
        };
        public string Name { get; } = "Spotify";
        public string Description { get; } = "Enables connections to Spotify.";
        public string Image { get; } = "icon_spotify.png"; 
        public Dictionary<string, ConnectorProperty> Properties { get; set; } =new Dictionary<string, ConnectorProperty>
        {
            {
                "Username", new ConnectorProperty(
                    label: "Username",
                    description: "The username of the Spotify User",
                    value: "",
                    protectValue: false,
                    userVisible: true)
            },
            {
                "Password", new ConnectorProperty(
                    label: "Password",
                    description: "The Password of the Spotify User",
                    value: "",
                    protectValue: false,
                    userVisible: true)
            }
        };

        public SyncStatusInfo SyncStatus { get; set; } = new();
        public Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateDb(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default)
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
        
        public MediaServerConnection GetConnectionType()
        {
            return MediaServerConnection.Spotify;
        }

        public SpotifyServerConnector() 
        {
            
        }
    }
}
