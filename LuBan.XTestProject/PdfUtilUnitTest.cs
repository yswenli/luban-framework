/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： PdfUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：64666d0c-752f-4087-aa3d-b88b7f6c58e2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/8 14:03:17
*描述：pdf工具测试
*
*=================================================
*修改标记
*修改时间：2025/8/8 14:03:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：pdf工具测试
*
*****************************************************************************/
using LuBan.Common.Data;
using LuBan.PdfKit;

namespace LuBan.UnitTestProject;


/// <summary>
/// pdf工具测试
/// </summary>
[TestClass]
public class PdfUtilUnitTest
{
    /// <summary>
    /// pdf工具测试1
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        var srcFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborServiceTemplate.pdf");
        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");
        File.Delete(dstFileName);

        PdfUtil.ReplaceText(srcFileName, dstFileName, "主要负责人", "yswenli");
        //PdfUtil.TestReplaceText(srcFileName, dstFileName, "表单", "表表单单");

        Assert.IsTrue(File.Exists(dstFileName));
    }


    /// <summary>
    /// pdf工具测试2
    /// </summary>
    [TestMethod]
    public void TestMethod2()
    {
        var srcFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborServiceTemplate.pdf");
        var dstFileName1 = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output1.pdf");
        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");
        File.Delete(dstFileName1);
        File.Delete(dstFileName);

        var data = new Dictionary<string, string>
        {
            { "'姓名'", "yswenli" },
            { "'职称'", "baba" },
            { "'医院'", "上海华山医院" },
            { "'科室'", "眼科" },
            { "'年'", "2025" },
            { "'月'", "08" },
            { "'日'", "25" },
            { "'城市'", "上海" },
            { "'金额'", "1500" },
            { "'金额大写'", 1500m.ConvertToChinese() },
            { "'身份证号'", "12332112341212121212" },
            { "'开户行'", "农行天山分行" },
            { "'银行卡卡号'", "1234567894613654987" },
            { "'手机号'", "12345678910" }
        };


        PdfUtil.ReplaceTexts(srcFileName, dstFileName1, data, @"D:\WorkBench\TeJingCai\北京白求恩\Bethune-Service\WebHost\fonts\simsun.ttc");


        var base64 = FileUtil.ReadString(Path.Combine(PathUtil.GetRootFilePath("files"), "base64png.txt"));
        var tempImagePath = Path.Combine(PathUtil.GetRootFilePath("files"), "temp.png");
        FileUtil.Write(tempImagePath, Convert.FromBase64String(base64));
        PdfUtil.ReplaceTextWithImage(dstFileName1, dstFileName, "'签字签名'", tempImagePath, 100, 35);
        File.Delete(tempImagePath);

        Assert.IsTrue(File.Exists(dstFileName1));
    }


    /// <summary>
    /// pdf工具测试3
    /// </summary>
    [TestMethod]
    public void TestMethod3()
    {
        var srcFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborServiceTemplate.pdf");
        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");
        File.Delete(dstFileName);

        var img = @"D:\Users\yswenli\图片\Marcus.png";

        PdfUtil.ReplaceTextWithImage(srcFileName, dstFileName, "'以下无正文'", img, 100, 100);

        Assert.IsTrue(File.Exists(dstFileName));
    }

    /// <summary>
    /// 测试从url生成pdf
    /// </summary>
    [TestMethod]
    public void TestMethod4()
    {
        var url = "https://www.baidu.com/";

        var fontPath = @"D:\WorkBench\TeJingCai\北京白求恩\Bethune-Service\WebHost\fonts\simsun.ttc";

        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");

        PdfUtil.FromUrl(dstFileName, url, fontPath);

        Assert.IsTrue(File.Exists(dstFileName));

    }


    /// <summary>
    /// 测试从url生成pdf
    /// </summary>
    [TestMethod]
    public void TestMethod5()
    {
        var htmlContent = "";

        var fontPath = @"D:\WorkBench\TeJingCai\北京白求恩\Bethune-Service\WebHost\fonts\simsun.ttc";

        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");

        PdfUtil.FromHtml(dstFileName, htmlContent, fontPath);

        Assert.IsTrue(File.Exists(dstFileName));

    }

    /// <summary>
    /// 测试从图片生成pdf
    /// </summary>
    [TestMethod]
    public void TestMethod6()
    {
        var imagePath = Path.Combine(PathUtil.GetRootFilePath("files"), "picture1_replace.jpg");

        var dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "LaborService_output.pdf");

        PdfUtil.FromImages(dstFileName, [imagePath]);

        Assert.IsTrue(File.Exists(dstFileName));

    }



}
