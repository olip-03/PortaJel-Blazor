using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Database;
using SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;

namespace Portajel.Connections.Services.Database
{
    public class DatabaseConnector : IDbConnector
    {
        private readonly SQLiteAsyncConnection _database;
        private const SQLiteOpenFlags DbFlags =
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        public IDbItemConnector Album { get; set; }
        public IDbItemConnector Artist { get; set; }
        public IDbItemConnector Song { get; set; }
        public IDbItemConnector Playlist { get; set; }
        public IDbItemConnector Genre { get; set; }
        // TODO: Storing Radio Stations in Db
        // First we need to implement the API functions 
        public Dictionary<string, IDbItemConnector> GetDataConnectors() => new()
        {
            { "Album", Album },
            { "Artist", Artist },
            { "Song", Song },
            { "Playlist", Playlist },
            { "Genre", Genre }
        };

        public DatabaseConnector(string appDataDirectory)
        {
            _database = new SQLiteAsyncConnection(appDataDirectory, DbFlags);

            _database.CreateTableAsync<AlbumData>().Wait();
            _database.CreateTableAsync<SongData>().Wait();
            _database.CreateTableAsync<ArtistData>().Wait();
            _database.CreateTableAsync<PlaylistData>().Wait();

            Album = new DatabaseAlbumConnector(_database);
            Artist = new DatabaseArtistConnector(_database);
            Song = new DatabaseSongConnector(_database);
            Playlist = new DatabasePlaylistConnector(_database);
            Genre = new DatabaseGenreConnector(_database);
        }

        public async Task<BaseMusicItem[]> SearchAsync(
            string searchTerm = "", 
            int? limit = null, 
            int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name, 
            SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // If no search term is provided, return an empty array.
                return Array.Empty<BaseMusicItem>();
            }

            searchTerm = FormatString(searchTerm);

            var resultList = new List<BaseMusicItem>();

            // Iterate over each data connector
            foreach (var connectorPair in GetDataConnectors())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var connectorName = connectorPair.Key;
                var dataConnector = connectorPair.Value;
                
                switch (connectorName)
                {
                    case "Album":
                        var matchingAlbums = (await _database.Table<AlbumData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingAlbums.Select(item => new Album(item)));
                        break;
                    case "Artist":
                        var matchingArtists = (await _database.Table<ArtistData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingArtists.Select(item => new Artist(item)));
                        break;
                    case "Song":
                        var matchingSongs = (await _database.Table<SongData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingSongs.Select(item => new Song(item)));
                        break;
                    case "Playlist":
                        var matchingPlaylists = (await _database.Table<PlaylistData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingPlaylists.Select(item => new Playlist(item)));
                        break;
                    case "Genre":
                        // var matchingItems = (await _database.Table<GenreData>().ToListAsync().ConfigureAwait(false))
                        //     .Where(item => FormatString(item.Name).Contains(searchTerm))
                        //     .ToList();
                        // resultList.InsertRange(matchingItems.Select(item => new Genre(item)));
                        break;
                }
            }

            // Now, apply sorting on the combined results
            IEnumerable<BaseMusicItem> sortedResult;

            switch (setSortTypes)
            {
                case ItemSortBy.DateCreated:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.DateAdded)
                        : resultList.OrderByDescending(item => item.DateAdded);
                    break;
                case ItemSortBy.DatePlayed:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.DatePlayed)
                        : resultList.OrderByDescending(item => item.DatePlayed);
                    break;
                case ItemSortBy.Name:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.Name)
                        : resultList.OrderByDescending(item => item.Name);
                    break;
                case ItemSortBy.PlayCount:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.PlayCount)
                        : resultList.OrderByDescending(item => item.PlayCount);
                    break;
                default:
                    // Default sorting by Name
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.Name)
                        : resultList.OrderByDescending(item => item.Name);
                    break;
            }

            // Apply limit and offset
            if (limit.HasValue)
            {
                sortedResult = sortedResult.Skip(startIndex).Take(limit.Value);
            }

            // Return the result as an array
            return sortedResult.ToArray();
        }

        private string FormatString(string toFormat)
        {
            if (string.IsNullOrEmpty(toFormat))
                return string.Empty;
            string result = Regex.Replace(toFormat, @"[^a-zA-Z0-9]", "");
            result = result.ToLowerInvariant();
            return result;
        }
    }
}