using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    /// <summary>
    /// Represents 'songs' and all relevent information to display, and play them 
    /// </summary>
    public class Song : BaseMusicItem
    {
        public SongData GetBase => _songData;
        public Guid AlbumId => _songData.AlbumId;
        public Guid LocalAlbumId => _songData.LocalAlbumId;
        public string? PlaylistId => _songData.PlaylistId;
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

        private readonly SongData _songData = new();
        private readonly AlbumData _albumData = new();
        private readonly ArtistData[] _artistData = [];

        #region Constructors
        public static readonly Song Empty = new();
        public Song()
        {
            IsPartial = true;
        }
        public Song(SongData songData, AlbumData albumData = null, ArtistData[] artistData = null)
        {
            _songData = songData;
            _albumData = albumData ?? new AlbumData();
            _artistData = artistData ?? [];

            if(_albumData == null || _artistData.Length  == 0)
            {
                IsPartial = true;
            }
             
            LocalId = _songData.LocalId;
            Id = _songData.Id;
            Name = _songData.Name;
            IsFavourite = _songData.IsFavourite;
            PlayCount = _songData.PlayCount;
            DateAdded = _songData.DateAdded;
            DatePlayed = _songData.DatePlayed;
            ServerAddress = _songData.ServerAddress;
            ImgSource = _songData.ImgSource;
            ImgBlurhash = _songData.ImgBlurhash;
            ImgBlurhashBase64 = _songData.BlurhashBase64;
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
