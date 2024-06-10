using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Data;
using SQLite;
using System.Text.Json;

namespace PortaJel_Blazor.Classes.Database
{
    public class AlbumData
    {
        [PrimaryKey, NotNull]
        public Guid Id { get; set; }
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
        public static AlbumData Builder(BaseItemDto baseItem, string server)
        {
            if (baseItem.UserData == null)
            {
                throw new ArgumentException("Cannot create Song without Album UserData! Please fix server call flags!");
            }
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Song without ID! Please fix server call flags!");
            }

            MusicItemImage musicItemImage = MusicItemImage.Builder(baseItem, server);
            AlbumData album = new();
            album.Id = (Guid)baseItem.Id;
            album.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            album.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
            album.DateAdded = baseItem.DateCreated;
            album.ServerAddress = server;
            album.ImgSource = musicItemImage.source;
            album.ImgBlurhash = musicItemImage.Blurhash;
            album.ArtistIdsJson = JsonSerializer.Serialize(baseItem.ArtistItems.Select(idPair => idPair.Id).ToArray());

            return album;
        }
    }
}
