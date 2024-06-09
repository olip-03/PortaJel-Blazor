using SQLite;
using System.Text.Json;

namespace PortaJel_Blazor.Classes.Database
{
    public class SongData
    {
        [PrimaryKey, NotNull]
        public Guid Id { get; set; }
        public string? PlaylistId { get; set; }
        public Guid? AlbumId { get; set; }
        public string ArtistIdsJson { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public long DurationMs { get; set; } = 0;
        public int PlayCount { get; set; } = 0;
        public DateTimeOffset? DateAdded { get; set; } 
        public int IndexNumber { get; set; } = 0;
        public int DiskNumber { get; set; } = 0;    
        public string ServerAddress { get; set; } = string.Empty;
        public bool IsDownloaded { get; set; } = false;
        public string FileLocation { get; set; } = string.Empty;
        public string StreamUrl { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public Guid[]? GetArtistIds()
        {
            return JsonSerializer.Deserialize<Guid[]>(ArtistIdsJson);
        }
    }
}
