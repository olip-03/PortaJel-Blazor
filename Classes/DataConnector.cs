using Jellyfin.Sdk;
using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    /// <summary>
    /// Main class for handling data from many servers, as well as cached data and whatever
    /// </summary>
    public class DataConnector
    {
        private Dictionary<string, ServerConnecter> connecters = new();
        /// <summary>
        /// Adds a new server to access data from.
        /// </summary>
        /// <param name="server">The ServerConnector to be added. This must be initalized and set up to be used in this class.</param>
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
        public void RemoveServer(ServerConnecter server) 
        { 
            connecters.Remove(server.GetBaseAddress());
        }
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
        public ServerConnecter[] GetServers()
        {
            return connecters.Values.ToArray();
        }
        #region AlbumEndpoints
        /// <summary>
        /// Retreieves all albums from connected servers. 
        /// </summary>
        /// <param name="limit">Total amount of albums that should be retuend. Depends on how many albums the requested server has at the specifiec index.</param>
        /// <param name="startIndex">What index the server should start counting from</param>
        /// <returns>Album[] (Album array) containing all albums as requested from all servers</returns>
        public async Task<Album[]> GetAllAlbumsAsync(int? limit = 50, int? startIndex = 0, bool? isFavourite = false, CancellationToken? cancellationToken = null)
        {
            List<Album> albumsReturn = new List<Album>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                albumsReturn.AddRange(await server.Value.GetAllAlbumsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, cancellationToken: cancellationToken));
            });
            //albumsReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            // Set return capacity to requested limit 
            if (albumsReturn.Count > limit) 
            {
                int capacity = (int)limit;
                albumsReturn.RemoveRange(capacity, albumsReturn.Count); // Cap the count at the maximum requested.
            }

            return albumsReturn.ToArray();
        }
        #endregion
        #region ArtistEndoings
        public async Task<Artist[]> GetAllArtistsAsync(int? limit = 50, int? startIndex = 0, bool? isFavourite = false, CancellationToken? cancellationToken = null)
        {
            List<Artist> artistsReturn = new List<Artist>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                artistsReturn.AddRange(await server.Value.GetAllArtistsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, cancellationToken: cancellationToken));
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
        #endregion
        #region SongsEndpoints
        public async Task<Song[]> GetAllSongsAsync(int? limit = 50, int? startIndex = 0, bool? isFavourite = false, CancellationToken? cancellationToken = null)
        {
            List<Song> songsReturn = new List<Song>();
            await Parallel.ForEachAsync(connecters, async (server, ct) => {
                songsReturn.AddRange(await server.Value.GetAllSongsAsync(limit: limit, startFromIndex: startIndex, favourites: isFavourite, cancellationToken: cancellationToken));
            });
            //songsReturn.Sort(); // TODO: Ensuere sorting method is actually sorting, you know. 

            // Set return capacity to requested limit 
            if (songsReturn.Count > limit)
            {
                int capacity = (int)limit;
                songsReturn.RemoveRange(capacity, songsReturn.Count); // Cap the count at the maximum requested.
            }

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
                playlistsReturn.AddRange(await server.Value.GetPlaylistsAsycn(limit: limit, startFromIndex: startIndex));
            });

            return playlistsReturn.ToArray();
        }
        #endregion
    }
}
