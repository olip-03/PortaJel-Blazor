using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using SQLite;
using FileSystem = Microsoft.VisualBasic.FileSystem;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemConnector  : IMediaServerConnector
{
    private SQLiteAsyncConnection _database = null;

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

    public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; }
    
    public Dictionary<string, ConnectorProperty> Properties { get; set; } =new Dictionary<string, ConnectorProperty>
    {
        {
            "Paths", new ConnectorProperty(
                label: "Music Directories",
                description: "The directories of music files in your file system.",
                value: new List<string>(),
                protectValue: false)
        }
    };

    public SyncStatusInfo SyncStatus { get; set; } = new();

    public FileSystemConnector()
    {
        
    }
    
    public FileSystemConnector(SQLiteAsyncConnection database, List<string> paths)
    {
        Properties["Paths"].Value = paths;
        _database = database;
        
        Album = new FileSystemAlbumConnector(_database);
        Artist = new FileSystemArtistConnector(_database);
        Song = new FileSystemSongConnector(_database);
        Playlist = new FileSystemPlaylistConnector(_database);
        Genre = new FileSystemGenreConnector(_database);
    }
    
    public Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AuthenticationResponse.Unneccesary());
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
    
    public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
        ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<BaseMusicItem>());
    }

    public string GetUsername()
    {
        return null;
    }
    
    public string GetPassword()
    {
        return null;
    }
    
    public string GetAddress()
    {
        return "http://localhost:5000";
    }

    public string GetProfileImageUrl()
    {
        return null;
    }

    public UserCredentials GetUserCredentials()
    {
        return new UserCredentials(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    }

    public MediaServerConnection GetConnectionType()
    {
        return MediaServerConnection.Filesystem;
    }
}