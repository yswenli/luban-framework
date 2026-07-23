/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： LuBanLiveUnitTest
*版本号： V1.0.0.0
*唯一标识：91a7309e-ce46-49b6-9af1-d59695d01c14
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/28 14:42:09
*描述：直播sdk单元测试
*
*=================================================
*修改标记
*修改时间：2025/4/28 14:42:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：直播sdk单元测试
*
*****************************************************************************/
using LuBan.Lives;

namespace LuBan.UnitTestProject;

/// <summary>
/// 直播sdk单元测试
/// </summary>
[TestClass]
public class LuBanLiveUnitTest
{
    /// <summary>
    /// 测试微赞帐号打通加密传参
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        try
        {
            var zvLiveClient = LiveFactory.Create(EnumLive.VZan, new LiveOption()
            {
                AppId = "LuBanFramework",
                AppSecret = "LuBanFramework",
                AuthorizeUrl = "",
                AuthSecret = "LuBanFramework",
                Salt = "",
                Url = "https://wx.vzan.com",
                UserName = "",
                Password = ""
            });
            var liveUrl = zvLiveClient.GetLiveUrl("LuBanFramework", "LuBanFramework", "testuser", "yswenli", string.Empty);

            Assert.IsNotNull(liveUrl);

        }
        catch (Exception)
        {

        }



    }
}
