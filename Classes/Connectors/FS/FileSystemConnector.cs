using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemConnector  : IMediaServerConnector
{
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

    public FileSystemConnector(List<string> paths)
    {
        Properties["Paths"].Value = paths;
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
    
    public string GetUsername()
    {
        throw new NotImplementedException();
    }
    
    public string GetPassword()
    {
        throw new NotImplementedException();
    }
    
    public string GetAddress()
    {
        throw new NotImplementedException();
    }
}