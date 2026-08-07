/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IIdentifiable
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：可标识组件接口定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 可标识组件接口
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// 组件唯一标识
    /// </summary>
    string Id { get; }
}
