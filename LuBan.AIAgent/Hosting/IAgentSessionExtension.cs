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
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentSessionExtension
*版本号： V1.0.0.0
*唯一标识：195257be-5ce1-4fc1-a7a1-54aa235d5259
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：会话级扩展点：建 Agent 前由宿主逐个应用
*
*****************************************************************************/
namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 会话级扩展点：建 Agent 前由宿主逐个调用（插件贡献、连接器启用等）。
/// </summary>
public interface IAgentSessionExtension
{
    /// <summary>在会话 scope 上应用扩展。</summary>
    Task ApplyAsync(DiContainer sessionServices, WorkspaceDescriptor workspace, CancellationToken ct = default);
}