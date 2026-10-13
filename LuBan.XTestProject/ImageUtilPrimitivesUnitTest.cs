using LuBan.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace LuBan.XTestProject;

[TestClass]
public class ImageUtilPrimitivesUnitTest
{
    [TestMethod]
    public void ScaleToFit_ShrinksWithinBox_Truncating()
    {
        Assert.AreEqual((100, 50), ImageUtil.ScaleToFit(200, 100, 100, 100));
    }

    [TestMethod]
    public void ScaleToFit_NoUpscale_WhenSmallerThanBox()
    {
        Assert.AreEqual((10, 20), ImageUtil.ScaleToFit(10, 20, 100, 100));
    }

    [TestMethod]
    public void Resize_ProducesExactDimensions()
    {
        using var src = new SKBitmap(200, 100);
        using var resized = ImageUtil.Resize(src, 50, 25);
        Assert.AreEqual(50, resized.Width);
        Assert.AreEqual(25, resized.Height);
    }

    [TestMethod]
    public void Encode_ReturnsNonEmptyBytes()
    {
        using var bmp = new SKBitmap(8, 8);
        var bytes = ImageUtil.Encode(bmp, SKEncodedImageFormat.Png, 100);
        Assert.IsTrue(bytes.Length > 0);
    }

    [TestMethod]
    public void SaveThumbnail_WritesJpegFile()
    {
        using var bmp = new SKBitmap(200, 100);
        var path = ImageUtil.SaveThumbnail(bmp);
        try
        {
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
