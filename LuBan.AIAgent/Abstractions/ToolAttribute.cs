/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ToolAttribute
*版本号： V1.0.0.0
*唯一标识：80d171eb-6d94-4f4f-8013-e677f232b7d3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：AI 工具方法特性标记
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AI 工具方法特性标记
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 标记方法为 AI 工具
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ToolAttribute : Attribute
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 创建 ToolAttribute 实例
    /// </summary>
    /// <param name="name">工具名称</param>
    public ToolAttribute(string name)
    {
        Name = name;
    }
}
