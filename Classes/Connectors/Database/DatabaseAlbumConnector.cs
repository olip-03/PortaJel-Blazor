using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabaseAlbumConnector : IMediaServerAlbumConnector
{
    private readonly SQLiteAsyncConnection _database = null;
    public DatabaseAlbumConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public async Task<Album[]> GetAllAlbumsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        List<AlbumData> filteredCache = new();
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

    public async Task<Album> GetAlbumAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        // Filter the cache based on the provided parameters
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        SongData[] songFromDb = [];
        ArtistData[] artistsFromDb = [];
        if (albumFromDb == null) return Album.Empty;

        Guid[] songIds = albumFromDb.GetSongIds();
        Guid[] artistIds = albumFromDb.GetArtistIds();

        if (songIds == null || artistIds == null) return new Album(albumFromDb, songFromDb, artistsFromDb);
        Task<SongData[]> songDbQuery = _database.Table<SongData>().Where(song => songIds.Contains(song.Id)).OrderBy(song => song.IndexNumber).ThenBy(song => song.DiskNumber).ToArrayAsync();
        Task<ArtistData[]> artistDbQuery = _database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.Id)).ToArrayAsync();

        Task.WaitAll(songDbQuery, artistDbQuery);

        songFromDb = songDbQuery.Result.OrderBy(s => s.DiskNumber).ToArray();
        artistsFromDb = artistDbQuery.Result;

        return new Album(albumFromDb, songFromDb, artistsFromDb);
    }

    public Task<Album[]> GetSimilarAlbumsAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetTotalAlbumCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to get the total count of albums in the database
        return await _database.Table<AlbumData>().CountAsync().ConfigureAwait(false);
    }
}