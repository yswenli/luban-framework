/****************************************************************************
* Copyright (c) YsWenLi All Rights Reserved.
* CLR版本： 4.0.30319.42000
* 机器名称：WALLE
* 公司名称：
* 命名空间：LuBan.UnitTestProject
* 文件名： ConfigUtilUnitTest
* 版本号： V1.0.0.0
* 唯一标识：b2d424b1-78a2-496e-b522-0fe581401675
* 当前的用户域：WALLE
* 创建人： YsWenLi
* 电子邮箱：$useremail$
* 创建时间：2026/3/12 13:41:47
* 描述：
*
*=====================================================================
* 修改标记
* 修改时间：2026/3/12 13:41:47
* 修改人： YsWenLi
* 版本号： V1.0.0.0
* 描述：
*
*****************************************************************************/
using LuBan.Common;
using LuBan.Web.Core;

namespace LuBan.UnitTestProject;

/// <summary>
/// Contains unit tests for the configuration utility methods.
/// </summary>
/// <remarks>This class is intended to validate the behavior of the configuration utility under various
/// scenarios. Each test method should be annotated with appropriate attributes to ensure proper execution during
/// testing.</remarks>
[TestClass]
public class ConfigUtilUnitTest
{

    /// <summary>
    /// tests the configuration utility methods to ensure they function correctly under expected conditions. This includes
    /// </summary>
    [TestMethod]
    public void ConfigUtilTest()
    {
        var t11 = ConfigUtil.Read("HostingOptions");
        var t12 = ConfigUtil.Read<HostingOptions>("HostingOptions");
        var t13 = ConfigUtil.Read<HostingOptions>();

        var t21 = ConfigUtil.Read("HostingOptions:ServiceName");
        var t22 = ConfigUtil.Read<string>("HostingOptions:ServiceName");

        var t31 = NacosConfigUtil.Read("Test:test1");
        var t32 = NacosConfigUtil.Read<bool>("Test:test1");

        Assert.IsNotNull(t11);
    }
}
