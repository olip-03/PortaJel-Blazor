using Jellyfin.Sdk;
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
        public async Task<Album[]> GetAllAlbumsAsync(int? limit = null, int? startIndex = 0, bool? isFavourite = false, ItemSortBy[]? sortTypes = null, SortOrder sortOrder = SortOrder.Descending)
        {
            List<Album> albumsReturn = new List<Album>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                //albumsReturn.AddRange(await server.Value.GetAllAlbumsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, cancellationToken: cancellationToken));
                int? actualLimit = null;
                if (limit != null)
                {
                    actualLimit = (int)limit / connecters.Count;
                }
                albumsReturn.AddRange(await server.Value.GetAllAlbumsAsync(setLimit: actualLimit, setStartIndex: startIndex, setFavourites: isFavourite, setSortTypes: sortTypes, setSortOrder: sortOrder));
            });

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
        public async Task<Album> GetAlbumAsync(Guid albumId)
        {
            Album toReturn = Album.Empty;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                toReturn = await server.Value.GetAlbumAsync(setId: albumId);
            });
            return toReturn;
        }
        #endregion

        #region ArtistEndoings
        public async Task<Artist[]> GetAllArtistsAsync(int? limit = 50, int? startIndex = 0, bool? isFavourite = false, CancellationToken? cancellationToken = null)
        {
            List<Artist> artistsReturn = new List<Artist>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                artistsReturn.AddRange(await server.Value.GetAllArtistsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite));
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

        public async Task<Artist> GetArtistAsync(Guid albumId)
        {
            Artist toReturn = Artist.Empty;
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                toReturn = await server.Value.GetArtistAsync(albumId);
            });
            return toReturn;
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
        public async Task<Song[]> GetAllSongsAsync(int? limit = null, int? startIndex = 0, bool? isFavourite = false, ItemSortBy[]? sortTypes = null, SortOrder[]? sortOrder = null)
        {
            List<Song> songsReturn = new List<Song>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                songsReturn.AddRange(await server.Value.GetAllSongsAsync(setLimit: limit, setStartIndex: startIndex, setFavourites: isFavourite, setSortTypes: sortTypes, setSortOrder: sortOrder));
            });
            //songsReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            return songsReturn.ToArray();
        }
        #endregion

        #region GenreEndpoints
        public async Task<Genre[]> GetAllGenresAsync(int? limit = 50, int? startIndex = 0, CancellationToken? cancellationToken = null)
        {
            List<Genre> genresReturn = new List<Genre>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                genresReturn.AddRange(await server.Value.GetAllGenresAsync(limit: limit, startFromIndex: startIndex, cancellationToken: cancellationToken));
            });
            //genresReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            // Set return capacity to requested limit 
            if (genresReturn.Count > limit)
            {
                int capacity = (int)limit;
                genresReturn.RemoveRange(capacity, genresReturn.Count); // Cap the count at the maximum requested.
            }

            return genresReturn.ToArray();
        }
        #endregion

        #region PlaylistsEndpoint
        public async Task<Playlist[]> GetAllPlaylistsAsync(int? limit = 50, int? startIndex = 0, bool? isFavourite = false, CancellationToken? cancellationToken = null)
        {
            List<Playlist> playlistsReturn = new List<Playlist>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                playlistsReturn.AddRange(await server.Value.GetPlaylistsAsync(limit: limit, startFromIndex: startIndex));
            });

            return playlistsReturn.ToArray();
        }
        #endregion
    
        public async Task<bool> SetFavourite(BaseMusicItem item, bool favouriteState)
        {
            await connecters[item.serverAddress].FavouriteItem(item.id, favouriteState);
            return favouriteState;
        }
    }
}
