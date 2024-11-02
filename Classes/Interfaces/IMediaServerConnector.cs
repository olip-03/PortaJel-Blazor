using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerConnector
    {
        public IMediaServerAlbumConnector Album { get; set; } 
        public IMediaServerArtistConnector Artist { get; set; }
        public IMediaServerSongConnector Song { get; set; }
        public IMediaServerPlaylistConnector Playlist { get; set; }
        public IMediaServerGenreConnector Genre { get; set; }
        Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; }
        Dictionary<string, ConnectorProperty> Properties { get; set; }
        public TaskStatus SyncStatus { get; set; }
        Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default);
        Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default);
        Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default);
        string GetUsername();
        string GetPassword();
        string GetAddress();
    }
}
