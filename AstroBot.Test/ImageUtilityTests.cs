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
        public void TestMergeTogether()
        {
            var merged = ImageUtility.MergeTogether(CreateGif(200, 300, 200), CreateGif(100, 250, 300));

            Assert.IsNotNull(merged);
            merged.Position = 0;
            using var result = Image.Load<Rgba32>(merged);
            Assert.AreEqual(250, result.Width);
            Assert.AreEqual(200, result.Height);
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
