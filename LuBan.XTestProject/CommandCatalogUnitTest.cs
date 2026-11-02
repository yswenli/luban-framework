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
*文件名： CommandCatalogUnitTest
*版本号： V1.0.0.0
*唯一标识：85f3546a-61da-4304-98c1-6bde99a016fd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 15:12:20
*描述：命令目录单元测试（前缀匹配与客户端可用性过滤）
*
*=================================================
*修改标记
*修改时间：2026/10/26 15:12:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：命令目录单元测试（前缀匹配与客户端可用性过滤）
*
*****************************************************************************/

using LuBan.AIAgent.Hosting.Commands;

namespace LuBan.XTestProject;

[TestClass]
public class CommandCatalogUnitTest
{
    private readonly CommandCatalog _catalog = new();

    [TestMethod]
    public void All_ContainsCoreCommands()
    {
        var names = _catalog.All.Select(c => c.Name).ToList();
        CollectionAssert.Contains(names, "/model");
        CollectionAssert.Contains(names, "/help");
    }

    [TestMethod]
    public void MatchByPrefix_EmptyPrefix_ReturnsOnlyClientSupported()
    {
        var cli = _catalog.MatchByPrefix("/", SlashCommandClient.Cli);

        Assert.IsTrue(cli.All(c => c.Supports(SlashCommandClient.Cli)));
    }

    [TestMethod]
    public void MatchByPrefix_FiltersByText()
    {
        var result = _catalog.MatchByPrefix("/mo", SlashCommandClient.Both);

        Assert.IsTrue(result.Any(c => c.Name == "/model"));
        Assert.IsFalse(result.Any(c => c.Name == "/help"));
    }
}