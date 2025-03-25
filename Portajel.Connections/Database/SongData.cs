using Jellyfin.Sdk.Generated.Models;
using SQLite;
using System.Text.Json;
using Portajel.Connections.Data;
using PortaJel_Blazor.Classes;

namespace Portajel.Connections.Database
{
    public class SongData
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public Guid LocalId { get; set; }
        public Guid Id { get; set; }
        public string? PlaylistId { get; set; }
        public Guid AlbumId { get; set; }
        public string ArtistIdsJson { get; set; } = string.Empty;
        public string ArtistNames { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public TimeSpan Duration { get; set; } = new();
        public int PlayCount { get; set; } = 0;
        public DateTimeOffset? DateAdded { get; set; }
        public DateTimeOffset? DatePlayed { get; set; }
        public int IndexNumber { get; set; } = 0;
        public int DiskNumber { get; set; } = 0;    
        public string ServerAddress { get; set; } = string.Empty;
        public bool IsDownloaded { get; set; } = false;
        public string FileLocation { get; set; } = string.Empty;
        public string StreamUrl { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public bool IsPartial { get; set; } = true;
        public Guid[] GetArtistIds()
        {
            Guid[]? artistIds = JsonSerializer.Deserialize<Guid[]>(ArtistIdsJson);
            if (artistIds == null) return [];
            return artistIds;
        }
        public static SongData Builder(BaseItemDto baseItem, string server)
        {
            SongData song = new();

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
                baseItem.ParentId = baseItem.Id;
                // throw new ArgumentException("Cannot create Song without Parent ID! Please fix server call flags!");
            }
            if (baseItem.ArtistItems == null)
            {
                throw new ArgumentException("Cannot create Song without Artist Items! Please fix server call flags!");
            }
            if(baseItem.RunTimeTicks.HasValue)
            {   // TODO: Figure out why the fuck not all songs have a duration value..
                song.Duration = TimeSpan.FromTicks(baseItem.RunTimeTicks.Value);
            }
            MusicItemImage musicItemImage = MusicItemImage.Builder(baseItem, server);

            song.Id = (Guid)baseItem.Id;
            song.LocalId = GuidHelper.GenerateNewGuidFromHash(song.Id, server);
            song.PlaylistId = baseItem.PlaylistItemId;
            song.AlbumId = (Guid)baseItem.ParentId;
            song.ArtistIdsJson = JsonSerializer.Serialize(baseItem.ArtistItems.Select(baseItem => baseItem.Id).ToArray());
            song.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            song.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            // song.PlayCount = songData.PlayCount; TODO: Implement playcount idk
            song.DateAdded = baseItem.DateCreated;
            song.DatePlayed = baseItem.UserData.LastPlayedDate;
            song.IndexNumber = baseItem.IndexNumber == null ? 0 : (int)baseItem.IndexNumber;
            song.DiskNumber = baseItem.ParentIndexNumber == null ? 0 : (int)baseItem.ParentIndexNumber;
            song.ServerAddress = server;
            song.IsDownloaded = false; // TODO: Check if file exists idk 
            song.FileLocation = string.Empty; // TODO: Add file location
            song.StreamUrl = server + "/Audio/" + baseItem.Id + "/stream?static=true&audioCodec=adts&enableAutoStreamCopy=true&allowAudioStreamCopy=true&enableMpegtsM2TsMode=true&context=Static";
            song.ImgSource = musicItemImage.Source;
            song.ImgBlurhash = musicItemImage.Blurhash;

            string artistNames = string.Empty;
            for (int i = 0; i < baseItem.ArtistItems.Count; i++)
            {
                artistNames += baseItem.ArtistItems[i].Name;
                if (i < baseItem.ArtistItems.Count - 1)
                {
                    artistNames += ", ";
                }
            }
            song.ArtistNames = artistNames;

            return song;
        }
    }
}
