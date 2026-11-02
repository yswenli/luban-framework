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
*文件名： ProviderDefaultsUnitTest
*版本号： V1.0.0.0
*唯一标识：134819c5-2d8a-4096-9610-9d893b94ead5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 15:17:42
*描述：Provider 默认 BaseUrl 与模型清单单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/26 15:17:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Provider 默认 BaseUrl 与模型清单单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Configuration.Providers;

namespace LuBan.XTestProject;

[TestClass]
public class ProviderDefaultsUnitTest
{
    [TestMethod]
    public void GetDefaultBaseUrl_KnownProvider_ReturnsUrl()
    {
        Assert.AreEqual("https://api.deepseek.com/v1/", ProviderDefaults.GetDefaultBaseUrl("deepseek"));
    }

    [TestMethod]
    public void GetDefaultBaseUrl_UnknownProvider_ReturnsNull()
    {
        Assert.IsNull(ProviderDefaults.GetDefaultBaseUrl("no-such-provider"));
    }

    [TestMethod]
    public void GetDefaultModels_KnownProvider_NotEmpty()
    {
        Assert.IsNotEmpty(ProviderDefaults.GetDefaultModels("openai").ToList());
    }

    [TestMethod]
    public void GetDefaultBaseUrl_IsCaseInsensitive()
    {
        Assert.AreEqual(ProviderDefaults.GetDefaultBaseUrl("openai"), ProviderDefaults.GetDefaultBaseUrl("OpenAI"));
    }

    [TestMethod]
    public void ProviderIds_ContainsOpenAi()
    {
        CollectionAssert.Contains(ProviderDefaults.ProviderIds.ToList(), "openai");
    }
}