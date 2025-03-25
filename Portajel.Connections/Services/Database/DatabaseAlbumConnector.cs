using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Database;
using Portajel.Connections.Interfaces;
using SQLite;

namespace Portajel.Connections.Services.Database;

// ReSharper disable once CoVariantArrayConversion
public class DatabaseAlbumConnector : IDbItemConnector
{
    private readonly SQLiteAsyncConnection _database;
    public DatabaseAlbumConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task<BaseMusicItem[]> GetAllAsync(
        int? limit = null, 
        int startIndex = 0,
        bool? getFavourite = null, 
        ItemSortBy setSortTypes = ItemSortBy.Album, 
        SortOrder setSortOrder = SortOrder.Ascending, 
        Guid?[]? includeIds = null, 
        Guid?[]? excludeIds = null, 
        CancellationToken cancellationToken = default)
    {
        List<AlbumData> filteredCache = [];
        limit ??= await _database.Table<AlbumData>().CountAsync();
        switch (setSortTypes)
        {
            case ItemSortBy.DateCreated:
                filteredCache.AddRange(await _database.Table<AlbumData>()
                    .OrderByDescending(album => album.DateAdded)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            case ItemSortBy.DatePlayed:
                filteredCache.AddRange(await _database.Table<AlbumData>()
                    .OrderByDescending(album => album.DatePlayed)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            case ItemSortBy.Name:
                filteredCache.AddRange(await _database.Table<AlbumData>()
                    .OrderByDescending(album => album.Name)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            case ItemSortBy.Random:
                var firstTake = await _database.Table<AlbumData>().ToListAsync().ConfigureAwait(false);
                filteredCache = firstTake
                    .OrderBy(album => Guid.NewGuid())
                    .Take((int)limit)
                    .ToList();
                break;
            case ItemSortBy.PlayCount:
                filteredCache.AddRange(await _database.Table<AlbumData>()
                    .OrderByDescending(album => album.PlayCount)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            default:
                filteredCache.AddRange(await _database.Table<AlbumData>()
                    .OrderByDescending(album => album.Name)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
        }
        return filteredCache.Select(dbItem => new Album(dbItem)).ToArray();
    }

    public async Task<BaseMusicItem> GetAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        // Filter the cache based on the provided parameters
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        // Null reference check
        if (albumFromDb == null) return Album.Empty;
        //Create tasks
        var songIds = albumFromDb.GetSongIds();
        var songTask = songIds.Length > 0
            ? _database.Table<SongData>().Where(song => songIds.Contains(song.Id)).ToArrayAsync()
            : Task.FromResult(Array.Empty<SongData>());
        var artistIds = albumFromDb.GetArtistIds();
        var artistTask = artistIds.Length > 0
            ? _database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.Id)).ToArrayAsync()
            : Task.FromResult(Array.Empty<ArtistData>());

        // Await
        Task.WaitAll([songTask, artistTask], cancellationToken);
        // Return data
        SongData[] songFromDb = songTask.Result;
        ArtistData[] artistsFromDb = artistTask.Result;
        return new Album(albumFromDb, songFromDb, artistsFromDb);
    }

    public async Task<int> GetTotalCountAsync(
        bool? getFavourite = null, 
        CancellationToken cancellationToken = default)
    {
        // Return the total count after removing duplicates
        var query = _database.Table<AlbumData>();
        if (getFavourite == true)
            query = query.Where(album => album.IsFavourite);

        return await query.CountAsync();
    }

    public async Task<bool> DeleteAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the album
            var album = await _database.Table<AlbumData>().FirstOrDefaultAsync(a => a.LocalId == id);
            if (album == null) return false;
            await _database.DeleteAsync(album);
            Trace.WriteLine($"Deleted album with ID {id}.");
            return true; 
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting album with ID {id}: {ex.Message}");
            return false; // Deletion failed
        }
    }

    public async Task<bool> DeleteRangeAsync(
        Guid[] ids, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var id in ids)
            {
                // Find the album
                var album = await _database.Table<AlbumData>().FirstOrDefaultAsync(a => a.LocalId == id);
                if (album == null)
                {
                    Trace.WriteLine($"Album with ID {id} not found.");
                    return false; // Stop if any album is not found
                }
                await _database.DeleteAsync(album);
                Trace.WriteLine($"Deleted album with ID {id}.");
            }
            return true; // All deletions succeeded
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting albums: {ex.Message}");
            return false; // Deletion failed for one or more
        }
    }

    public async Task<bool> InsertAsync(
        BaseMusicItem album,
        CancellationToken cancellationToken = default)
    {
        if (album is Album a && a.GetBase != null)
        {
            await _database.InsertOrReplaceAsync(a.GetBase, a.GetBase.GetType());
        }
        return true;
    }

    public async Task<bool> InsertRangeAsync(
        BaseMusicItem[] albums, 
        CancellationToken cancellationToken = default)
    {
        foreach (var baseMusicItem in albums)
        {
            if (baseMusicItem is Album a && a.GetBase != null)
            {
                await _database.InsertOrReplaceAsync(a.GetBase, a.GetBase.GetType());
            }
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
        return true;
    }
}