using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Database;
using System.Text.Json;

namespace PortaJel_Blazor.Data
{
    /// <summary>
    /// Represents 'songs' and all relevent information to display, and play them 
    /// </summary>
    public class Song : BaseMusicItem
    {
        public Guid? Id => _songData.Id;
        public string Name => _songData.Name;
        public bool IsFavourite => _songData.IsFavourite;
        public int PlayCount => _songData.PlayCount;
        public DateTimeOffset? DateAdded => _songData.DateAdded;
        public string ServerAddress => _songData.ServerAddress;
        public string? PlaylistId => _songData.PlaylistId;
        public string ImgSource => _songData.ImgSource;
        public string ImgBlurhash => _songData.ImgBlurhash;

        public ArtistData[] Artists => _artistData;
        public string ArtistNames
        {
            get
            {
                if (Artists == null) { return String.Empty; }
                string data = string.Empty;
                for (int i = 0; i < Artists.Length; i++)
                {
                    data += Artists[i].Name;
                    if (i < Artists.Length - 1)
                    {
                        data += ", ";
                    }
                }
                return data;
            }
            private set { }
        }
        public AlbumData Album => _albumData;
        public int IndexNumber => _songData.IndexNumber;
        public string StreamUrl => _songData.StreamUrl;
        public int DiskNumber => _songData.DiskNumber;
        public long DurationMs => _songData.DurationMs;
        public string FileLocation => _songData.FileLocation;
        public bool IsDownloaded => _songData.IsDownloaded;
        public bool IsPartial { get; private set; } = false;

        private SongData _songData = new();
        private AlbumData _albumData = new();
        private ArtistData[] _artistData = [];

        #region Constructors
        public static readonly Song Empty = new();
        public Song()
        {
            IsPartial = true;
        }
        public Song(SongData songData, AlbumData? albumData, ArtistData[]? artistData)
        {
            _songData = songData;
            _albumData = albumData == null ? new() : albumData;
            _artistData = artistData == null ? [] : artistData;

            if(_albumData == null || _artistData.Length  == 0)
            {
                IsPartial = true;
            }
        }
        #endregion

        #region Methods
        public static Song Builder(BaseItemDto songData, string server, BaseItemDto? albumData = null, BaseItemDto[]? artistData = null)
        {
            if(songData.UserData == null)
            {
                throw new ArgumentException("Cannot create Song without Song UserData! Please fix server call flags!");
            }
            if (songData.Id == null)
            {
                throw new ArgumentException("Cannot create Song without ID! Please fix server call flags!");
            }

            MusicItemImage musicItemImage = MusicItemImage.Builder(albumData, server);

            SongData song = new();
            song.Id = (Guid)songData.Id;
            song.PlaylistId = songData.PlaylistItemId;
            song.AlbumId = albumData.Id;
            song.ArtistIdsJson = JsonSerializer.Serialize(artistData.Select(baseItem => baseItem.Id).ToArray());
            song.Name = songData.Name == null ? string.Empty : songData.Name;
            song.IsFavourite = songData.UserData.IsFavorite == null ? false : (bool)songData.UserData.IsFavorite;
            song.DurationMs = songData.CumulativeRunTimeTicks == null ? 0 : TimeSpan.FromTicks((long)songData.CumulativeRunTimeTicks).Milliseconds;
            // song.PlayCount = songData.PlayCount; TODO: Implement playcount idk
            song.DateAdded = songData.DateCreated;
            //song.DiskNumber = songData.DiskNumber; TODO: Implement disknumber
            song.ServerAddress = server;
            song.IsDownloaded = false; // TODO: Check if file exists idk 
            song.FileLocation = string.Empty; // TODO: Add file location
            song.StreamUrl = server + "/Audio/" + songData.Id + "/stream?static=true&audioCodec=adts&enableAutoStreamCopy=true&allowAudioStreamCopy=true&enableMpegtsM2TsMode=true&context=Static";
            song.ImgSource = musicItemImage.source;
            song.ImgBlurhash = musicItemImage.Blurhash;

            AlbumData album = new();
            if (albumData != null)
            {
                if (albumData.UserData == null)
                {
                    throw new ArgumentException("Cannot create Song without Album UserData! Please fix server call flags!");
                }
                if (albumData.Id == null)
                {
                    throw new ArgumentException("Cannot create Song without ID! Please fix server call flags!");
                }

                album.Id = (Guid)albumData.Id;
                album.Name = albumData.Name == null ? string.Empty : albumData.Name;
                album.IsFavourite = albumData.UserData.IsFavorite == null ? false : (bool)albumData.UserData.IsFavorite;
                // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
                album.DateAdded = albumData.DateCreated;
                album.ServerAddress = server;
                album.ImgSource = musicItemImage.source;
                album.ImgBlurhash = musicItemImage.Blurhash;
                album.ArtistIdsJson = JsonSerializer.Serialize(artistData.Select(baseItem => baseItem.Id).ToArray());
            }

            List<ArtistData> artists = [];
            if(artistData != null && artistData.Length > 0)
            {
                foreach (BaseItemDto artist in artistData)
                {
                    if (artist.UserData == null)
                    {
                        throw new ArgumentException("Cannot create Artist without Artist UserData! Please fix server call flags!");
                    }

                    MusicItemImage artistLogo = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Logo);
                    MusicItemImage artistBackdrop = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Backdrop);
                    MusicItemImage artistImg = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Primary);

                    ArtistData toAdd = new();
                    toAdd.Id = artist.Id;
                    toAdd.Name = artist.Name == null ? string.Empty : artist.Name;
                    toAdd.IsFavourite = artist.UserData.IsFavorite == null ? false : (bool)artist.UserData.IsFavorite;
                    toAdd.Description = artist.Overview == null ? string.Empty : artist.Overview;
                    toAdd.LogoImgSource = artistLogo.source;
                    toAdd.ImgSource = artistImg.source;
                    toAdd.ImgBlurhash = artistImg.Blurhash;
                    toAdd.BackgroundImgSource = artistBackdrop.source;
                    toAdd.BackgroundImgBlurhash = artistBackdrop.Blurhash;

                    artists.Add(toAdd);
                }
            }

            Song newSong = new(song, album, artists.ToArray());
            return newSong;
        }
        public void SetIsFavourite(bool state)
        {
            _songData.IsFavourite = state;

        }
        #endregion
    }
}
