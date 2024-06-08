using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Data;
using SQLite;
using System.Text.Json;

namespace PortaJel_Blazor.Classes.Database
{
    public class AlbumData
    {
        [PrimaryKey, NotNull]
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public int PlayCount { get; set; } = 0;
        public DateTimeOffset? DateAdded { get; set; }
        public string ServerAddress { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public string ArtistIdsJson { get; set; } = string.Empty;
        public string SongIdsJson { get; set; } = string.Empty;
        public Guid[]? GetArtistIds()
        {
            return JsonSerializer.Deserialize<Guid[]>(ArtistIdsJson);
        }
        public Guid[]? GetSongIds()
        {
            return JsonSerializer.Deserialize<Guid[]>(SongIdsJson);
        }
    }
}
