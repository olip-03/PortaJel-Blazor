using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Artist : BaseMusicItem
    {
        /// <summary>
        /// Boolean variable to determin if the information in this class is not complete
        /// </summary>
        public bool isPartial { get; set; } = false;
        public string description { get; set; } = String.Empty;
        public string imgSrc { get; set; } = String.Empty;
        public string backgroundImgSrc { get; set; } = String.Empty;
        public string logoImgSrc { get; set; } = String.Empty;
        public MusicItemImage backgroundImage { get; set; } = new();
        public MusicItemImage logoImage { get; set; } = new();
        public Album[] artistAlbums { get; set; } = new Album[0];

        public static Artist Empty = new Artist(); 
        public static Artist Builder(BaseItemDto baseItem, string server)
        {
            if (baseItem == null)
            {
                return Artist.Empty;
            }

            Artist newArtist = new();
            newArtist.serverAddress = server;
            newArtist.name = baseItem.Name;
            newArtist.id = (Guid)baseItem.Id;
            newArtist.description = baseItem.Overview;
            newArtist.isFavourite = (bool)baseItem.UserData.IsFavorite;
            newArtist.image = MusicItemImage.Builder(baseItem, server);
            newArtist.isPartial = false;

            newArtist.backgroundImage = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Backdrop);
            newArtist.logoImage = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Logo);

            return newArtist;
        }
        public static Artist Builder(NameGuidPair nameGuidPair, string server)
        {
            if (nameGuidPair == null)
            {
                return Artist.Empty;
            }

            Artist newArtist = new();
            newArtist.name = nameGuidPair.Name;
            newArtist.id = (Guid)nameGuidPair.Id;
            newArtist.serverAddress = server;
            newArtist.image = MusicItemImage.Builder(nameGuidPair, server);
            newArtist.isPartial = true;

            return newArtist;
        }
    }
}
