using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    public class Album(AlbumData albumData = null, SongData[] songData = null, ArtistData[] artistData = null) : BaseMusicItem
    {
        public AlbumData GetBase => albumData;
        public new Guid Id => albumData.Id;
        public string Name => albumData.Name;
        public bool IsFavourite => albumData.IsFavourite;
        public int PlayCount => albumData.PlayCount;
        public DateTimeOffset? DateAdded => albumData.DateAdded;
        public DateTimeOffset? DatePlayed => albumData.DatePlayed;
        public string ServerAddress => albumData.ServerAddress;
        public new string ImgSource =>   albumData.ImgSource;
        public new string ImgBlurhash => albumData.ImgBlurhash;
        public ArtistData[] Artists { get; } = artistData;
        public string ArtistNames => albumData.ArtistNames;
        public Guid[] ArtistIds => albumData.GetArtistIds();
        public SongData[] Songs { get; } = songData;
        public Guid[] SimilarIds => albumData.GetSimilarIds();
        public bool IsPartial { get; private set; } = true;
        
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
            return Songs == null ? [] : Songs.Select(song => new Song(song, albumData, Artists)).ToArray();
        }
        public void SetIsFavourite(bool state)
        {
            albumData.IsFavourite = state;
        }

    }
}
