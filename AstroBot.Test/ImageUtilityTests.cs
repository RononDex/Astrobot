using System.IO;
using AstroBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AstroBot.Test
{
    [TestClass]
    public class ImageUtilityTests
    {
        private static MemoryStream CreateGif(byte red, int width, int height)
        {
            using var image = new Image<Rgba32>(width, height, new Rgba32(red, 0, 0));
            var stream = new MemoryStream();
            image.SaveAsGif(stream);
            stream.Position = 0;
            return stream;
        }

        [TestMethod]
        public void TestMakeGrayscaleFromRGB()
        {
            var gray = ImageUtility.MakeGrayscaleFromRGB(CreateGif(200, 300, 200));

            Assert.IsNotNull(gray);
            gray.Position = 0;
            using var result = Image.Load<Rgba32>(gray);
            Assert.AreEqual(300, result.Width);
        }
    }
}
