/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ConfirmResult.cs
*版本号： V1.0.0.0
*唯一标识：b050aa04-738c-49ba-9176-d01e1d0f6d74
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/20 12:01:06
*描述：ConfirmResult 类
*
*=================================================
*修改标记
*修改时间：2026/8/20 12:01:06
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConfirmResult 类
*
*****************************************************************************/

namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具确认块中用户的选择结果。
/// </summary>
public enum ConfirmResult
{
    /// <summary>允许本次调用。</summary>
    Allow,

    /// <summary>拒绝本次调用。</summary>
    Deny,

    /// <summary>本轮（当前 agent 交互回合内）后续同类工具调用全部允许，免确认。</summary>
    AllowAll
}
