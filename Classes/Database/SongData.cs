using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Data;
using SQLite;
using System.Text.Json;

namespace PortaJel_Blazor.Classes.Database
{
    public class SongData
    {
        [PrimaryKey, NotNull]
        public Guid Id { get; set; }
        public string? PlaylistId { get; set; }
        public Guid AlbumId { get; set; }
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
        public Guid[] GetArtistIds()
        {
            Guid[]? artistIds = JsonSerializer.Deserialize<Guid[]>(ArtistIdsJson);
            if (artistIds == null) return [];
            return artistIds;
        }
        public static SongData Builder(BaseItemDto baseItem, string server)
        {
            if (baseItem.UserData == null)
            {
                throw new ArgumentException("Cannot create Song without Album UserData! Please fix server call flags!");
            }
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Song without ID! Please fix server call flags!");
            }
            if (baseItem.ParentId == null)
            {
                throw new ArgumentException("Cannot create Song without Parent ID! Please fix server call flags!");
            }
            if (baseItem.ArtistItems == null)
            {
                throw new ArgumentException("Cannot create Song without Artist Items! Please fix server call flags!");
            }
            MusicItemImage musicItemImage = MusicItemImage.Builder(baseItem, server);

            SongData song = new();
            song.Id = (Guid)baseItem.Id;
            song.PlaylistId = baseItem.PlaylistItemId;
            song.AlbumId = (Guid)baseItem.ParentId;
            song.ArtistIdsJson = JsonSerializer.Serialize(baseItem.ArtistItems.Select(baseItem => baseItem.Id).ToArray());
            song.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            song.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            song.DurationMs = baseItem.CumulativeRunTimeTicks == null ? 0 : TimeSpan.FromTicks((long)baseItem.CumulativeRunTimeTicks).Milliseconds;
            // song.PlayCount = songData.PlayCount; TODO: Implement playcount idk
            song.DateAdded = baseItem.DateCreated;
            song.IndexNumber = baseItem.IndexNumber == null ? 0 : (int)baseItem.IndexNumber;
            //song.DiskNumber = songData.DiskNumber; TODO: Implement disknumber
            song.ServerAddress = server;
            song.IsDownloaded = false; // TODO: Check if file exists idk 
            song.FileLocation = string.Empty; // TODO: Add file location
            song.StreamUrl = server + "/Audio/" + baseItem.Id + "/stream?static=true&audioCodec=adts&enableAutoStreamCopy=true&allowAudioStreamCopy=true&enableMpegtsM2TsMode=true&context=Static";
            song.ImgSource = musicItemImage.source;
            song.ImgBlurhash = musicItemImage.Blurhash;

            return song;
        }
    }
}
