using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;

namespace AstroBot.Utilities
{
    public class ImageUtility
    {
        /// <summary>
        /// Creates a grey scale image out of a single channel RGB image (where only R channel has data)
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MemoryStream? MakeGrayscaleFromRGB(MemoryStream original)
        {
            // Sometimes the image does not exist
            if (original.Length < 1000)
            {
                return null;
            }

            var image = Image.Load(original);
            image.Mutate(x => x.Grayscale());
            var changedImage = new MemoryStream();
            image.SaveAsJpeg(changedImage, new JpegEncoder { Quality = 85 });
            return changedImage;
        }

        /// <summary>
        /// Creates a grey scale image out of a single channel RGB image (where only R channel has data)
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MemoryStream? MergeTogether(MemoryStream originalRedImage, MemoryStream originalBlueImage)
        {
            // Sometimes the blue image does not exist
            if (originalBlueImage.Length < 1000)
            {
                return MakeGrayscaleFromRGB(originalRedImage);
            }

            var redImage = Image.Load<Rgba32>(originalRedImage);
            var blueImage = Image.Load<Rgba32>(originalBlueImage);
            redImage.Mutate(x => x.Grayscale());
            blueImage.Mutate(x => x.Grayscale());

            var minHeight = new[] { redImage.Height, blueImage.Height }.Min();
            var minWidth = new[] { redImage.Width, blueImage.Width }.Min();

            var colorImage = new Image<Rgba32>(minWidth, minHeight);

            redImage.ProcessPixelRows(colorImage, blueImage, (redImageAccessor, colorImageAccessor, blueImageAccessor) =>
            {
                for (int y = 0; y < minHeight; y++)
                {
                    Span<Rgba32> pixelRowSpanRed = redImageAccessor.GetRowSpan(y);
                    Span<Rgba32> pixelRowSpanBlue = blueImageAccessor.GetRowSpan(y);
                    Span<Rgba32> colorImageRowSpan = colorImageAccessor.GetRowSpan(y);

                    for (int x = 0; x < minWidth; x++)
                    {
                        colorImageRowSpan[x] = new Rgba32(
                                pixelRowSpanRed[x].R,
                                Convert.ToByte((0.5 * pixelRowSpanRed[x].R) + (0.5 * pixelRowSpanBlue[x].R)),
                                pixelRowSpanBlue[x].R);
                    }
                }
            });

            var changedImage = new MemoryStream();
            colorImage.SaveAsJpeg(changedImage, new JpegEncoder { Quality = 85 });
            return changedImage;
        }
    }
}