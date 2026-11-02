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
*文件名： AgentRunRequestUnitTest
*版本号： V1.0.0.0
*唯一标识：e7b4a0c2-3d58-4f91-8a06-2c5e9d7b1f43
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：运行请求新字段与异步确认装配单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：运行请求新字段与异步确认装配单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Hosting;

namespace LuBan.XTestProject;

[TestClass]
public class AgentRunRequestUnitTest
{
    [TestMethod]
    public void NewFields_HaveSafeDefaults()
    {
        var req = new AgentRunRequest { Input = "hi" };

        Assert.AreEqual(0, req.Attachments.Count);
        Assert.IsNull(req.SystemPrompt);
        Assert.IsTrue(req.EnableTools);
        Assert.IsNull(req.ConfirmHandlerAsync);
    }

    [TestMethod]
    public async Task ConfigureForTurnAsync_UsesAsyncCallbackDirectly()
    {
        var context = new ToolConfirmationContext();
        var seen = "";

        context.ConfigureForTurnAsync(
            ToolPermissionMode.Default,
            CancellationToken.None,
            (name, args, _) => { seen = name; return Task.FromResult(ConfirmResult.Allow); });

        Assert.IsNotNull(context.Callback);
        var result = await context.Callback!("DeleteFileAsync", new Dictionary<string, object?>());

        Assert.AreEqual(ConfirmResult.Allow, result);
        Assert.AreEqual("DeleteFileAsync", seen);
    }

    [TestMethod]
    public async Task ConfigureForTurnAsync_NullCallback_LeavesNull()
    {
        var context = new ToolConfirmationContext();

        context.ConfigureForTurnAsync(
            ToolPermissionMode.Default, CancellationToken.None, null);

        Assert.IsNull(context.Callback);
        await Task.CompletedTask;
    }
}