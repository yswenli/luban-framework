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
*文件名： SessionRuntimeSeed
*版本号： V1.0.0.0
*唯一标识：de1e3f2d-f1b3-4395-b91b-8261fa98d2e7
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：会话运行时初始化种子（会话/用户/工作区）
*
*****************************************************************************/
namespace LuBan.AIAgent.Hosting;

/// <summary>会话 scope 初始化数据。</summary>
/// <param name="SessionId">会话标识。</param>
/// <param name="WorkspaceId">工作区标识（可为 null）。</param>
/// <param name="WorkspaceRoot">工作区根目录绝对路径。</param>
/// <param name="AllowedRoots">允许访问的根路径快照。</param>
public sealed record SessionRuntimeSeed(
    string SessionId,
    string? WorkspaceId,
    string WorkspaceRoot,
    IReadOnlyList<string> AllowedRoots);