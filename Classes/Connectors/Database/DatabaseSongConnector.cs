using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;
using System.Linq;

namespace PortaJel_Blazor.Classes.Connectors.Database
{
    public class DatabaseSongConnector : IMediaDataConnector
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseSongConnector(SQLiteAsyncConnection database)
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
            limit ??= 50;

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
                    filteredCache = allSongs.OrderBy(_ => Guid.NewGuid()).Take(limit.Value).ToList();
                    break;
                case ItemSortBy.PlayCount:
                    query = query.OrderByDescending(song => song.PlayCount);
                    break;
                default:
                    query = query.OrderBy(song => song.Name);
                    break;
            }

            if (setSortTypes != ItemSortBy.Random)
                filteredCache = await query.Skip(startIndex).Take(limit.Value).ToListAsync();

            return filteredCache.Select(song => new Song(song)).ToArray();
        }
        

        public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            var songData = await _database.Table<SongData>().Where(song => song.Id == id).FirstOrDefaultAsync();
            if (songData == null) return Song.Empty;

            var albumData = await _database.Table<AlbumData>().Where(album => songData.AlbumId == album.Id).FirstOrDefaultAsync();
            var artistData = await _database.Table<ArtistData>().Where(artist => songData.GetArtistIds().First() == artist.Id).FirstOrDefaultAsync();
            return new Song(songData, albumData, [artistData]);
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);

        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);
        }

        public async Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();
            if (getFavourite)
                query = query.Where(song => song.IsFavourite);

            return await query.CountAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            try
            {
                // Delete associated songs
                var songs = await _database.Table<SongData>().Where(s => s.LocalId == id).ToListAsync();
                foreach (var song in songs)
                {
                    await _database.DeleteAsync(song);
                    Trace.WriteLine($"Deleted song with ID {song.LocalId} associated with album ID {id}.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error deleting song with ID {id}: {ex.Message}");
                return false; // Deletion failed
            }
        }

        public async Task<bool> AddRange(Song[] songs, CancellationToken cancellationToken = default)
        {
            foreach (var s in songs)
            {
                await _database.InsertOrReplaceAsync(s.GetBase, songs.First().GetBase.GetType());
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
