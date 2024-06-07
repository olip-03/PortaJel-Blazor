using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Data;
using SQLite;


namespace PortaJel_Blazor.Classes.SQL
{
    public class SQLAlbum
    {
        [PrimaryKey, NotNull]
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public bool isFavourite { get; set; } = false;
        public int playCount { get; set; } = 0;
        public int dateAdded { get; set; } = 0;
        public string serverAddress { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
        public string blurHash { get; set; } = string.Empty;
        public string artistIds { get; set; } = string.Empty;
        public string songIds { get; set; } = string.Empty;
        public static SQLAlbum Builder(Album fromAlbum)
        {
            return new SQLAlbum();
        }
        public static SQLAlbum Builder(BaseItemDto baseItem, string server)
        {
            return new SQLAlbum();
        }
    }
}
