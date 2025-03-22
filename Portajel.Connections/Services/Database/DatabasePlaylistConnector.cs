using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Database;
using Portajel.Connections.Interfaces;
using SQLite;

namespace Portajel.Connections.Services.Database;

public class DatabasePlaylistConnector : IMediaDataConnector, IMediaPlaylistInterface
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabasePlaylistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        limit ??= 50;
        List<PlaylistData> filteredCache = [];
        filteredCache.AddRange(await _database.Table<PlaylistData>()
            .OrderByDescending(playlist => playlist.Name)
            .Take(limit.Value).ToListAsync().ConfigureAwait(false));
        return filteredCache.Select(dbItem => new Playlist(dbItem)).ToArray();
    }

    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        PlaylistData playlistDbItem =
            await _database.Table<PlaylistData>().Where(p => p.Id == id).FirstOrDefaultAsync();
        var songData = await _database.Table<SongData>().Where(song => playlistDbItem.GetSongIds().Contains(song.Id)).ToArrayAsync();
        return new Playlist(playlistDbItem, songData);
    }

    public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var query = _database.Table<PlaylistData>();
        if (getFavourite == true)
            query = query.Where(song => song.IsFavourite);

        return await query.CountAsync();
    }
    
    public Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        try
        {
            // Delete associated playlists
            var playlists = await _database.Table<PlaylistData>().Where(p => p.LocalId == id).ToListAsync();
            foreach (var playlist in playlists)
            {
                await _database.DeleteAsync(playlist);
                Trace.WriteLine($"Deleted playlist with ID {playlist.LocalId} associated with album ID {id}.");
            }
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting playlist with ID {id}: {ex.Message}");
            return false; // Deletion failed
        }
    }

    public async Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var id in ids)
            {
                // Find the album
                var playlist = await _database.Table<PlaylistData>().FirstOrDefaultAsync(p => p.LocalId == id);
                if (playlist == null)
                {
                    Trace.WriteLine($"Playlist with ID {id} not found.");
                    return false; // Stop if any album is not found
                }
                await _database.DeleteAsync(playlist);
                Trace.WriteLine($"Deleted playlist with ID {id}.");
            }
            return true; // All deletions succeeded
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting playlists: {ex.Message}");
            return false; // Deletion failed for one or more
        }
    }

    public async Task<bool> AddRange(BaseMusicItem[] musicItems, CancellationToken cancellationToken = default)
    {
        foreach (var p in musicItems)
        {
            if (p is not Playlist playlist) continue;
            await _database.InsertOrReplaceAsync(playlist.GetBase, playlist.GetBase.GetType());
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }
        return true;
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

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}