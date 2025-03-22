using Portajel.Connections.Database;

namespace Portajel.Connections.Data
{
    /// <summary>
    /// Represents 'songs' and all relevent information to display, and play them 
    /// </summary>
    public class Song : BaseMusicItem
    {
        public SongData GetBase => _songData;
        public new Guid LocalId => _songData.LocalId;
        public new Guid Id => _songData.Id;
        public Guid AlbumId => _songData.AlbumId;
        public new string Name => _songData.Name;
        public new bool IsFavourite => _songData.IsFavourite;
        public new int PlayCount => _songData.PlayCount;
        public new DateTimeOffset? DateAdded => _songData.DateAdded;
        public new DateTimeOffset? DatePlayed => _songData.DatePlayed;
        public new string ServerAddress => _songData.ServerAddress;
        public string? PlaylistId => _songData.PlaylistId;
        public new string ImgSource => _songData.ImgSource;
        public new string ImgBlurhash => _songData.ImgBlurhash;
        public ArtistData[] Artists => _artistData;
        public Guid[] ArtistIds => _songData.GetArtistIds();
        public string ArtistNames => _songData.ArtistNames;
        public AlbumData Album => _albumData;
        public int IndexNumber => _songData.IndexNumber;
        public string StreamUrl => _songData.StreamUrl;
        public int DiskNumber =>  _songData.DiskNumber;
        public TimeSpan Duration => _songData.Duration;
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
        public Song(SongData songData, AlbumData? albumData = null, ArtistData[]? artistData = null)
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
        public void SetIsFavourite(bool state)
        {
            _songData.IsFavourite = state;

        }
        public void SetPlaylistId(string playlistId)
        {
            _songData.PlaylistId = playlistId;
        }
        #endregion
    }
}
