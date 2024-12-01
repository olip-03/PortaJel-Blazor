using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blurhash;
using OpenTK.Graphics.OpenGLES2;
using SkiaSharp;
using OpenGLES2PixelFormat = OpenTK.Graphics.OpenGLES2.PixelFormat;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PortaJel_Blazor.Classes;

public static class Blurhelper
{
    public static string BlurhashToBase64Async_OpenTK(string? blurhash, int width = 0, int height = 0,
        float brightness = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blurhash) || width <= 0 || height <= 0)
        {
            return string.Empty;
        }

        try
        {
            // Decode the Blurhash into pixel data
            Pixel[,] pixels = new Pixel[width, height];
            Core.Decode(blurhash, pixels);

            // Flatten pixel data into an array for OpenGL
            byte[] pixelData = new byte[width * height * 4]; // RGBA
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[x, y];
                    pixelData[index++] = (byte)(Math.Clamp(pixel.Red * 255 * brightness, 0, 255));
                    pixelData[index++] = (byte)(Math.Clamp(pixel.Green * 255 * brightness, 0, 255));
                    pixelData[index++] = (byte)(Math.Clamp(pixel.Blue * 255 * brightness, 0, 255));
                    pixelData[index++] = 255; // Alpha
                }
            }

            TextureTarget target = new();
            int texture;
            GL.GenTexture(out texture);
            GL.BindTexture(target, texture);

            // Upload pixel data to GPU
            GL.TexImage2D(target, 0, InternalFormat.Rgba, width, height, 0, OpenGLES2PixelFormat.Rgba,
                PixelType.UnsignedByte, pixelData);

            // Set texture parameters
            GL.TexParameterf(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameterf(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            // Create framebuffer
            int framebuffer = 0;
            GL.GenFramebuffers(1, ref framebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2d, texture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferStatus.FramebufferComplete)
            {
                return null;
            }

            // Read pixels back from the framebuffer
            byte[] outputData = new byte[width * height * 4];
            GL.ReadPixels(0, 0, width, height, OpenGLES2PixelFormat.Rgba, PixelType.UnsignedByte, outputData);

            // Cleanup
            GL.DeleteTextures(1, ref texture);
            GL.DeleteFramebuffers(1, ref framebuffer);

            // Encode pixels to Base64 string
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    bitmap.PixelFormat
                );

                // Copy pixel data
                System.Runtime.InteropServices.Marshal.Copy(outputData, 0, bitmapData.Scan0, outputData.Length);
                bitmap.UnlockBits(bitmapData);

                // Save bitmap to a memory stream as PNG
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

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