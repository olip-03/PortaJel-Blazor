using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerConnector
    {
        Dictionary<string, IMediaDataConnector> GetDataConnectors();
        Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; }
        public Dictionary<string, ConnectorProperty> Properties { get; set; }
        public SyncStatusInfo SyncStatus { get; set; }
        Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default);
        Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default);
        Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default);
        Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl);
        public Task<BaseMusicItem[]> SearchAsync(CancellationToken cancellationToken = default);
        string GetUsername();
        string GetPassword();
        string GetAddress();
        string GetProfileImageUrl();
        UserCredentials GetUserCredentials();
        MediaServerConnection GetConnectionType();
    }
}
