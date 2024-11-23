using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin;

public class JellyfinServerGenreConnector(JellyfinApiClient api, JellyfinSdkSettings clientSettings, UserDto user) : IMediaDataConnector
{
    public SyncStatusInfo SyncStatusInfo { get; set; } = new();

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        SyncStatusInfo.TaskStatus = status;
        SyncStatusInfo.StatusPercentage = percentage;
    }

    public Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem[]>([]);
    }

    public Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem>(new Album());
    }

    public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem[]>([]);
    }

    public Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<int>(0);
    }

    public Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<bool>(false);
    }
}
