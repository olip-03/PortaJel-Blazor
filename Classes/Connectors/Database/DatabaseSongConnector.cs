using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;
using System.Linq;

namespace PortaJel_Blazor.Classes.Connectors.Database
{
    public class DatabaseSongConnector : IMediaServerSongConnector
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseSongConnector(SQLiteAsyncConnection database)
        {
            _database = database;
        }

        public async Task<Song[]> GetAllSongsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            string serverUrl = "", CancellationToken cancellationToken = default)
        {
            List<SongData> filteredCache = new();
            var query = _database.Table<SongData>();

            if (getFavourite)
                query = query.Where(song => song.IsFavourite);

            switch (setSortTypes)
            {
                case ItemSortBy.DateCreated:
                    query = query.OrderByDescending(song => song.DateAdded);
                    break;
                case ItemSortBy.DatePlayed:
                    query = query.OrderByDescending(song => song.DatePlayed);
                    break;
                case ItemSortBy.Name:
                    query = query.OrderBy(song => song.Name);
                    break;
                case ItemSortBy.Random:
                    var allSongs = await query.ToListAsync();
                    filteredCache = allSongs.OrderBy(_ => Guid.NewGuid()).Take(limit).ToList();
                    break;
                case ItemSortBy.PlayCount:
                    query = query.OrderByDescending(song => song.PlayCount);
                    break;
                default:
                    query = query.OrderBy(song => song.Name);
                    break;
            }

            if (setSortTypes != ItemSortBy.Random)
                filteredCache = await query.Skip(startIndex).Take(limit).ToListAsync();

            return filteredCache.Select(song => new Song(song)).ToArray();
        }

        public async Task<Song> GetSongAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            var songData = await _database.Table<SongData>().Where(song => song.Id == id).FirstOrDefaultAsync();
            if (songData == null) return Song.Empty;

            var albumData = await _database.Table<AlbumData>().Where(album => songData.AlbumId == album.Id).FirstOrDefaultAsync();
            var artistData = await _database.Table<ArtistData>().Where(artist => songData.GetArtistIds().First() == artist.Id).FirstOrDefaultAsync();
            return new Song(songData, albumData, [artistData]);
        }

        public Task<Song[]> GetSimilarSongsAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Song[]>([]);
        }

        public async Task<int> GetTotalSongCountAsync(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();
            if (getFavourite)
                query = query.Where(song => song.IsFavourite);

            return await query.CountAsync();
        }
    }
}
