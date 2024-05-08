using PortaJel_Blazor.Classes;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Jellyfin.Sdk;
namespace PortaJel_Blazor.Data
{
    /// <summary>
    /// Represents 'songs' and all relevent information to display, and play them 
    /// </summary>
    public class Song : BaseMusicItem
    {
        public string playlistId = string.Empty;
        /// <summary>
        /// Reference to the artists of this song.
        /// </summary>
        public Artist[] artists = new Artist[0];
        public string artistCongregate
        {
            get
            {
                string data = string.Empty;
                for (int i = 0; i < artists.Length; i++)
                {
                    data += artists[i].name;
                    if(i < artists.Length - 1)
                    {
                        data += ", ";
                    }
                }
                return data;
            }
            private set 
            { 

            }
        }
        /// <summary>
        /// Reference to the album this song belongs to.
        /// </summary>
        public Album? album = Album.Empty;
        public string streamUrl { get; set; } = String.Empty;
        public int diskNum { get; set; }
        public long duration { get; set; }
        public string fileLocation { get; set; } = String.Empty;
        public bool isDownloaded { get; set; } = false;

        #region Constructors
        public static readonly Song Empty = new(setGuid: Guid.Empty, setName: String.Empty);
        public Song(Guid setGuid, string setName, string? setPlaylistId = null, string? setServerId = null, Guid[]? setArtistIds = null, Guid? setAlbumID = null, string? setStreamUrl = null, int? setDiskNum = 0, long? setDuration = 0, bool setIsFavourite = false, string? setFileLocation = null, bool setIsDownloaded = false)
        {
            // Required variables
            id = setGuid;
            name = setName;

            // Rest of em
            if (setArtistIds != null)
            { //Set Artists
                List<Artist> newArtistsIDs = new();
                for (int i = 0; i < setArtistIds.Length; i++)
                {
                    Artist toAdd = new();
                    toAdd.id = setArtistIds[i];
                    newArtistsIDs.Add(toAdd);
                }
                artists = newArtistsIDs.ToArray();
            }
            if(setAlbumID != null)
            { //Set Album
                Album setAlbum = new Album();
                setAlbum.id = (Guid)setAlbumID;
                album = setAlbum;
            }
            if(setStreamUrl != null)
            { //Set Stream Location
                streamUrl = setStreamUrl;
            }
            if(setDiskNum != null)
            { //Set Disk Number
                diskNum = (int)setDiskNum;
            }
            if(setDuration!= null)
            {
                duration = (long)setDuration;
            }
            if(setPlaylistId != null)
            {
                playlistId = setPlaylistId;
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
        /// Fetches the Artist based on if the artist has been returned by the API already, also fills in the artist of this object will the full data of that artist. If not fetch from API.
        /// </summary>
        /// <returns>Artist array of this object.</returns>
        public async Task<Artist[]?> GetArtistAsync() 
        {
            List<Artist> toAdd = new List<Artist>();
            for (int i = 0; i < artists.Length; i++)
            {
                Artist? artist = Artist.Empty;
                bool exists = MauiProgram.artistDictionary.TryGetValue(artists[i].id, out artist);
                if (!exists)
                {
                    // artist = await MauiProgram.servers[0].GetArtistAsync(artists[i].id);
                    artist = await MauiProgram.api.GetArtistAsync(artists[i].id);
                }
                if (artist != null)
                {
                    toAdd.Add(artist);
                }
            }
            artists = toAdd.ToArray();
            return artists;
        }
        public List<ContextMenuItem> GetContextMenuItems()
        {
            contextMenuItems.Clear();

            if (this.isFavourite)
            {
                contextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Task(async () =>
                {
                    MauiProgram.MainPage.CloseContextMenu();

                    this.isFavourite = false;
                    await MauiProgram.api.SetFavourite(this, false);
                })));
            }
            else
            {
                contextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Task(async () =>
                {
                    MauiProgram.MainPage.CloseContextMenu();

                    this.isFavourite = true;
                    await MauiProgram.api.SetFavourite(this, true);
                })));
            }
            contextMenuItems.Add(new ContextMenuItem("Download", "light_cloud_download.png", new Task(() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
                // TODO: implement download manager and all
            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Playlist", "light_playlist.png", new Task(() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
                // TODO: implement adding to playlist feature
            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Task(async () =>
            {
                MauiProgram.MainPage.CloseContextMenu();

                MauiProgram.MediaService.AddSong(this);

                #if !WINDOWS
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                string text = $"{this.name} added to queue.";
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                #endif
                
            })));
            contextMenuItems.Add(new ContextMenuItem("View Album", "light_album.png", new Task(async () =>
            {
                MauiProgram.MainPage.CloseContextMenu();

                await MauiProgram.MainPage.AwaitContextMenuClose();
                MauiProgram.MainPage.ShowLoadingScreen(true);
                MauiProgram.WebView.NavigateAlbum(this.id);
            })));
            contextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Task(async () =>
            {
                MauiProgram.MainPage.CloseContextMenu();

                await MauiProgram.MainPage.AwaitContextMenuClose();
                MauiProgram.MainPage.ShowLoadingScreen(true);
                MauiProgram.WebView.NavigateArtist(this.artists.FirstOrDefault().id);
            })));
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
            })));

            return contextMenuItems;
        }
        public static Song Builder(BaseItemDto baseItem, string server)
        {
            long duration = -1;
            if (baseItem.RunTimeTicks != null)
            {
                duration = (long)baseItem.RunTimeTicks;
            }
            else if (baseItem.CumulativeRunTimeTicks != null)
            {
                duration = (long)baseItem.CumulativeRunTimeTicks;
            }

            Song newSong = new(
                    setGuid: baseItem.Id,
                    setPlaylistId: baseItem.PlaylistItemId,
                    setName: baseItem.Name,
                    setAlbumID: baseItem.AlbumId,
                    setDiskNum: 0, //TODO: Fix disk num
                    setDuration: duration,
                    setIsFavourite: baseItem.UserData.IsFavorite) ;
            newSong.playCount = baseItem.UserData.PlayCount;
            newSong.image = MusicItemImage.Builder(baseItem, server);
            newSong.streamUrl = server + "/Audio/" + baseItem.Id + "/stream";
            newSong.serverAddress = server;
            
            if (baseItem.RunTimeTicks != null)
            {
                newSong.duration = (long)baseItem.RunTimeTicks;
            }
            else if(baseItem.CumulativeRunTimeTicks != null)
            {
                newSong.duration = (long)baseItem.CumulativeRunTimeTicks;
            }

            List<Artist> artists = new List<Artist>();
            foreach (var item in baseItem.AlbumArtists)
            {
                artists.Add(Artist.Builder(item, server));
            }
            newSong.artists = artists.ToArray();
            return newSong;
        }
        #endregion
    }
}
