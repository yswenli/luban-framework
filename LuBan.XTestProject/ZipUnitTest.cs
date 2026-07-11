/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ZipUnitTest
*版本号： V1.0.0.0
*唯一标识：5a79af58-a6cf-45f6-abff-56801672d0a7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/22 13:36:25
*描述：测试压缩工具类
*
*=================================================
*修改标记
*修改时间：2025/8/22 13:36:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：测试压缩工具类
*
*****************************************************************************/
using LuBan.Common;

namespace LuBan.UnitTestProject;

/// <summary>
/// 测试压缩工具类
/// </summary>
[TestClass]
public class ZipUnitTest
{
    /// <summary>
    /// 测试从文件列表创建压缩文件
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        var filePath = PathUtil.GetRootFilePath("files");
        var fileList = new DirectoryInfo(filePath).GetFiles().Select(q => q.FullName).ToList();
        var zipPath = PathUtil.Combine(filePath, "files.zip");
        ZipUtil.CreateZipFromFiles(fileList, zipPath);
        Assert.IsTrue(File.Exists(zipPath));
    }
}
