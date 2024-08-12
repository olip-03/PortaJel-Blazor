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
        public DateTimeOffset? DatePlayed => _albumData.DatePlayed;
        public string ServerAddress => _albumData.ServerAddress;
        public string ImgSource => _albumData.ImgSource;
        public string ImgBlurhash => _albumData.ImgBlurhash;
        public ArtistData[]? Artists => _artistData;
        public string ArtistNames => _albumData.ArtistNames;
        public Guid[]? ArtistIds => _albumData.GetArtistIds();
        public SongData[]? Songs => _songData;
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
            AlbumData album = AlbumData.Builder(albumData, server);
            SongData[] songs = [];
            ArtistData[] artists = [];
            if(songData != null)
            {
                songs = songData.Select(data => SongData.Builder(data, server)).ToArray();
            }
            if(artistData  != null)
            {
                artists = artistData.Select(data => ArtistData.Builder(data, server)).ToArray();
            }
            return new Album(album, songs, artists);
        }
        public Song[] GetSongs()
        {
            if (_songData == null) return [];
            return _songData.Select(song => new Song(song, _albumData, _artistData)).ToArray();
        }
        public void SetIsFavourite(bool state)
        {
            _albumData.IsFavourite = state;
        }
    }
}
