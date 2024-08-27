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
        public DateTimeOffset? DatePlayed { get; set; }
        public string ServerAddress { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public string ArtistIdsJson { get; set; } = string.Empty;
        public string ArtistNames { get; set; } = string.Empty;
        public string SongIdsJson { get; set; } = string.Empty;
        public Guid[]? GetArtistIds()
        {
            Guid[]? artistIds;
            try
            {
                artistIds = JsonSerializer.Deserialize<Guid[]>(ArtistIdsJson);
            }
            catch (Exception)
            {
                artistIds = null;
            }
            return artistIds;
        }
        public Guid[]? GetSongIds()
        {
            Guid[]? songIds;
            try
            {
                songIds = JsonSerializer.Deserialize<Guid[]>(SongIdsJson);
            }
            catch (Exception)
            {
                songIds = null;
            }
            return songIds;
        }
        public static AlbumData Builder(BaseItemDto baseItem, string server, Guid[]? songIds = null, SongData[]? songDataItems = null)
        {
            if (baseItem.UserData == null)
            {
                throw new ArgumentException("Cannot create Album without Album UserData! Please fix server call flags!");
            }
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Album without ID! Please fix server call flags!");
            }
            if(baseItem.ArtistItems == null)
            {
                throw new ArgumentException("Cannot create Album without ArtistItems! Please fix server call flags!");
            }

            MusicItemImage musicItemImage = MusicItemImage.Builder(baseItem, server);
            AlbumData album = new();
            album.Id = (Guid)baseItem.Id;
            album.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            album.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
            album.DateAdded = baseItem.DateCreated;
            album.DatePlayed = baseItem.UserData.LastPlayedDate;
            album.ServerAddress = server;
            album.ImgSource = musicItemImage.source;
            album.ImgBlurhash = musicItemImage.Blurhash;
            album.ArtistIdsJson = JsonSerializer.Serialize(baseItem.ArtistItems.Select(idPair => idPair.Id).ToArray());
            if(songIds != null)
            {
                album.SongIdsJson = JsonSerializer.Serialize(songIds);
            }
            if(songDataItems != null && songDataItems.Length > 0)
            {
                album.SongIdsJson = JsonSerializer.Serialize(songDataItems.Select(s => s.Id).ToArray());
                album.DatePlayed = songDataItems.OrderBy(s => s.DatePlayed).First().DatePlayed;
            }

            string artistNames = string.Empty;
            for (int i = 0; i < baseItem.ArtistItems.Count; i++)
            {
                artistNames += baseItem.ArtistItems[i].Name;
                if (i < baseItem.ArtistItems.Count - 1)
                {
                    artistNames += ", ";
                }
            }
            album.ArtistNames = artistNames;

            return album;
        }
    }
}
