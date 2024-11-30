using System.Text.Json;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Classes.Data
{
    public class Artist : BaseMusicItem
    {
        public ArtistData GetBase => _artistData;
        public new Guid LocalId => _artistData.LocalId;
        public new Guid Id => _artistData.Id;
        public new string Name => _artistData.Name;
        public new bool IsFavourite => _artistData.IsFavourite;
        public string Description => _artistData.Description;
        public string LogoImgSource => _artistData.LogoImgSource;
        public string BackgroundImgSource => _artistData.BackgroundImgSource;
        public string BackgroundImgBlurhash => _artistData.BackgroundImgBlurhash;
        public new string ImgSource => _artistData.ImgSource;
        public new string ImgBlurhash => _artistData.ImgBlurhash;
        public new string ImgBlurhashBase64 => _artistData.BlurhashBase64;
        public Guid[] AlbumIds => _artistData.GetAlbumIds();
        public Guid[] SimilarIds => _artistData.GetSimilarIds();
        public new string ServerAddress => _artistData.ServerAddress;
        public bool IsPartial { get; set; } = false;
        public AlbumData[] Albums => _albumData;

        private readonly ArtistData _artistData;
        private readonly AlbumData[] _albumData; 

        public static Artist Empty = new Artist();
        public Artist()
        {
            _artistData = new();
            _albumData = [];
        }
        public Artist(ArtistData artistData, AlbumData[] albumData)
        {
            _artistData = artistData;
            _albumData = albumData;
            IsPartial = false;
        }
        public Artist(ArtistData artistData)
        {
            _artistData = artistData;
            _albumData = [];
            IsPartial = true;
        }
        public static Artist Builder(BaseItemDto baseItem, BaseItemDto[] albumData, string server)
        {
            if (baseItem.UserData == null)
            {
                throw new ArgumentException("Cannot create Artist without Artist UserData! Please fix server call flags!");
            }
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Artist without Artist UserData! Please fix server call flags!");
            }

            MusicItemImage artistLogo = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Logo);
            MusicItemImage artistBackdrop = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Backdrop);
            MusicItemImage artistImg = MusicItemImage.Builder(baseItem, server, ImageBuilderImageType.Primary);

            ArtistData toAdd = new();
            toAdd.Id = (Guid)baseItem.Id;
            toAdd.Name = baseItem.Name == null ? string.Empty : baseItem.Name;
            toAdd.IsFavourite = baseItem.UserData.IsFavorite == null ? false : (bool)baseItem.UserData.IsFavorite;
            toAdd.Description = baseItem.Overview == null ? string.Empty : baseItem.Overview;
            toAdd.LogoImgSource = artistLogo.Source;
            toAdd.ImgSource = artistImg.Source;
            toAdd.ImgBlurhash = artistImg.Blurhash;
            toAdd.BackgroundImgSource = artistBackdrop.Source;
            toAdd.BackgroundImgBlurhash = artistBackdrop.Blurhash;

            List<AlbumData> albums = new List<AlbumData>();
            foreach (BaseItemDto albumItem in albumData)
            {
                if(albumItem.UserData == null)
                {
                    throw new ArgumentException("Cannot create Artist without Album Data UserData! Please fix server call flags!");
                }
                if (albumItem.AlbumArtists == null)
                {
                    throw new ArgumentException("Cannot create Artist without Album Data Album Artists! Please fix server call flags!");
                }
                if (albumItem.Id == null)
                {
                    throw new ArgumentException("Cannot create Artist without Album Data ID! Please fix server call flags!");
                }

                MusicItemImage albumImg = MusicItemImage.Builder(albumItem, server, ImageBuilderImageType.Primary);

                AlbumData album = new();
                album.Id = (Guid)albumItem.Id;
                album.Name = albumItem.Name == null ? string.Empty : albumItem.Name;
                album.IsFavourite = albumItem.UserData.IsFavorite == null ? false : (bool)albumItem.UserData.IsFavorite;
                // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
                album.DateAdded = albumItem.DateCreated;
                album.ServerAddress = server;
                album.ImgSource = albumImg.Source;
                album.ImgBlurhash = albumImg.Blurhash;
                album.ArtistIdsJson = JsonSerializer.Serialize(albumItem.AlbumArtists.Select(idPair => idPair.Id).ToArray());
            }

            return new Artist(toAdd);
        }
        public static Artist Builder(NameGuidPair nameGuidPair, string server)
        {
            if (nameGuidPair == null)
            {
                return Artist.Empty;
            }

            ArtistData toAdd = new();

            MusicItemImage artistLogo = MusicItemImage.Builder(nameGuidPair, server);
            MusicItemImage artistBackdrop = MusicItemImage.Builder(nameGuidPair, server);
            MusicItemImage artistImg = MusicItemImage.Builder(nameGuidPair, server);

            Artist newArtist = new();
            toAdd.Name = nameGuidPair.Name;
            toAdd.Id = (Guid)nameGuidPair.Id;
            toAdd.ServerAddress = server;
            toAdd.LogoImgSource = artistLogo.Source;
            toAdd.ImgSource = artistImg.Source;
            toAdd.ImgBlurhash = artistImg.Blurhash;
            toAdd.BackgroundImgSource = artistBackdrop.Source;
            toAdd.BackgroundImgBlurhash = artistBackdrop.Blurhash;
            newArtist.IsPartial = true;

            return newArtist;
        }
        public void SetIsFavourite(bool state)
        {
            _artistData.IsFavourite = state;
        }
    }
}
