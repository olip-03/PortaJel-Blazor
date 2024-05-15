using Blurhash;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class MusicItemImage
    {
        // Variables
        public string serverAddress { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
        public string blurHash { get; set; } = string.Empty;
        public int soureResolution { get; set; } = 500;
        public string sourceAtResolution { get
            {
                return SourceAtResolution(soureResolution);
            }
            private set { }
        }
        public MusicItemImageType musicItemImageType { get; set; } = MusicItemImageType.url;

        ///<summary>
        ///A read-only instance of the Data.MusicItemImage structure with default values.
        ///</summary>
        public static readonly MusicItemImage Empty = new();

        /// <summary>
        /// Appends the URL with information that'll request it at a different size. Useful for web optimization when we dont need a full res image.
        /// </summary>
        /// <param name="px">The reqeusted resolution in pixels</param>
        /// <returns>Image at the requested resolution, by the 'px' variable.</returns>
        public string SourceAtResolution(int px)
        {
            if(musicItemImageType == MusicItemImageType.url)
            {
                string toReturn = source + $"&fillHeight={px}&fillWidth={px}&quality=96";
                return toReturn;
            }
            return source;
        }
        public Task<string> BlurhashToBase64Async(int width = 0, int height = 0)
        {
            // Assuming you have a method to decode the blurhash into a pixel array
            Pixel[,] pixels = new Pixel[width, height];
            Blurhash.Core.Decode(blurHash, pixels);

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
                            float brightnessFactor = 2f; // Additional brightness
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
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = new MemoryStream())
                {
                    data.SaveTo(stream);
                    byte[] imageBytes = stream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return Task.FromResult<string>(base64String);
                }
            }
        }
        public static MusicItemImage Builder(BaseItemDto baseItem, string server, ImageBuilderImageType? imageType = ImageBuilderImageType.Primary)
        {
            MusicItemImage image = new();
            image.serverAddress = server;
            image.musicItemImageType = MusicItemImageType.url;
            string imgType = "Primary";
            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    imgType = "Backdrop";
                    if (baseItem.ImageBlurHashes.Backdrop != null)
                    { // Set backdrop img 
                        string? hash = baseItem.ImageBlurHashes.Backdrop.AdditionalData.First().Value.ToString();
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    else if (baseItem.ImageBlurHashes.Primary != null)
                    { // if there is no backdrop, fall back to primary image
                        string? hash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
                        imgType = "Primary";
                        if (hash != null)
                        {
                            image.blurHash = hash;
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
                            image.blurHash = hash;
                        }
                    }
                    else
                    {
                        image.blurHash = String.Empty;
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
                            image.blurHash = hash;
                        }
                    }
                    else
                    {
                        image.blurHash = String.Empty;
                    }
                    break;
            }

            if (baseItem.Type == BaseItemDto_Type.MusicAlbum)
            {
                if (baseItem.ImageBlurHashes.Primary != null)
                {
                    image.source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                else
                {
                    image.source = "emptyAlbum.png";
                }
            }
            else if (baseItem.Type == BaseItemDto_Type.Playlist)
            {
                image.source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;

                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemDto_Type.Audio)
            {
                if (baseItem.AlbumId != null)
                {
                    image.source = server + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                }
                else
                {
                    image.source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemDto_Type.MusicArtist)
            {
                image.source = server + "/Items/" + baseItem.Id + "/Images/" + imgType;
            }
            else if (baseItem.Type == BaseItemDto_Type.MusicGenre && baseItem.ImageBlurHashes.Primary != null)
            {
                image.source =  server + "/Items/" + baseItem.Id + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
            {
                image.source = server + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = server + "/Items/" + baseItem.Id.ToString() + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ArtistItems != null)
            {
                image.source = server + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/" + imgType;
            }

            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    image.source += "?format=jpg";
                    break;
                case ImageBuilderImageType.Logo:
                    imgType = "Logo";
                    break;
                default:
                    image.source += "?format=jpg";
                    break;
            }

            return image;
        }
        public static MusicItemImage Builder(NameGuidPair nameGuidPair, string server)
        {
            MusicItemImage image = new();

            image.musicItemImageType = MusicItemImageType.url;
            image.source = server + "/Items/" + nameGuidPair.Id + "/Images/Primary?format=jpg";

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
