﻿using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    /// <summary>
    /// Main class for handling data from many servers, as well as cached data and whatever
    /// </summary>
    public class DataConnector
    {
        /// <summary>
        /// List of connectors that data is pulled from. Key is the URL of the server, ServerConnector is the connector itself.
        /// </summary>
        private Dictionary<string, ServerConnecter> connecters = new();
        
        /// <summary>
        /// Adds a new server to access data from.
        /// </summary>
        /// <param name="server">The ServerConnector to be added. This must be initialized and set up to be used in this class.</param>
        /// <returns>True if the server was successfully added, false if otherwise. Will return false if duplicate keys (servers with the same address) are added.</returns>
        public bool AddServer(ServerConnecter server)
        {
            if (!connecters.ContainsKey(server.GetBaseAddress().ToLower()))
            {
                connecters.Add(server.GetBaseAddress().ToLower(), server);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a particular server from sources of info. 
        /// </summary>
        /// <param name="server">The ServerConnector to be removed.</param>
        public void RemoveServer(ServerConnecter server) 
        { 
            connecters.Remove(server.GetBaseAddress());
        }
        
        /// <summary>
        /// Removes a particular server from sources of info. 
        /// </summary>
        /// <param name="address">The URL of the ServerConnector to be removed.</param>
        public void RemoveServer(string address)
        {
            ServerConnecter[] enumerate = connecters.Values.ToArray();
            foreach (var srv in enumerate)
            {
                if(srv.GetBaseAddress().ToLower() == address.ToLower())
                {
                    connecters.Remove(address.ToLower());
                }
            }
        }
        
        public async Task<bool> AuthenticateAllAsync()
        {
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
               await server.Value.AuthenticateServerAsync().ConfigureAwait(false);
            });
            return true;
        }

        /// <summary>
        /// Gets all the servers we're currently connected to.
        /// </summary>
        /// <returns>ServerConnecter[] (ServerConnecter array) containing all ServerConnectors added to the DataConnector</returns>
        public ServerConnecter[] GetServers()
        {
            return connecters.Values.ToArray();
        }

        #region AlbumEndpoints
        /// <summary>
        /// Retrieves all albums from connected servers. 
        /// </summary>
        /// <param name="limit">The maximum number of albums to retrieve.</param>
        /// <param name="startIndex">The index from which to start retrieving albums.</param>
        /// <param name="isFavourite">A boolean value indicating whether to retrieve only favorite albums.</param>
        /// <param name="sortTypes">Optional. Specify one or more sort orders, comma delimited. Options: Album, AlbumArtist, Artist, Budget, CommunityRating, CriticRating, DateCreated, DatePlayed, PlayCount, PremiereDate, ProductionYear, SortName, Random, Revenue, Runtime.</param>
        /// <param name="sortOrder">The sort order for the albums.</param>
        /// <returns>Album[] (Album array) containing all albums as requested from all servers</returns>
        public async Task<Album[]> GetAllAlbumsAsync(
            int? limit = null, 
            int startIndex = 0, 
            bool isFavourite = false, 
            bool isPartial = true, 
            ItemSortBy sortTypes = ItemSortBy.Default, 
            SortOrder sortOrder = SortOrder.Descending, 
            bool offline = false, 
            bool downloaded = false, 
            CancellationToken cancellactionToken = new())
        {
            List<Album> albumsReturn = new List<Album>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                //albumsReturn.AddRange(await server.Value.GetAllAlbumsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, cancellationToken: cancellationToken));
                int? actualLimit = null;
                if (limit != null)
                {
                    actualLimit = (int)limit / connecters.Count;
                }
                albumsReturn.AddRange(await server.Value.GetAllAlbumsAsync(setLimit: actualLimit, setStartIndex: startIndex, setFavourites: isFavourite, getPartial: isPartial, setSortTypes: sortTypes, setSortOrder: sortOrder, getOffline: offline, getDownloaded: downloaded, cancellactionToken).ConfigureAwait(false));
            }).ConfigureAwait(false);

            // TODO: ensure we're capping the limit as set in the initali integer

            return albumsReturn.ToArray();
        }

        /// <summary>
        /// Retrieves an album asynchronously based on the specified album ID.
        /// </summary>
        /// <param name="albumId">The unique identifier of the album to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is the album 
        /// corresponding to the specified album ID.
        /// </returns>
        public async Task<Album> GetAlbumAsync(Guid albumId, bool offline = false, bool downloaded = false)
        {
            Album toReturn = Album.Empty;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                toReturn = await server.Value.GetAlbumAsync(setId: albumId, getOffline: offline, getDownloaded: downloaded);
            });
            return toReturn;
        }

        /// <summary>
        /// Retrieves similar albums asynchronously based on the provided album set ID.
        /// </summary>
        /// <param name="setId">The ID of the album set to find similar albums for.</param>
        /// <returns>An array of albums that are similar to the provided album set.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the server connector has not been initialized.</exception>
        public async Task<Album[]> GetSimilarAlbumsAsync(Guid setId, int limit = 30)
        {
            List<Album> toReturn = new();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                Album[] toAdd = await server.Value.GetSimilarAlbumsAsync(setId, limit);
                toReturn.AddRange(toAdd); 
            });
            return toReturn.ToArray();
        }

        public async Task<int> GetTotalAlbumCount(bool isFavourite = false)
        {
            int total = 0;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                total += await server.Value.GetTotalAlbumCount(isFavourite).ConfigureAwait(false);
            });
            return total;
        }
        #endregion

        #region ArtistEndoings
        public async Task<Artist[]> GetAllArtistsAsync(
            int limit = 50, 
            int? startIndex = 0, 
            bool? isFavourite = false,
            CancellationToken cancellactionToken = new(), 
            bool offline = false, 
            bool downloaded = false)
        {
            List<Artist> artistsReturn = new List<Artist>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                artistsReturn.AddRange(await server.Value.GetAllArtistsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, getOffline: offline, getDownloaded: downloaded));
            });
            // artistsReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            // Set return capacity to requested limit 
            if (artistsReturn.Count > limit)
            {
                int capacity = (int)limit;
                artistsReturn.RemoveRange(capacity, artistsReturn.Count); // Cap the count at the maximum requested.
            }

            return artistsReturn.ToArray();
        }

        public async Task<Artist> GetArtistAsync(Guid albumId, bool offline = false, bool downloaded = false)
        {
            Artist toReturn = Artist.Empty;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                toReturn = await server.Value.GetArtistAsync(albumId, getOffline: offline, getDownloaded: downloaded);
            });
            return toReturn;
        }

        public async Task<Artist[]> GetSimilarArtistsAsync(Guid setId, int limit = 30)
        {
            List<Artist> toReturn = new();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                Artist[] toAdd = await server.Value.GetSimilarArtistsAsync(setId, limit);
                toReturn.AddRange(toAdd);
            });
            return toReturn.ToArray();
        }

        public async Task<int> GetTotalArtistCount(bool isFavourite = false)
        {
            int total = 0;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                total += await server.Value.GetTotalArtistCount(isFavourite).ConfigureAwait(false);
            });
            return total;
        }
        #endregion

        #region SongsEndpoints
        /// <summary>
        /// Retrieves all songs asynchronously.
        /// </summary>
        /// <param name="limit">The maximum number of songs to retrieve. Default is null.</param>
        /// <param name="startIndex">The starting index for retrieving songs. Default is 0.</param>
        /// <param name="isFavourite">Specifies whether to retrieve only favorite songs. Default is false.</param>
        /// <param name="sortTypes">Optional. Specify one or more sort orders, comma delimited. Options: Album, AlbumArtist, Artist, Budget, CommunityRating, CriticRating, DateCreated, DatePlayed, PlayCount, PremiereDate, ProductionYear, SortName, Random, Revenue, Runtime.</param>
        /// <param name="sortOrder">The sort order for the retrieved songs. Default is descending.</param>
        /// <returns>An array of songs.</returns>
        public async Task<Song[]> GetAllSongsAsync(
            int? limit = null, 
            int? startIndex = 0, 
            bool? isFavourite = false, 
            ItemSortBy sortTypes = ItemSortBy.Default, 
            SortOrder[]? sortOrder = null, 
            bool offline = false, 
            bool downloaded = false,
            CancellationToken cancellactionToken = new())
        {
            List<Song> songsReturn = new List<Song>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                songsReturn.AddRange(await server.Value.GetAllSongsAsync(setLimit: limit, setStartIndex: startIndex, setFavourites: isFavourite, setSortTypes: sortTypes, setSortOrder: sortOrder, getOffline: offline, getDownloaded: downloaded, setCancellactionToken: cancellactionToken));
            });
            //songsReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            return songsReturn.ToArray();
        }
        public async Task<int> GetTotalSongCount(bool isFavourite = false)
        {
            int total = 0;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                total += await server.Value.GetTotalSongCount(isFavourite).ConfigureAwait(false);
            });
            return total;
        }
        #endregion

        #region GenreEndpoints
        //public async Task<Genre[]> GetAllGenresAsync(int limit = 50, int? startIndex = 0, CancellationToken? cancellationToken = null)
        //{
        //    List<Genre> genresReturn = new List<Genre>();
        //    await Parallel.ForEachAsync(connecters, async (server, ct) => {
        //        genresReturn.AddRange(await server.Value.GetAllGenresAsync(limit: limit, startFromIndex: startIndex, cancellationToken: cancellationToken));
        //    });
        //    //genresReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

        //    // Set return capacity to requested limit 
        //    if (genresReturn.Count > limit)
        //    {
        //        int capacity = (int)limit;
        //        genresReturn.RemoveRange(capacity, genresReturn.Count); // Cap the count at the maximum requested.
        //    }

        //    return genresReturn.ToArray();
        //}
        #endregion

        #region PlaylistsEndpoint
        public async Task<Playlist[]> GetAllPlaylistsAsync(
            int limit = -1,
            int startIndex = 0,
            bool isFavourite = false,
            bool isPartial = true,
            ItemSortBy sortTypes = ItemSortBy.Default,
            SortOrder sortOrder = SortOrder.Descending,
            bool offline = false,
            bool downloaded = false,
            CancellationToken cancellactionToken = new())
        {
            List<Playlist> playlistsReturn = new List<Playlist>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                playlistsReturn.AddRange(await server.Value.GetAllPlaylistsAsync(
                    limit: limit, 
                    startIndex: startIndex, 
                    isFavourite: isFavourite));
            });

            return playlistsReturn.ToArray();
        }

        public async Task<Playlist> GetPlaylistAsync(Guid playlistId, bool offline = false, bool downloaded = false)
        {
            Playlist? toReturn = Playlist.Empty;
            await Parallel.ForEachAsync(connecters, async (server, ct) =>
            {
                toReturn = await server.Value.GetPlaylistAsync(playlistId, getOffline: offline, getDownloaded: downloaded);
            });
            return toReturn;
        }
        public async Task<int> GetTotalPlaylistCount(bool isFavourite = false)
        {
            int total = 0;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                total += await server.Value.GetTotalPlaylistCount(isFavourite).ConfigureAwait(false);
            });
            return total;
        }
        #endregion

        public async Task<bool> SetFavourite(Guid itemId, string serverAddress, bool favouriteState)
        {
            if (connecters.ContainsKey(serverAddress))
            {
                // This needs to shoot its shot out into the ether, if it isn't able to respond it needs to
                // - Kill itself 
                // - Tell the application it's failed to do so, and add it to the 'do when online' queue
                await connecters[serverAddress].FavouriteItem(itemId, favouriteState);
                return true;
            }
            else
            {
                throw new InvalidOperationException("The specified item does not have a server address in the list of connectors!");
            }
        }
        
        public UserCredentials[] GetAllUserCredentials()
        {
            return connecters.Select(s => s.Value.GetUserCredentials()).ToArray();
        }
    }
}
