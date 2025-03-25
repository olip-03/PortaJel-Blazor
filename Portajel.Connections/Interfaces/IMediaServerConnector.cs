using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Services;
using PortaJel_Blazor.Classes;

namespace Portajel.Connections.Interfaces
{
    public interface IMediaServerConnector
    {
        Dictionary<string, IMediaDataConnector> GetDataConnectors();
        public string Name { get; }
        public string Description { get; }
        public string Image { get; }
        Dictionary<MediaTypes, bool> SupportedReturnTypes { get; set; }
        public Dictionary<string, ConnectorProperty> Properties { get; set; }
        public SyncStatusInfo SyncStatus { get; set; }
        Task<AuthResponse> AuthenticateAsync(
            CancellationToken cancellationToken = default);
        Task<bool> IsUpToDateAsync(
            CancellationToken cancellationToken = default);
        Task<bool> BeginSyncAsync(
            CancellationToken cancellationToken = default);
        Task<bool> SetIsFavourite(
            Guid id, 
            bool isFavourite,
            string serverUrl);
        public Task<BaseMusicItem[]> SearchAsync(
            string searchTerm = "", 
            int? limit = null, 
            int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name, 
            SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default);
        string GetUsername();
        string GetPassword();
        string GetAddress();
        string GetProfileImageUrl();
        UserCredentials GetUserCredentials();
        MediaServerConnection GetConnectionType();
    }
}
