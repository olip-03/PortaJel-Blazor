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

    public IMediaServerAlbumConnector Album { get; set; }
    public IMediaServerArtistConnector Artist { get; set; }
    public IMediaServerSongConnector Song { get; set; }
    public IMediaServerPlaylistConnector Playlist { get; set; }
    public IMediaServerGenreConnector Genre { get; set; }
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

    public TaskStatus SyncStatus { get; set; } = TaskStatus.WaitingToRun;

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
    
    public Task<BaseMusicItem[]> SearchAsync(CancellationToken cancellationToken = default)
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