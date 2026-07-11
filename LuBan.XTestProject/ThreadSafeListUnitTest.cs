/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ThreadSafeListUnitTest
*版本号： V1.0.0.0
*唯一标识：3ae48976-6cd3-4c8d-94a6-5a882326dc9c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/10 11:26:16
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/10 11:26:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.UnitTestProject;

/// <summary>
/// ThreadSafeListUnitTest
/// </summary>
[TestClass]
public class ThreadSafeListUnitTest
{
    /// <summary>
    /// Test1
    /// </summary>
    [TestMethod]
    public void Test1()
    {
        var list = new ThreadSafeList<int>();

        var count = list.Count();

        Assert.AreEqual(0, count);
    }
}
