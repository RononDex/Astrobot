using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace AstroBot.Utilities
{
    public class ImageUtility
    {
        /// <summary>
        /// Creates a grey scale image out of a single channel RGB image (where only R channel has data)
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MemoryStream MakeGrayscaleFromRGB(MemoryStream original)
        {
            var image = Image.Load(original);
            image.Mutate(x => x.Grayscale());
            var changedImage = new MemoryStream();
            image.SaveAsJpeg(changedImage, new JpegEncoder { Quality = 85 });
            return changedImage;
        }
    }
}