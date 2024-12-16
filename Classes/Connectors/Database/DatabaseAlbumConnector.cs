using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

// ReSharper disable once CoVariantArrayConversion
public class DatabaseAlbumConnector : IMediaDataConnector
{
    private readonly SQLiteAsyncConnection _database = null;
    private readonly DatabaseConnector _databaseConnector;

    public DatabaseAlbumConnector(SQLiteAsyncConnection database, DatabaseConnector connector)
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
        List<AlbumData> filteredCache = new();

        // Set default limit if not provided
        limit ??= await _database.Table<AlbumData>().CountAsync();

        // Base query
        var query = _database.Table<AlbumData>();

        // Apply includeIds and excludeIds filters
        if (includeIds != null && includeIds.Any())
        {
            query = query.Where(album => includeIds.Contains(album.LocalId));
        }
        
        if (excludeIds != null && excludeIds.Any())
        {
            query = query.Where(album => !excludeIds.Contains(album.LocalId));
        }
        
        // Apply serverUrl filter
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            query = query.Where(album => album.ServerAddress == serverUrl);
        }

        if (getFavourite.HasValue)
        {
            query = query.Where(album => album.IsFavourite == getFavourite.Value);
        }
        
        // Sort and retrieve data based on sorting type
        switch (setSortTypes)
        {
            case ItemSortBy.DateCreated:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(album => album.DateAdded)
                    : query.OrderByDescending(album => album.DateAdded);
                break;

            case ItemSortBy.DatePlayed:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(album => album.DatePlayed)
                    : query.OrderByDescending(album => album.DatePlayed);
                break;

            case ItemSortBy.Name:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(album => album.Name)
                    : query.OrderByDescending(album => album.Name);
                break;

            case ItemSortBy.PlayCount:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(album => album.PlayCount)
                    : query.OrderByDescending(album => album.PlayCount);
                break;

            case ItemSortBy.Random:
                // Random sorting has to be done in memory
                var allItems = await query.ToListAsync().ConfigureAwait(false);
                filteredCache = allItems
                    .OrderBy(_ => Guid.NewGuid())
                    .Skip(startIndex)
                    .Take((int)limit)
                    .ToList();
                return filteredCache.Select(album => new Album(album)).ToArray();

            default:
                query = setSortOrder == SortOrder.Ascending
                    ? query.OrderBy(album => album.Name)
                    : query.OrderByDescending(album => album.Name);
                break;
        }

        // Apply pagination and limit
        filteredCache.AddRange(await query
            .Skip(startIndex)
            .Take((int)limit)
            .ToListAsync()
            .ConfigureAwait(false));

        // Convert filtered data to the output type
        return filteredCache.Select(dbItem => new Album(dbItem)).ToArray();
    }
    
    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Filter the cache based on the provided parameters
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.LocalId == id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        // Null reference check
        if (albumFromDb == null) return Album.Empty;
        //Create tasks
        var songTask = _database.Table<SongData>().Where(song => song.LocalAlbumId == albumFromDb.LocalId)
            .ToArrayAsync();
        var artistIds = albumFromDb.GetArtistIds();
        var artistTask = artistIds.Length > 0
            ? _database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.LocalId)).ToArrayAsync()
            : Task.FromResult(Array.Empty<ArtistData>());

        // Await
        Task.WaitAll([songTask, artistTask], cancellationToken);
        // Return data
        SongData[] songFromDb = songTask.Result.OrderBy(s => s.IndexNumber).ToArray();
        ArtistData[] artistsFromDb = artistTask.Result;
        return new Album(albumFromDb, songFromDb, artistsFromDb);
    }

    public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.LocalId == id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (albumFromDb == null) return [];
        var artistsFromDb = await _database.Table<AlbumData>()
            .Where(album => albumFromDb.GetSimilarIds().Contains(album.LocalId)).ToArrayAsync();
        return artistsFromDb.Select(a => new Album(a)).ToArray<BaseMusicItem>();
    }

    public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Return the total count after removing duplicates
        var query = _database.Table<AlbumData>();
        if (getFavourite == true)
            query = query.Where(album => album.IsFavourite);

        return await query.CountAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, string serverUrl = "",
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

    public async Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "",
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

    public async Task<bool> AddRange(BaseMusicItem[] albums, CancellationToken cancellationToken = default)
    {
        foreach (var baseMusicItem in albums)
        {
            if (baseMusicItem is Album { GetBase: not null } a)
            {
                AlbumData album = a.GetBase;
                album.BlurhashBase64 = baseMusicItem.ImgBlurhashBase64;
                await _database.InsertOrReplaceAsync(album, a.GetBase.GetType());
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return true;
    }
}