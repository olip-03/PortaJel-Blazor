using SQLite;
using System.Text.Json;

namespace PortaJel_Blazor.Classes.Database
{
    public class ArtistData
    {
        [PrimaryKey, NotNull]
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string LogoImgSource { get; set; } = string.Empty;
        public string BackgroundImgSource { get; set; } = string.Empty;
        public string BackgroundImgBlurhash { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public string AlbumIdsJson { get; set;} = string.Empty;
        public Guid[]? GetAlbumIds()
        {
            return JsonSerializer.Deserialize<Guid[]>(AlbumIdsJson);
        }
    }
}
