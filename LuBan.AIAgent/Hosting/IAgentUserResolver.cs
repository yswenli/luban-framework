/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentUserResolver
*版本号： V1.0.0.0
*唯一标识：d6629770-8c3b-4fba-82be-e00a9cc8fc42
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：用户标识解析契约
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

/// <summary>解析当前会话归属用户。</summary>
public interface IAgentUserResolver
{
    /// <summary>返回用户标识（不得返回 null 或空串）。</summary>
    string ResolveUserId(IServiceProvider services);
}