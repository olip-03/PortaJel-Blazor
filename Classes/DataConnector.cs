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
        // Cached data. Sorted by unique ID, the by the type for quicker access
        // These will potentially contain thousands of albums and songs and shit so it's important that these 
        // are done efficiently.
        public Dictionary<Guid, Album> cachedAlbums = new Dictionary<Guid, Album>();
        public Dictionary<Guid, Artist> cachedArtists = new Dictionary<Guid, Artist>();
        public Dictionary<Guid, Song> cachedSongs = new Dictionary<Guid, Song>();

        private List<ServerConnecter> connecters = new List<ServerConnecter>();

        public void AddServer(ServerConnecter server)
        {
            connecters.Add(server);
        }
        /// <summary>
        /// Retreieves all albums from connected servers. 
        /// </summary>
        /// <param name="limit">Total amount of albums that should be retuend. Depends on how many albums the requested server has at the specifiec index.</param>
        /// <param name="startIndex">What index the server should start counting from</param>
        /// <returns>Album[] (Album array) containing all albums as requested from all servers</returns>
        public async Task<Album[]> GetAllAlbumsAsync(int? limit = 50, int? startIndex = 0)
        {
            List<Album> toReturn = new List<Album>();

            // await Parallel.ForEachAsync

            // Create tasks to pull requested data
            List<Task> toExecute = new List<Task>();
            foreach (ServerConnecter server in connecters)
            {
                List<Album> albums = new List<Album>();
                toExecute.Add(server.GetAlbumsAsync(limit: limit, startFromIndex: startIndex));
            }
            
            // Execure tasks
            await Task.WhenAll(toExecute);

            // Return data and add to list  
            foreach (var task in toExecute)
            {
                var returnValue = ((Task<Album[]>)task).Result;
                toReturn.AddRange(returnValue);
            }

            // Sort data 
            // TODO: Ensuere sorting method is actually sorting, you know. 
            toReturn.Sort();

            // Set return capacity to requested limit 
            if (toReturn.Count > limit) 
            {
                int capacity = (int)limit;// you're a real boy now int
                toReturn.RemoveRange(capacity, toReturn.Count); // Cap the count at the maximum requested.
            }

            return toReturn.ToArray();
        }
    }
}
