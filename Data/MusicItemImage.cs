using Blurhash;
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
        public string source { get; set; } = String.Empty;
        public string blurHash { get; set; } = String.Empty;
        public int soureResolution { get; set; } = 50;
        public string sourceAtResolution { get
            {
                return SourceAtResolution(soureResolution);
            }
            private set { }
        }
        public MusicItemImageType musicItemImageType { get; set; } = MusicItemImageType.url;
        public enum MusicItemImageType
        {
            onDisk,
            url,
            base64,
        }
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
                            int scaledRed = (int)(pixel.Red * 255); scaledRed = Math.Clamp(scaledRed, 0, 255);
                            int scaledGreen = (int)(pixel.Green * 255); scaledGreen = Math.Clamp(scaledGreen, 0, 255);
                            int scaledBlue = (int)(pixel.Blue * 255); scaledBlue = Math.Clamp(scaledBlue, 0, 255);

                            // Convert int to byte
                            byte red = (byte)scaledRed;
                            byte green = (byte)scaledGreen;
                            byte blue = (byte)scaledBlue;

                            // Create SKColor
                            SKColor color = new SKColor(red, green, blue);
                            bitmap.SetPixel(x, y, color);
                            // Set the pixel color
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
        public Stream BlurHashToStream(int width = 0, int height = 0)
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
                            int scaledRed = (int)(pixel.Red * 255); scaledRed = Math.Clamp(scaledRed, 0, 255);
                            int scaledGreen = (int)(pixel.Green * 255); scaledGreen = Math.Clamp(scaledGreen, 0, 255);
                            int scaledBlue = (int)(pixel.Blue * 255); scaledBlue = Math.Clamp(scaledBlue, 0, 255);

                            // Convert int to byte
                            byte red = (byte)scaledRed;
                            byte green = (byte)scaledGreen;
                            byte blue = (byte)scaledBlue;

                            // Create SKColor
                            SKColor color = new SKColor(red, green, blue);
                            bitmap.SetPixel(x, y, color);
                            // Set the pixel color
                        }
                    }
                }

                // Convert the bitmap to a base64 string
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = new MemoryStream())
                {
                    data.SaveTo(stream);
                    return stream;
                }
            }
        }
    }
}
