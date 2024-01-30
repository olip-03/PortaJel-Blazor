using System.Data.SqlTypes;

namespace PortaJel_Blazor.Data
{
    /// <summary>
    /// Represents 'songs' and all relevent information to display, and play them 
    /// </summary>
    public class Song : BaseMusicItem
    {
        /// <summary>
        /// Reference to the artists of this song.
        /// </summary>
        private Artist[] artists = new Artist[0];
        /// <summary>
        /// Reference to the album this song belongs to.
        /// </summary>
        private Album album = Album.Empty;
        public string streamUrl { get; private set; } = String.Empty;
        public int diskNum { get; private set; }
        public bool isFavourite { get; private set; } = false;
        public string fileLocation { get; set; } = String.Empty;
        public bool isDownloaded { get; set; } = false;

        #region Constructors
        public static readonly Song Empty = new(setGuid: Guid.Empty, setName: String.Empty);
        public Song(Guid setGuid, string setName, Guid[]? setArtistIds = null, Guid? setAlbumID = null, string? setStreamUrl = null, int? setDiskNum = 0, bool setIsFavourite = false, string? setFileLocation = null, bool setIsDownloaded = false)
        {
            // Required variables
            id = setGuid;
            name = setName;

            // Rest of em
            if (setArtistIds != null)
            { //Set Artists
                List<Artist> newArtists = new();
                for (int i = 0; i < setArtistIds.Length; i++)
                {
                    Artist toAdd = new Artist();
                    toAdd.id = setArtistIds[i];
                    newArtists.Add(toAdd);
                }
                artists = newArtists.ToArray();
            }
            if(setAlbumID != null)
            { //Set Album
                Album newAlbum = new Album();
                newAlbum.id = (Guid)setAlbumID;
                album = newAlbum;
            }
            if(setStreamUrl != null)
            { //Set Stream Location
                streamUrl = setStreamUrl;
            }
            if(setDiskNum != null)
            { //Set Disk Number
                diskNum = (int)setDiskNum;
            }
            if(setFileLocation != null)
            { //Set File Location
                fileLocation = setFileLocation;
            }
            isFavourite = setIsFavourite;
            isDownloaded = setIsDownloaded;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Fetches the Artist based on if the artist has been returned by the API already. If not fetch from API.
        /// </summary>
        /// <returns>Artist array of this object.</returns>
        public async Task<Artist[]> GetArtistAsync() 
        {
            List<Artist> toAdd = new List<Artist>();
            for (int i = 0; i < artists.Length; i++)
            {
                Artist? artist = artists[i];
                bool exists = MauiProgram.artistDictionary.TryGetValue(artists[i].id, out artist);
                if (!exists)
                {
                    artist = await MauiProgram.servers[0].GetArtistAsync(artists[i].id);
                }

                if (artist != null)
                {
                    toAdd.Add(artist);
                }
            }
            artists = toAdd.ToArray();
            return artists;
        }
        /// <summary>
        /// Fetches the Album of this song based on if the Album has been returned by the API already. If not force fetch from API.
        /// </summary>
        /// <returns>Album of this object.</returns>
        public async Task<Album> GetAlbumAsync()
        {
            bool exists = MauiProgram.albumDictionary.TryGetValue(album.id, out album);
            if (!exists)
            {
                album = await MauiProgram.servers[0].FetchAlbumByIDAsync(album.id);
            }
            return album;
        }
        #endregion
    }
}
