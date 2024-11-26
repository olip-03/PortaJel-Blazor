using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;


namespace PortaJel_Blazor.Classes.Connectors.Database;

// ReSharper disable once CoVariantArrayConversion
public class DatabaseArtistConnector : IMediaDataConnector
{
    private readonly SQLiteAsyncConnection _database = null;
    
    public DatabaseArtistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Descending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        List<ArtistData> filteredCache = [];
        limit ??= await _database.Table<ArtistData>().CountAsync();
        switch (setSortTypes)
        {
            case ItemSortBy.DateCreated:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(album => album.DateAdded)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            case ItemSortBy.Name:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(album => album.Name)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
            case ItemSortBy.Random:
                var firstTake = await _database.Table<ArtistData>().ToListAsync().ConfigureAwait(false);
                filteredCache = firstTake
                    .OrderBy(album => Guid.NewGuid())
                    .Take((int)limit)
                    .ToList();
                break;
            default:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(album => album.Name)
                    .Take((int)limit).ToListAsync().ConfigureAwait(false));
                break;
        }

        return filteredCache.Select(dbItem => new Artist(dbItem)).ToArray();
    }
    
    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl, CancellationToken cancellationToken)
    {
        ArtistData artistDbItem =
            await _database.Table<ArtistData>().Where(artist => artist.Id == id).FirstOrDefaultAsync();
        AlbumData[] albumData = await _database.Table<AlbumData>()
            .Where(album => artistDbItem.GetAlbumIds().Contains(album.Id)).ToArrayAsync();
        return new Artist(artistDbItem, albumData);
    }
    
    public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        ArtistData artistFromDb = await _database.Table<ArtistData>().Where(artist => artist.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (artistFromDb == null) return [];
        var artistsFromDb = await _database.Table<ArtistData>().Where(artist => artistFromDb.GetSimilarIds()
            .Contains(artist.Id)).ToArrayAsync();
        return artistsFromDb.Select(a => new Artist(a)).ToArray(); 
    }
    
    public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var query = _database.Table<ArtistData>();
        if (getFavourite == true)
            query = query.Where(album => album.IsFavourite);

        return await query.CountAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        try
        {
            // Delete associated artists (if applicable)
            var artists = await _database.Table<ArtistData>().Where(a => a.LocalId == id).ToListAsync();
            foreach (var artist in artists)
            {
                await _database.DeleteAsync(artist);
                Trace.WriteLine($"Deleted artist with ID {artist.LocalId} associated with album ID {id}.");
            }
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting artist with ID {id}: {ex.Message}");
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
                var artist = await _database.Table<ArtistData>().FirstOrDefaultAsync(a => a.LocalId == id);
                if (artist == null)
                {
                    Trace.WriteLine($"Artist with ID {id} not found.");
                    return false; // Stop if any album is not found
                }
                await _database.DeleteAsync(artist);
                Trace.WriteLine($"Deleted artist with ID {id}.");
            }
            return true; // All deletions succeeded
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting artist: {ex.Message}");
            return false; // Deletion failed for one or more
        }
    }

    public async Task<bool> AddRange(BaseMusicItem[] artists, CancellationToken cancellationToken = default)
    {
        foreach (var a in artists)
        {
            if (a is not Artist artist) continue;
            await _database.InsertOrReplaceAsync(artist.GetBase, artist.GetBase.GetType());
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }
        return true;
    }
}