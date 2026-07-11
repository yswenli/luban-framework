/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ImageUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：1194df58-6ab0-4819-8bed-f748866d4aef
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/3 17:40:11
*描述：
*
*=================================================
*修改标记
*修改时间：2025/1/3 17:40:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Common;

namespace LuBan.UnitTestProject;

/// <summary>
/// 测试绘图
/// </summary>
[TestClass]
public class ImageUtilUnitTest
{
    /// <summary>
    /// 测试绘图
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        var imageTemplateFile = @"D:\\Users\\yswenli\\图片\\elephent.jpg";
        var sourceContent = File.ReadAllBytes(imageTemplateFile);

        //插入文字
        var textInfo = new ImageInfo.TextDescription()
        {
            Value = "Elehpent,我是添添，快开门！",
            Left = 120,
            Top = 200,
            Size = 20,
            Color = "#000000",
            FontFamilyName = "微软雅黑"
        };
        //自定义文字
        //textInfo.LoadFont("C:\\Windows\\Fonts\\msyh.ttc");
        var imageInfo = new ImageInfo(textInfo);
        //指定图片
        var image = new ImageInfo.ImageDescription()
        {
            //FilePath = "D:\\Users\\yswenli\\图片\\本机照片\\yswenli.png",
            FilePath = "D:\\Users\\yswenli\\图片\\snake.png",
            Left = 150,
            Top = 220,
            MaxHeight = 150,
            MaxWidth = 150
        };
        imageInfo.Image = image;
        var targetContent = ImageUtil.DrawImage(sourceContent, imageInfo);

        Assert.IsTrue(targetContent.Length > 0);

        var targetImage = @"D:\\Users\\yswenli\\图片\\elephent2.jpg";
        File.WriteAllBytes(targetImage, targetContent);
        Assert.IsTrue(File.Exists(targetImage));
    }
}
