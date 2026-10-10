/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SessionHistoryScopeUnitTest
*创建时间：2026/10/10
*描述：验证会话历史提供者依赖会话上下文而非进程级 CurrentSession
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Sessions;

namespace LuBan.XTestProject;

[TestClass]
public class SessionHistoryScopeUnitTest
{
    [TestMethod]
    public void Ctor_Accepts_ISessionContext()
    {
        var ctor = typeof(SessionChatHistoryProvider).GetConstructors()[0];
        var hasCtx = Array.Exists(ctor.GetParameters(), p => p.ParameterType == typeof(ISessionContext));
        Assert.IsTrue(hasCtx, "构造函数应注入 ISessionContext");
    }
}