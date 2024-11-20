using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;


namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabaseArtistConnector : IMediaServerArtistConnector
{
    private readonly SQLiteAsyncConnection _database = null;
    
    public DatabaseArtistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public async Task<Artist[]> GetAllArtistAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Artist, SortOrder setSortOrder = SortOrder.Ascending,
        string serverUrl = "",
        CancellationToken cancellationToken = default)
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
    
    public async Task<Artist> GetArtistAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        ArtistData artistDbItem =
            await _database.Table<ArtistData>().Where(artist => artist.Id == id).FirstOrDefaultAsync();
        AlbumData[] albumData = await _database.Table<AlbumData>()
            .Where(album => artistDbItem.GetAlbumIds().Contains(album.Id)).ToArrayAsync();
        return new Artist(artistDbItem, albumData);
    }
    
    public async Task<Artist[]> GetSimilarArtistAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        ArtistData artistFromDb = await _database.Table<ArtistData>().Where(artist => artist.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (artistFromDb == null) return [];
        var artistsFromDb = await _database.Table<ArtistData>().Where(artist => artistFromDb.GetSimilarIds()
            .Contains(artist.Id)).ToArrayAsync();
        return artistsFromDb.Select(a => new Artist(a)).ToArray(); 
    }
    
    public async Task<int> GetTotalArtistCount(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var query = _database.Table<ArtistData>();
        if (getFavourite)
            query = query.Where(album => album.IsFavourite);

        return await query.CountAsync();
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