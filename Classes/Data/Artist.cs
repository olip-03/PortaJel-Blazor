using System.Text.Json;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Classes.Data
{
    public class Artist : BaseMusicItem
    {
        public ArtistData GetBase => _artistData;
        
        
        
        
        public string Description => _artistData.Description;
        public string LogoImgSource => _artistData.LogoImgSource;
        public string BackgroundImgSource => _artistData.BackgroundImgSource;
        public string BackgroundImgBlurhash => _artistData.BackgroundImgBlurhash;
        public Guid[] AlbumIds => _artistData.GetAlbumIds();
        public Guid[] SimilarIds => _artistData.GetSimilarIds();
        public bool IsPartial { get; set; } = false;
        public AlbumData[] Albums { get; }

        private readonly ArtistData _artistData;

        public static readonly Artist Empty = new Artist();
        public Artist()
        {
            _artistData = new();
            Albums = [];
            SetVars();
        }
        public Artist(ArtistData artistData, AlbumData[] albumData)
        {
            _artistData = artistData;
            Albums = albumData;
            IsPartial = false;
            SetVars();
        }
        public Artist(ArtistData artistData)
        {
            _artistData = artistData;
            Albums = [];
            IsPartial = true;
            SetVars();
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

            ArtistData toAdd = new()
            {
                Id = (Guid)baseItem.Id,
                Name = baseItem.Name ?? string.Empty,
                IsFavourite = baseItem.UserData.IsFavorite ?? false,
                Description = baseItem.Overview ?? string.Empty,
                LogoImgSource = artistLogo.Source,
                ImgSource = artistImg.Source,
                ImgBlurhash = artistImg.Blurhash,
                BackgroundImgSource = artistBackdrop.Source,
                BackgroundImgBlurhash = artistBackdrop.Blurhash
            };

            var albums = new List<AlbumData>();
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

                AlbumData album = new()
                {
                    Id = (Guid)albumItem.Id,
                    Name = albumItem.Name ?? string.Empty,
                    IsFavourite = albumItem.UserData.IsFavorite ?? false,
                    // album.PlayCount = albumData.PlayCount; TODO: Implement playcount
                    DateAdded = albumItem.DateCreated,
                    ServerAddress = server,
                    ImgSource = albumImg.Source,
                    ImgBlurhash = albumImg.Blurhash,
                    ArtistIdsJson = JsonSerializer.Serialize(albumItem.AlbumArtists.Select(idPair => idPair.Id).ToArray())
                };
                albums.Add(album);
            }

            return new Artist(toAdd, albums.ToArray());
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
        private void SetVars()
        {
            Id = _artistData.Id;
            LocalId = _artistData.LocalId;
            Name = _artistData.Name;
            IsFavourite = _artistData.IsFavourite;
            ImgSource = _artistData.ImgSource;
            ImgBlurhash = _artistData.ImgBlurhash;
            ImgBlurhashBase64 = _artistData.BlurhashBase64;
            ServerAddress = _artistData.ServerAddress;
        }
    }
}
