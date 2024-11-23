using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors.Discogs;

// https://github.com/David-Desmaisons/DiscogsClient

public class DiscogsConnector : IMediaServerConnector
{
    public IMediaDataConnector Album { get; set; }
    public IMediaDataConnector Artist { get; set; }
    public IMediaDataConnector Song { get; set; }
    public IMediaDataConnector Playlist { get; set; }
    public IMediaDataConnector Genre { get; set; }
    public Dictionary<string, IMediaDataConnector> GetDataConnectors()=> new()
    {
        { "Album", Album },
        { "Artist", Artist },
        { "Song", Song },
        { "Playlist", Playlist },
        { "Genre", Genre }
    };

    public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; }
    public Dictionary<string, ConnectorProperty> Properties { get; set; }
    public SyncStatusInfo SyncStatus { get; set; } = new();
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

    public Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
    {
        throw new NotImplementedException();
    }

    public Task<BaseMusicItem[]> SearchAsync(CancellationToken cancellationToken = default)
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

    public string GetProfileImageUrl()
    {
        throw new NotImplementedException();
    }

    public UserCredentials GetUserCredentials()
    {
        throw new NotImplementedException();
    }

    public MediaServerConnection GetConnectionType()
    {
        throw new NotImplementedException();
    }
}