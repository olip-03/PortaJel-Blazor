using PortaJel_Blazor.Classes.Database;
using Jellyfin.Sdk.Generated.Models;
using System.Text.Json;

namespace PortaJel_Blazor.Data
{
    public class Album : BaseMusicItem
    {
        public Guid Id => _albumData.Id;
        public string Name => _albumData.Name;
        public bool IsFavourite => _albumData.IsFavourite;
        public int PlayCount => _albumData.PlayCount;
        public DateTimeOffset? DateAdded => _albumData.DateAdded;
        public string ServerAddress => _albumData.ServerAddress;
        public string ImgSource => _albumData.ImgSource;
        public string ImgBlurhash => _albumData.ImgBlurhash;
        public ArtistData[]? Artists => _artistData;
        public string ArtistNames
        {
            get
            {
                if(Artists == null) { return String.Empty; }
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
            private set
            {

            }
        }
        public SongData[]? songs => _songData;
        public bool IsPartial { get; private set; } = true;

        private AlbumData _albumData; 
        private SongData[]? _songData;
        private ArtistData[]? _artistData;

        public Album()
        {
            _albumData = new();
            _songData = [];
            _artistData = [];
        }
        public Album(AlbumData albumData)
        {
            _albumData = albumData;
            _songData = [];
            _artistData = [];
        }
        public Album(AlbumData albumData, SongData[] songData)
        {
            _albumData = albumData;
            _songData = songData;
            _artistData = [];
        }
        public Album(AlbumData albumData, SongData[] songData, ArtistData[] artistData)
        {
            _albumData = albumData;
            _songData = songData;
            _artistData = artistData;
        }

        public AlbumSortMethod sortMethod { get; set; } = AlbumSortMethod.name;
        public enum AlbumSortMethod
        {
            name,
            artist,
            id
        }
        public static readonly Album Empty = new();

        public static Album Builder(BaseItemDto albumData, string server, BaseItemDto[]? songData = null, BaseItemDto[]? artistData = null)
        {
            if (albumData.Id == null)
            {
                throw new ArgumentException("Cannot create Album without Id! Please fix server call flags!");
            }
            if (albumData.UserData == null)
            {
                throw new ArgumentException("Cannot create Album without UserData! Please fix server call flags!");
            }
            if (albumData.AlbumArtists == null)
            {
                throw new ArgumentException("Cannot create Album without AlbumArtists! Please fix server call flags!");
            }

            MusicItemImage albumImage = MusicItemImage.Builder(albumData, server);

            AlbumData album = new();
            album.Id = (Guid)albumData.Id;
            album.Name = albumData.Name == null ? string.Empty : albumData.Name;
            album.IsFavourite = albumData.UserData.IsFavorite == null ? false : (bool)albumData.UserData.IsFavorite;
            // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
            album.DateAdded = albumData.DateCreated;
            album.ServerAddress = server;
            album.ImgSource = albumImage.source;
            album.ImgBlurhash = albumImage.Blurhash;
            album.ArtistIdsJson = JsonSerializer.Serialize(albumData.AlbumArtists.Select(idPair => idPair.Id).ToArray());

            List<SongData> songs = new();
            if(songData != null)
            {
                foreach (BaseItemDto song in songData)
                {
                    if (song.Id == null)
                    {
                        throw new ArgumentException("Cannot create Song without Song Id! Please fix server call flags!");
                    }
                    if (song.UserData == null)
                    {
                        throw new ArgumentException("Cannot create Song without Song UserData! Please fix server call flags!");
                    }
                    if (song.ArtistItems == null)
                    {
                        throw new ArgumentException("Cannot create Song without Song ArtistItems! Please fix server call flags!");
                    }

                    MusicItemImage songImage = MusicItemImage.Builder(albumData, server);

                    SongData toAdd = new();
                    toAdd.Id = (Guid)song.Id;
                    toAdd.PlaylistId = song.PlaylistItemId;
                    toAdd.AlbumId = albumData.Id;
                    toAdd.ArtistIdsJson = JsonSerializer.Serialize(song.ArtistItems.Select(idPair => idPair.Id).ToArray());
                    toAdd.Name = song.Name == null ? string.Empty : song.Name;
                    toAdd.IsFavourite = song.UserData.IsFavorite == null ? false : (bool)song.UserData.IsFavorite;
                    toAdd.DurationMs = song.CumulativeRunTimeTicks == null ? 0 : TimeSpan.FromTicks((long)song.CumulativeRunTimeTicks).Milliseconds;
                    // song.PlayCount = songData.PlayCount; TODO: Implement playcount idk
                    toAdd.DateAdded = song.DateCreated;
                    //song.DiskNumber = songData.DiskNumber; TODO: Implement disknumber
                    toAdd.ServerAddress = server;
                    toAdd.IsDownloaded = false; // TODO: Check if file exists idk 
                    toAdd.FileLocation = string.Empty; // TODO: Add file location
                    toAdd.StreamUrl = server + "/Audio/" + song.Id + "/stream?static=true&audioCodec=adts&enableAutoStreamCopy=true&allowAudioStreamCopy=true&enableMpegtsM2TsMode=true&context=Static";
                    toAdd.ImgSource = songImage.source;
                    toAdd.ImgBlurhash = songImage.Blurhash;

                    songs.Add(toAdd);
                }
            }

            List<ArtistData> artists = [];
            if (artistData != null && artistData.Length > 0)
            {
                foreach (BaseItemDto artist in artistData)
                {
                    if(artist.Id == null)
                    {
                        throw new ArgumentException("Cannot create Artist without Id! Please fix server call flags!");
                    }
                    if (artist.UserData == null)
                    {
                        throw new ArgumentException("Cannot create Artist without Artist UserData! Please fix server call flags!");
                    }

                    MusicItemImage artistLogo = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Logo);
                    MusicItemImage artistBackdrop = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Backdrop);
                    MusicItemImage artistImg = MusicItemImage.Builder(artist, server, ImageBuilderImageType.Primary);

                    ArtistData toAdd = new();
                    toAdd.Id = (Guid)artist.Id;
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

            return new Album(album, songs.ToArray(), artists.ToArray());
        }

        public void SetIsFavourite(bool state)
        {
            _albumData.IsFavourite = state;
        }
    }
}
