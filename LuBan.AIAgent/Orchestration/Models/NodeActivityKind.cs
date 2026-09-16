/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： NodeActivityKind
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：子 Agent 节点内部活动类型枚举（思考/正文/工具调用/工具结果）
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 子 Agent 节点内部活动类型，用于上层 UI 还原"这个子代理到底干了什么"。
/// </summary>
public enum NodeActivityKind
{
    /// <summary>
    /// 思考过程（推理内容增量）。
    /// </summary>
    Thinking,

    /// <summary>
    /// 正文输出（可能是增量片段，上层可合并连续片段）。
    /// </summary>
    Text,

    /// <summary>
    /// 工具调用开始。
    /// </summary>
    ToolCall,

    /// <summary>
    /// 工具调用结果。
    /// </summary>
    ToolResult
}