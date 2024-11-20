using Jellyfin.Sdk.Generated.Models;
using SQLite;
using System.Text.Json;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Database
{
    public class ArtistData
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public Guid LocalId { get; set; }
        public Guid Id { get; set; }
        public string ServerAddress { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset? DateAdded { get; set; }
        public bool IsFavourite { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string LogoImgSource { get; set; } = string.Empty;
        public string BackgroundImgSource { get; set; } = string.Empty;
        public string BackgroundImgBlurhash { get; set; } = string.Empty;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public string AlbumIdsJson { get; set;} = string.Empty;
        public bool IsPartial { get; set; } = true;
        public Guid[] GetAlbumIds()
        {
            Guid[]? guids = JsonSerializer.Deserialize<Guid[]>(AlbumIdsJson);
            return guids == null ? [] : guids;
        }

        public Guid[] GetSimilarIds()
        {
            return [];
        }
        public static ArtistData Builder(BaseItemDto baseItem, string server)
        {
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Artist without Artist Id! Please fix server call flags!");
            }
            if (baseItem.UserData == null)
            {
                throw new ArgumentException("Cannot create Artist without Artist UserData! Please fix server call flags!");
            }

            MusicItemImage artistLogo = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Logo);
            MusicItemImage artistBackdrop = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Backdrop);
            MusicItemImage artistImg = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Primary);

            ArtistData toAdd = new();
            toAdd.Id = (Guid)baseItem.Id;
            toAdd.LocalId = GuidHelper.GenerateNewGuidFromHash(toAdd.Id, server);
            toAdd.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            toAdd.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            toAdd.Description = baseItem.Overview == null ? string.Empty : baseItem.Overview;
            toAdd.LogoImgSource = artistLogo.source;
            toAdd.ImgSource = artistImg.source;
            toAdd.ImgBlurhash = artistImg.Blurhash;
            toAdd.BackgroundImgSource = artistBackdrop.source;
            toAdd.BackgroundImgBlurhash = artistBackdrop.Blurhash;

            return toAdd;
        }
    }
}
