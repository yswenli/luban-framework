/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： NacosConfigUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：6fa0a22e-963a-4538-b790-841dd4ec80d7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/3 10:10:15
*描述：
*
*=================================================
*修改标记
*修改时间：2025/4/3 10:10:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Common;

namespace LuBan.UnitTestProject;

/// <summary>
/// NacosConfigUtilUnitTest
/// </summary>
[TestClass]
public class NacosConfigUtilUnitTest
{
    /// <summary>
    /// 测试读取配置
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        var env = NacosConfigUtil.GetEnvironment();
        Assert.IsTrue(!string.IsNullOrEmpty(env));
    }
}
