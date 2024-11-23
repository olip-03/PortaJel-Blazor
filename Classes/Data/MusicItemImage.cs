using Blurhash;
using Jellyfin.Sdk.Generated.Models;
using SkiaSharp;

namespace PortaJel_Blazor.Classes.Data
{
    public class MusicItemImage
    {
        // Variables
        public string ServerAddress { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Blurhash { get; set; } = string.Empty;
        public int SoureResolution { get; set; } = 500;
        public string SourceAtResolution => GetSourceAtResolution(SoureResolution);
        public MusicItemImageType MusicItemImageType { get; set; } = MusicItemImageType.url;

        ///<summary>
        ///A read-only instance of the Data.MusicItemImage structure with default values.
        ///</summary>
        public static readonly MusicItemImage Empty = new();

        /// <summary>
        /// Appends the URL with information that'll request it at a different size. Useful for web optimization when we dont need a full res image.
        /// </summary>
        /// <param name="px">The reqeusted resolution in pixels</param>
        /// <returns>Image at the requested resolution, by the 'px' variable.</returns>
        public string GetSourceAtResolution(int px)
        {
            if(MusicItemImageType == MusicItemImageType.url)
            {
                string toReturn = Source + $"&fillHeight={px}&fillWidth={px}&quality=96";
                return toReturn;
            }
            return Source;
        }
        public static Task<string?> BlurhashToBase64Async(string? blurhash, int width = 0, int height = 0, float brightness = 1)
        {
            if(String.IsNullOrWhiteSpace(blurhash))
            {
                return Task.FromResult<string?>(string.Empty);
            }
            try
            {            
                // TODO: SQLite DB for Blurhashes you have already decoded ;) 
                // Assuming you have a method to decode the blurhash into a pixel array
                Pixel[,] pixels = new Pixel[width, height];
                Core.Decode(blurhash, pixels);

                // Create a SkiaSharp bitmap
                using (SKBitmap bitmap = new SKBitmap(width, height))
                {
                    // Lock the pixels of the bitmap
                    using (var canvas = bitmap.PeekPixels())
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var pixel = pixels[x, y];
                                int scaledRed = (int)(pixel.Red * 255);
                                int scaledGreen = (int)(pixel.Green * 255);
                                int scaledBlue = (int)(pixel.Blue * 255);

                                // Increase brightness by multiplying with a factor (e.g., 1.2 for 20% increase)
                                float brightnessFactor = 2f * brightness; // Additional brightness
                                scaledRed = (int)(scaledRed * brightnessFactor);
                                scaledGreen = (int)(scaledGreen * brightnessFactor);
                                scaledBlue = (int)(scaledBlue * brightnessFactor);

                                // Clamp values to ensure they stay within the valid range (0-255)
                                scaledRed = Math.Clamp(scaledRed, 0, 255);
                                scaledGreen = Math.Clamp(scaledGreen, 0, 255);
                                scaledBlue = Math.Clamp(scaledBlue, 0, 255);

                                // Convert int to byte
                                byte red = (byte)scaledRed;
                                byte green = (byte)scaledGreen;
                                byte blue = (byte)scaledBlue;

                                // Create SKColor
                                SKColor color = new SKColor(red, green, blue);
                                bitmap.SetPixel(x, y, color);
                            }
                        }
                    }

                    // Convert the bitmap to a base64 string
                    using (var image = SKImage.FromBitmap(bitmap))
                        if (image != null)
                        {
                            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                            using (var stream = new MemoryStream())
                            {
                                data.SaveTo(stream);
                                byte[] imageBytes = stream.ToArray();
                                string base64String = Convert.ToBase64String(imageBytes);
                                return Task.FromResult<string?>(base64String);
                            }
                        }
                        else
                        {
                            return Task.FromResult<string?>("");
                        }
                }
            }
            catch (Exception ex)
            {
                string? noval = null;
                return Task.FromResult<string?>(noval);
            }
        }
        public static Task<string?> BlurhashToBase64Async(BaseMusicItem musicItem, int width = 0, int height = 0, float brightness = 1)
        {
            string base64 = string.Empty;
            if (musicItem is Album)
            {
                Album? album = musicItem as Album;
                return Task.FromResult<string?>(BlurhashToBase64Async(album.ImgBlurhash, 20, 20).Result);
            }
            if (musicItem is Song)
            {
                Song? song = musicItem as Song;
                return Task.FromResult<string?>(BlurhashToBase64Async(song.ImgBlurhash, 20, 20).Result);
            }
            if (musicItem is Artist)
            {
                Artist? artist = musicItem as Artist;
                return Task.FromResult<string?>(BlurhashToBase64Async(artist.ImgBlurhash, 20, 20).Result);
            }
            if (musicItem is Playlist)
            {
                Playlist? playlist = musicItem as Playlist;
                // TODO: Implementation 
                // string? imgString = await MusicItemImage.BlurhashToBase64Async(playlist.ImgBlurhash, 20, 20);
                // if (imgString != null)
                // {
                //     placeholderImages[i] = imgString;
                // }
            }
            return Task.FromResult<string?>(null);
        }
        public static MusicItemImage Builder(BaseItemDto baseItem, string server, ImageBuilderImageType? imageType = ImageBuilderImageType.Primary)
        {
            MusicItemImage image = new();
            image.ServerAddress = server;
            image.MusicItemImageType = MusicItemImageType.url;
            string imgType = "Primary";

            if(baseItem.ImageTags.AdditionalData.Count() == 0)
            {
                return MusicItemImage.Empty;
            }

            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    imgType = "Backdrop";
                    if (baseItem.ImageBlurHashes.Backdrop != null)
                    { // Set backdrop img 
                        string? hash = baseItem.ImageBlurHashes.Backdrop.AdditionalData.First().Value.ToString();
                        if (hash != null)
                        {
                            image.Blurhash = hash;
                        }
                    }
                    else if (baseItem.ImageBlurHashes.Primary != null)
                    { // if there is no backdrop, fall back to primary image
                        string? hash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
                        imgType = "Primary";
                        if (hash != null)
                        {
                            image.Blurhash = hash;
                        }
                    }
                    break;
                case ImageBuilderImageType.Logo:
                    imgType = "Logo";
                    if (baseItem.ImageBlurHashes.Logo != null)
                    {
                        string? hash = baseItem.ImageBlurHashes.Logo.AdditionalData.First().Value.ToString();
                        if (hash != null)
                        {
                            image.Blurhash = hash;
                        }
                    }
                    else
                    {
                        image.Blurhash = String.Empty;
                        return image; // bascially returning nothing if no logo is found
                    }
                    break;
                default:
                    imgType = "Primary";
                    if (baseItem.ImageBlurHashes.Primary != null)
                    {
                        string? hash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
                        if (hash != null)
                        {
                            image.Blurhash = hash;
                        }
                    }
                    else
                    {
                        image.Blurhash = String.Empty;
                    }
                    break;
            }

            if (baseItem.Type == BaseItemDto_Type.MusicAlbum)
            {
                if (baseItem.ImageBlurHashes.Primary != null)
                {
                    image.Source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                else
                {
                    image.Source = "emptyAlbum.png";
                }
            }
            else if (baseItem.Type == BaseItemDto_Type.Playlist)
            {
                image.Source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;

                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemDto_Type.Audio)
            {
                if (baseItem.AlbumId != null)
                {
                    image.Source = server + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                }
                else
                {
                    image.Source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemDto_Type.MusicArtist)
            {
                image.Source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
            }
            else if (baseItem.Type == BaseItemDto_Type.MusicGenre && baseItem.ImageBlurHashes.Primary != null)
            {
                image.Source =  server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                image.Blurhash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
            {
                image.Source = server + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                image.Blurhash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                image.Source = server + "/Items/" + baseItem.Id.ToString() + "/Images/" + imgType;
                image.Blurhash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ArtistItems != null)
            {
                image.Source = server + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/" + imgType;
            }

            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    image.Source += "?format=jpg";
                    break;
                case ImageBuilderImageType.Logo:
                    imgType = "Logo";
                    break;
                default:
                    image.Source += "?format=jpg";
                    break;
            }

            return image;
        }
        public static MusicItemImage Builder(NameGuidPair nameGuidPair, string server)
        {
            MusicItemImage image = new();

            image.MusicItemImageType = MusicItemImageType.url;
            image.Source = server + "/Items/" + nameGuidPair.Id + "/Images/Primary?format=jpg";

            return image;
        }
    }
    public enum MusicItemImageType
    {
        onDisk,
        url,
        base64,
    }
    public enum ImageBuilderImageType
    {
        Primary,
        Backdrop,
        Logo
    }
}
