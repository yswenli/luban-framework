/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools
*文件名： WorkspaceRootFallbackAIFunction
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：标记类型：表示该工具对路径参数具备“工作区根兜底”，免于必填参数守卫拦截
*
*****************************************************************************/
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Tools;

/// <summary>
/// 标记类型：由 <see cref="AIFunctionFactoryHelper"/> 在提供了工作区根兜底时包装产生。
/// <para>
/// 语义：该工具的路径参数漏传时会自动回退到工作区根（如 ListDirectory/GetWorkspaceOverview/
/// SearchFiles/Grep 等只读发现类工具），因此不应被 <see cref="RequiredArgumentGuardAIFunction"/>
/// 以“缺少必填参数”拦截。
/// </para>
/// <para>
/// 面向具体文件/目录的读写删改工具不得使用此标记——静默回退会产生
/// “路径被误识别为目录”“返回工作区统计”等误导结果。
/// </para>
/// </summary>
public sealed class WorkspaceRootFallbackAIFunction : DelegatingAIFunction
{
    /// <summary>
    /// 创建工作区根兜底标记函数
    /// </summary>
    /// <param name="innerFunction">被标记的工具函数</param>
    public WorkspaceRootFallbackAIFunction(AIFunction innerFunction)
        : base(innerFunction)
    {
    }
}