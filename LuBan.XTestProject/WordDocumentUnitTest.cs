/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： WordDocumentUnitTest
*版本号： V1.0.0.0
*唯一标识：9f7cfe5e-8430-48ac-ab24-ca3f98116d92
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/28 16:20:21
*描述：word文档测试
*
*=================================================
*修改标记
*修改时间：2025/10/28 16:20:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：word文档测试
*
*****************************************************************************/
using LuBan.Office.Docx;

namespace LuBan.UnitTestProject;

/// <summary>
/// word文档测试
/// </summary>
[TestClass]
public class WordDocumentUnitTest
{
    /// <summary>
    /// TestMethod1
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        WordDocument wordDocument = new WordDocument("D:\\Users\\yswenli\\文档\\北白-课题申请书模板参考.docx");

        Assert.IsNotNull(wordDocument);

        wordDocument.ReplaceText("{{课题名称}}", "测试项目");

        var imagePath = "D:\\Users\\yswenli\\图片\\Marcus.png";

        wordDocument.ReplaceTextWithImage("{{申请人}}", imagePath);

        wordDocument.ReplaceText("{{姓名}}", "张三三");

        wordDocument.Save("D:\\Users\\yswenli\\文档\\北白-课题申请书模板参考-测试.docx");


    }
}
