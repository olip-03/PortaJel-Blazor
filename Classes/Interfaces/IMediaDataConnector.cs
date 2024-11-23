using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces;

public interface IMediaDataConnector
{
    public SyncStatusInfo SyncStatusInfo { get; set; }
    public void SetSyncStatusInfo(TaskStatus status, int percentage);
    Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default);

    Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);

    Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);
}