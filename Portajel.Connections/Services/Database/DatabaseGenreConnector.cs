using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Database;
using Portajel.Connections.Interfaces;
using SQLite;

namespace Portajel.Connections.Services.Database;

public class DatabaseGenreConnector : IMediaDataConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabaseGenreConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
    }

    public Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem[]>([]);
    }

    public Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem>(new Genre());
    }

    public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem[]>([]);
    }

    public Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> AddRange(BaseMusicItem[] musicItems, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}