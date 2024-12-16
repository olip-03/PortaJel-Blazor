using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    public class Album: BaseMusicItem
    {
        private readonly AlbumData _albumData = new();
        public AlbumData GetBase => _albumData;
        public ArtistData[] Artists { get; }
        public string ArtistNames => _albumData.ArtistNames;
        public Guid[] ArtistIds => _albumData.GetArtistIds();
        public SongData[] Songs { get; private set; }
        public Guid[] SimilarIds => _albumData.GetSimilarIds();
        public bool IsPartial { get; private set; } = true;
        
        public AlbumSortMethod sortMethod { get; set; } = AlbumSortMethod.name;
        public enum AlbumSortMethod
        {
            name,
            artist,
            id
        }
        public Album(AlbumData albumData = null, SongData[] songData = null, ArtistData[] artistData = null)
        {
            Artists = artistData ?? [];
            Songs = songData ?? [];
            _albumData = albumData ?? new();
            
            LocalId = _albumData.LocalId;
            Id = _albumData.Id;
            Name = _albumData.Name;
            IsFavourite = _albumData.IsFavourite;
            PlayCount = _albumData.PlayCount;
            DateAdded = _albumData.DateAdded;
            DatePlayed = _albumData.DatePlayed;
            ServerAddress = _albumData.ServerAddress;
            ImgSource =   _albumData.ImgSource;
            ImgBlurhash = _albumData.ImgBlurhash;
            ImgBlurhashBase64 = _albumData.BlurhashBase64;
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
            return Songs == null ? [] : Songs.Select(song => new Song(song, _albumData, Artists)).ToArray();
        }

        // Method to add a song
        public bool AddSong(SongData newSong)
        {
            var list = Songs.ToList();
            list.Add(newSong);
            Songs = list.ToArray();
            return true; 
        }
        public void SetIsFavourite(bool state)
        {
            _albumData.IsFavourite = state;
        }

    }
}
