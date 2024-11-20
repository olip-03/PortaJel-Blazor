using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors.Discogs;

// https://github.com/David-Desmaisons/DiscogsClient

public class DiscogsConnector : IMediaServerConnector
{
    public IMediaServerAlbumConnector Album { get; set; }
    public IMediaServerArtistConnector Artist { get; set; }
    public IMediaServerSongConnector Song { get; set; }
    public IMediaServerPlaylistConnector Playlist { get; set; }
    public IMediaServerGenreConnector Genre { get; set; }
    public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; }
    public Dictionary<string, ConnectorProperty> Properties { get; set; }
    public TaskStatus SyncStatus { get; set; }
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