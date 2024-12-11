using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerPlaylistConnector(List<IMediaServerConnector> servers) : IMediaDataConnector, IMediaPlaylistInterface
{
    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid[] id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddRange(BaseMusicItem[] musicItems, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AddAsync(Guid playlistId, BaseMusicItem musicItem, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        int failed = 0;
        var tasks = servers.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    if (server.GetDataConnectors()["Playlist"] is IMediaPlaylistInterface srvPlaylistInterface)
                    {
                        var pTask = srvPlaylistInterface.AddAsync(playlistId, musicItem, serverUrl, cancellationToken);
                        pTask.Wait(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Interlocked.Increment(ref failed);
                    throw;
                }
            }, cancellationToken))
            .ToList();

        Task t = Task.WhenAll(tasks);
        try
        {
            await t;
        }
        catch
        {
            // ignored
        }

        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                Trace.WriteLine($"All connections successfully authenticated");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} album request attempts failed!");
                break;
        }
        return failed == 0;
    }

    public Task<bool> AddRangeAsync(Guid playlistId, BaseMusicItem[] musicItems, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}