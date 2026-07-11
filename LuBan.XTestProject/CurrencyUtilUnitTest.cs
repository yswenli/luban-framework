/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： CurrencyUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：54adbef5-80e9-40ac-95cc-8b385e5980b3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/7 15:24:30
*描述：货币工具类单元测试
*
*=================================================
*修改标记
*修改时间：2025/8/7 15:24:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：货币工具类单元测试
*
*****************************************************************************/
using LuBan.Common.Data;

namespace LuBan.UnitTestProject;

/// <summary>
/// 货币工具类单元测试
/// </summary>
[TestClass]
public class CurrencyUtilUnitTest
{
    /// <summary>
    /// test
    /// </summary>

    [TestMethod]
    public void TestMethod1()
    {
        var amount = 123456789.213m;
        var result = amount.ConvertToChinese();
        Assert.AreEqual("￥", result);
    }
}