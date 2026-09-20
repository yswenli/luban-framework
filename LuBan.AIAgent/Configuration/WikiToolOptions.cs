/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： WikiToolOptions
*版本号： V1.0.0.0
*唯一标识：5f3c9a41-7d2e-4b18-9c60-2a4f1b8e6d37
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：Wiki 工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Wiki 工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Wiki 工具组配置（opt-in 组，默认启用；靠 ToolGroups 显式点名 + IsOptIn 门控隔离）。
/// </summary>
public class WikiToolOptions
{
    /// <summary>
    /// 是否启用 wiki 工具组。默认 true，与其余 7 个工具组选项保持一致；
    /// 普通工作区靠 opt-in 门控隔离，Rag 工作区在 ToolGroups 中点名 "wiki" 后生效。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认搜索返回页数。
    /// </summary>
    public int TopK { get; set; } = 8;

    /// <summary>
    /// 搜索默认是否回落到 raw（工作区根）溯源。
    /// </summary>
    public bool IncludeRawDefault { get; set; }

    /// <summary>
    /// 单次搜索结果最大字符数。
    /// </summary>
    public int MaxResultChars { get; set; } = 8000;
}