/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： WorkspaceRootProviderUnitTest
*创建时间：2026/10/10
*描述：验证工具插件工作区根按会话上下文解析
*
*****************************************************************************/
using LuBan.AIAgent.Tools.FileSystem;
using LuBan.AIAgent.Tools.Git;

namespace LuBan.XTestProject;

[TestClass]
public class WorkspaceRootProviderUnitTest
{
    [TestMethod]
    public void FileSystemPlugin_Ctor_Takes_ISessionContext()
    {
        var ctor = typeof(FileSystemToolPlugin).GetConstructors()[0];
        Assert.IsTrue(ctor.GetParameters().Any(p => p.ParameterType.Name == "ISessionContext"));
    }

    [TestMethod]
    public void GitPlugin_Ctor_Takes_ISessionContext()
    {
        var ctor = typeof(GitToolPlugin).GetConstructors()[0];
        Assert.IsTrue(ctor.GetParameters().Any(p => p.ParameterType.Name == "ISessionContext"));
    }
}