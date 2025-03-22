using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;

namespace Portajel.Connections.Interfaces;

public interface IMediaDataConnector
{
    public SyncStatusInfo SyncStatusInfo { get; set; }
    public void SetSyncStatusInfo(TaskStatus status, int percentage);
    Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default);

    Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);

    Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync( bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid[] id, string serverUrl = "", CancellationToken cancellationToken = default);


    Task<bool> AddRange(BaseMusicItem[] musicItems, CancellationToken cancellationToken = default);
}