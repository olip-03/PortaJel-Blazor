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
        public Guid Id => _songData.Id;
        public Guid AlbumId => _songData.AlbumId;
        public string Name => _songData.Name;
        public bool IsFavourite => _songData.IsFavourite;
        public int PlayCount => _songData.PlayCount;
        public DateTimeOffset? DateAdded => _songData.DateAdded;
        public string ServerAddress => _songData.ServerAddress;
        public string? PlaylistId => _songData.PlaylistId;
        public string ImgSource => _songData.ImgSource;
        public string ImgBlurhash => _songData.ImgBlurhash;
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
