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

    public async Task<Album[]> GetAllAlbumsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        List<AlbumData> filteredCache = new();
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

    public async Task<Album> GetAlbumAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Filter the cache based on the provided parameters
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        // Null reference check
        if (albumFromDb == null) return Album.Empty;
        //Create tasks
        var songTask = _database.Table<SongData>().Where(song => albumFromDb.GetSongIds().Contains(song.Id))
            .ToArrayAsync();
        var artistTask =  _database.Table<ArtistData>().Where(artist => albumFromDb.GetArtistIds().Contains(artist.Id)).ToArrayAsync();
        // Await
        Task.WaitAll([songTask, artistTask],  cancellationToken);
        // Return data
        SongData[] songFromDb = songTask.Result;
        ArtistData[] artistsFromDb = artistTask.Result;
        return new Album(albumFromDb, songFromDb, artistsFromDb);
    }

    public async Task<Album[]> GetSimilarAlbumsAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        AlbumData albumFromDb = await _database.Table<AlbumData>().Where(album => album.Id == id).FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (albumFromDb == null) return [];
        var artistsFromDb = await _database.Table<AlbumData>().Where(album => albumFromDb.GetSimilarIds().Contains(album.Id)).ToArrayAsync();
        return artistsFromDb.Select(a => new Album(a)).ToArray(); 
    }

    public async Task<int> GetTotalAlbumCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to get the total count of albums in the database
        return await _database.Table<AlbumData>().CountAsync().ConfigureAwait(false);
    }

    public async Task<bool> AddRange(Album[] albums, CancellationToken cancellationToken = default)
    {
        foreach (var a in albums)
        {
            await _database.InsertOrReplaceAsync(a.GetBase, albums.First().GetBase.GetType());
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
        return true;
    }
}