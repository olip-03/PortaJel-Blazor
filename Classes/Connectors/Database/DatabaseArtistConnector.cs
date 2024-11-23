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

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        limit ??= 0;
        List<ArtistData> filteredCache = [];
        switch (setSortTypes)
        {
            case ItemSortBy.DateCreated:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(artist => artist.DateAdded)
                    .Take((int)limit).ToListAsync());
                break;
            case ItemSortBy.Name:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(artist => artist.Name)
                    .Take((int)limit).ToListAsync());
                break;
            case ItemSortBy.Random:
                var firstTake = await _database.Table<ArtistData>().ToListAsync();
                filteredCache = firstTake
                    .OrderBy(artist => Guid.NewGuid())
                    .Take((int)limit)
                    .ToList();
                break;
            default:
                filteredCache.AddRange(await _database.Table<ArtistData>()
                    .OrderByDescending(artist => artist.Name)
                    .Take((int)limit).ToListAsync());
                break;
        }

        return filteredCache.Select(artist => new Artist(artist)).ToArray();
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
    
    public async Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var query = _database.Table<ArtistData>();
        if (getFavourite)
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

    public async Task<bool> AddRange(Artist[] artists, CancellationToken cancellationToken = default)
    {
        foreach (var a in artists)
        {
            await _database.InsertOrReplaceAsync(a.GetBase, artists.First().GetBase.GetType());
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }
        return true;
    }
}