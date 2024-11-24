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

    public DatabaseAlbumConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
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

    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
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

    public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (albumFromDb == null) return [];
        var artistsFromDb = await _database.Table<AlbumData>()
            .Where(album => albumFromDb.GetSimilarIds().Contains(album.Id)).ToArrayAsync();
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
    
    public async Task<bool> AddRange(BaseMusicItem[] albums, CancellationToken cancellationToken = default)
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