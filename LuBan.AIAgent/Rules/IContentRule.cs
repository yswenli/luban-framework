/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： IContentRule
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：内容规则接口，提供规则内容文本
*
*****************************************************************************/
namespace LuBan.AIAgent.Rules;

/// <summary>
/// 提供内容文本的规则（如 base-behavior 引导文本）
/// </summary>
public interface IContentRule
{
    /// <summary>
    /// 规则内容文本
    /// </summary>
    string Content { get; }
}
