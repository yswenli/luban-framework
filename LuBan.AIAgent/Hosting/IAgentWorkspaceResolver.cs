/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentWorkspaceResolver
*版本号： V1.0.0.0
*唯一标识：工作区解析接口
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：按会话解析工作区的契约
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

/// <summary>解析「某会话属于哪个工作区」。</summary>
public interface IAgentWorkspaceResolver
{
    /// <summary>解析会话绑定的工作区；实现应保证返回非 null（无工作区时回退默认）。</summary>
    Task<WorkspaceDescriptor> ResolveAsync(string sessionId, CancellationToken ct = default);
}