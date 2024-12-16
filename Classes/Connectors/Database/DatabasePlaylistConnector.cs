using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabasePlaylistConnector : IMediaDataConnector, IMediaPlaylistInterface
{
    private readonly SQLiteAsyncConnection _database = null;
    private readonly DatabaseConnector _databaseConnector;

    public DatabasePlaylistConnector(SQLiteAsyncConnection database, DatabaseConnector connector)
    {
        _database = database;
        _databaseConnector = connector;
    }

    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(
        int? limit = null,
        int startIndex = 0,
        bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album,
        SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null,
        string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Initialize filtered cache
        List<PlaylistData> filteredCache = new();

        // Set default limit if not provided
        limit ??= await _database.Table<PlaylistData>().CountAsync();

        // Base query
        var query = _database.Table<PlaylistData>();

        // Apply includeIds and excludeIds filters
        if (includeIds != null && includeIds.Any())
        {
            query = query.Where(playlist => includeIds.Contains(playlist.LocalId));
        }

        if (excludeIds != null && excludeIds.Any())
        {
            query = query.Where(playlist => !excludeIds.Contains(playlist.LocalId));
        }

        // Apply serverUrl filter
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            query = query.Where(playlist => playlist.ServerAddress == serverUrl);
        }

        // Sort and retrieve data based on sorting type
        switch (setSortTypes)
        {
            // TODO: Reimplement commented filters. Currently playlist does not contain the data
            // case ItemSortBy.DateCreated:
            //     query = setSortOrder == SortOrder.Ascending
            //         ? query.OrderBy(playlist => playlist.DateAdded)
            //         : query.OrderByDescending(playlist => playlist.DateAdded);
            //     break;
            //
            // case ItemSortBy.DatePlayed:
            //     query = setSortOrder == SortOrder.Ascending
            //         ? query.OrderBy(playlist => playlist.DatePlayed)
            //         : query.OrderByDescending(playlist => playlist.DatePlayed);
            //     break;

            case ItemSortBy.Name:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(playlist => playlist.Name)
                    : query.OrderByDescending(playlist => playlist.Name);
                break;
            //
            // case ItemSortBy.PlayCount:
            //     query = setSortOrder == SortOrder.Ascending
            //         ? query.OrderBy(playlist => playlist.PlayCount)
            //         : query.OrderByDescending(playlist => playlist.PlayCount);
            //     break;

            case ItemSortBy.Random:
                // Random sorting has to be done in memory
                var allItems = await query.ToListAsync().ConfigureAwait(false);
                filteredCache = allItems
                    .OrderBy(_ => Guid.NewGuid())
                    .Skip(startIndex)
                    .Take((int)limit)
                    .ToList();
                return filteredCache.Select(playlist => new Playlist(playlist)).ToArray();

            default:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(playlist => playlist.Name)
                    : query.OrderByDescending(playlist => playlist.Name);
                break;
        }

        // Apply pagination and limit
        filteredCache.AddRange(await query
            .Skip(startIndex)
            .Take((int)limit)
            .ToListAsync()
            .ConfigureAwait(false));

        // Convert filtered data to the output type
        return filteredCache.Select(dbItem => new Playlist(dbItem)).ToArray();
    }


    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        PlaylistData playlistDbItem =
            await _database.Table<PlaylistData>().Where(p => p.Id == id).FirstOrDefaultAsync();
        var songData = await _database.Table<SongData>().Where(song => playlistDbItem.GetSongIds().Contains(song.Id))
            .ToArrayAsync();
        return new Playlist(playlistDbItem, songData);
    }

    public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
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

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex,
        CancellationToken cancellationToken = default)
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

    public async Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "",
        CancellationToken cancellationToken = default)
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
        foreach (var baseMusicItem in musicItems)
        {
            if (baseMusicItem is Playlist { GetBase: not null } p)
            {
                PlaylistData playlist = p.GetBase;
                playlist.BlurhashBase64 = baseMusicItem.ImgBlurhashBase64;
                await _database.InsertOrReplaceAsync(playlist, p.GetBase.GetType());
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        return true;
    }

    public Task<bool> AddAsync(Guid playlistId, BaseMusicItem musicItem, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}