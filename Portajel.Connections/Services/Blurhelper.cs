using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blurhash;
using SkiaSharp;

namespace Portajel.Connections.Services;

public static class Blurhelper
{

    public static string BlurhashToBase64Async(string? blurhash, int width = 0, int height = 0, float brightness = 1)
    {
        if (string.IsNullOrWhiteSpace(blurhash))
            return "";

        try
        {
            // Decode blurhash into a pixel array
            Pixel[,] pixels = new Pixel[width, height];
            Core.Decode(blurhash, pixels);

            // Create and populate the bitmap
            using var bitmap = new SKBitmap(width, height);
            float brightnessFactor = 2f * brightness;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[x, y];

                    // Scale RGB values with brightness factor and clamp
                    byte red = (byte)Math.Clamp(pixel.Red * 255 * brightnessFactor, 0, 255);
                    byte green = (byte)Math.Clamp(pixel.Green * 255 * brightnessFactor, 0, 255);
                    byte blue = (byte)Math.Clamp(pixel.Blue * 255 * brightnessFactor, 0, 255);

                    bitmap.SetPixel(x, y, new SKColor(red, green, blue));
                }
            }

            // Convert bitmap to Base64 PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return Convert.ToBase64String(data.ToArray());
        }
        catch
        {
            return "";
        }
    }
}