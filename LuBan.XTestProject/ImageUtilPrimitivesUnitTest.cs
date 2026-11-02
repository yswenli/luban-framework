/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： ImageUtilPrimitivesUnitTest
*版本号： V1.0.0.0
*唯一标识：ae567571-c0bd-4556-8a98-7873fdf0bbab
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/13 11:01:51
*描述：ImageUtil 缩放、编码与缩略图等基础图元单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/13 11:01:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ImageUtil 缩放、编码与缩略图等基础图元单元测试
*
*****************************************************************************/

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
    public void ScaleToFit_TruncatesRatherThanRounds()
    {
        // ratio = 5/9 ≈ 0.5556 -> truncating (5,3); rounding would give (5,4)
        Assert.AreEqual((5, 3), ImageUtil.ScaleToFit(9, 7, 5, 5));
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
            using var decoded = SKBitmap.Decode(path);
            Assert.AreEqual(64, decoded.Width);
            Assert.AreEqual(32, decoded.Height);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
