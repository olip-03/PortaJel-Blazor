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

        public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            Guid?[] includeIds = null,
            Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();

            // Apply includeIds filter
            if (includeIds != null && includeIds.Any())
            {
                query = query.Where(song => includeIds.Contains(song.Id));
            }
            else if(includeIds != null)
            {
                return [];
            }

            // Apply excludeIds filter
            if (excludeIds != null && excludeIds.Any())
            {
                query = query.Where(song => !excludeIds.Contains(song.Id));
            }
            else if(includeIds != null)
            {
                return [];
            }

            // Apply getFavourite filter
            if (getFavourite.HasValue)
            {
                query = query.Where(song => song.IsFavourite == getFavourite.Value);
            }

            // Apply sorting
            switch (setSortTypes)
            {
                case ItemSortBy.DateCreated:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.DateAdded)
                        : query.OrderByDescending(song => song.DateAdded);
                    break;
                case ItemSortBy.DatePlayed:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.DatePlayed)
                        : query.OrderByDescending(song => song.DatePlayed);
                    break;
                case ItemSortBy.Name:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.Name)
                        : query.OrderByDescending(song => song.Name);
                    break;
                case ItemSortBy.PlayCount:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.PlayCount)
                        : query.OrderByDescending(song => song.PlayCount);
                    break;
                case ItemSortBy.Random:
                    // For random sorting, fetch data first and then shuffle
                    var allSongs = await query.ToListAsync().ConfigureAwait(false);
                    allSongs = allSongs.OrderBy(song => Guid.NewGuid()).ToList();
                    if (limit.HasValue)
                    {
                        allSongs = allSongs.Take(limit.Value).ToList();
                    }

                    return allSongs.Select(dbItem => new Song(dbItem)).ToArray();
                default:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.Name)
                        : query.OrderByDescending(song => song.Name);
                    break;
            }

            // Apply pagination
            if (startIndex > 0)
            {
                query = query.Skip(startIndex);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            // Execute the query
            var filteredCache = await query.ToListAsync().ConfigureAwait(false);

            // Convert to BaseMusicItem[]
            return filteredCache.Select(dbItem => new Song(dbItem)).ToArray();
        }

        public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var songData = await _database.Table<SongData>().Where(song => song.Id == id).FirstOrDefaultAsync();
            if (songData == null) return Song.Empty;

            var albumData = await _database.Table<AlbumData>().Where(album => songData.AlbumId == album.Id)
                .FirstOrDefaultAsync();
            var artistData = await _database.Table<ArtistData>()
                .Where(artist => songData.GetArtistIds().First() == artist.Id).FirstOrDefaultAsync();
            return new Song(songData, albumData, [artistData]);
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);
        }

        public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();
            if (getFavourite == true)
                query = query.Where(song => song.IsFavourite);

            return await query.CountAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
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

        public async Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var id in ids)
                {
                    // Find the album
                    var song = await _database.Table<SongData>().FirstOrDefaultAsync(s => s.LocalId == id);
                    if (song == null)
                    {
                        Trace.WriteLine($"Song with ID {id} not found.");
                        return false; // Stop if any album is not found
                    }
                    await _database.DeleteAsync(song);
                    Trace.WriteLine($"Deleted Song with ID {id}.");
                }
                return true; // All deletions succeeded
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error deleting albums: {ex.Message}");
                return false; // Deletion failed for one or more
            }
        }

        public async Task<bool> AddRange(BaseMusicItem[] songs, CancellationToken cancellationToken = default)
        {
            foreach (var s in songs)
            {
                if (s is not Song song) continue;
                await _database.InsertOrReplaceAsync(song.GetBase, song.GetBase.GetType());
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            return true;
        }
    }
}