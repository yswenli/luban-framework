/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentProfileResolver
*版本号： V1.0.0.0
*唯一标识：Profile 解析接口
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：按工作区解析 Agent Profile 的契约
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

/// <summary>按工作区选择 Agent Profile。</summary>
public interface IAgentProfileResolver
{
    /// <summary>选 Profile。</summary>
    IAgentProfile Resolve(WorkspaceDescriptor workspace);
}