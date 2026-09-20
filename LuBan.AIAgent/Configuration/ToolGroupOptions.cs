/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ToolGroupOptions
*版本号： V1.0.0.0
*唯一标识：dfd78e48-4ee1-485d-a2b8-01e99287eb9b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：工具分组配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工具分组配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 工具组配置。
/// <para>
/// 注意各组成员均为非 null 默认实例：把本对象整体传给
/// <c>ILuBanToolPlugin.GetTools</c> 时，未被显式赋值的组会以
/// 「框架默认值」参与构建，而不会回落到宿主 configuration 中的
/// <c>LuBanAgent:Tools:*</c>。若要保留宿主配置，请勿传本对象，或保证显式填齐所需成员。
/// </para>
/// </summary>
public class ToolGroupOptions
{
    /// <summary>
    /// 浏览器工具配置
    /// </summary>
    public BrowserToolOptions Browser { get; set; } = new();

    /// <summary>
    /// 文件系统工具配置
    /// </summary>
    public FileSystemToolOptions FileSystem { get; set; } = new();

    /// <summary>
    /// 脚本工具配置
    /// </summary>
    public ScriptToolOptions Script { get; set; } = new();

    /// <summary>
    /// Web 工具配置
    /// </summary>
    public WebToolOptions Web { get; set; } = new();

    /// <summary>
    /// 语义检索工具配置
    /// </summary>
    public RetrievalToolOptions Retrieval { get; set; } = new();

    /// <summary>
    /// 本地长期记忆工具配置
    /// </summary>
    public LocalMemoryOptions LocalMemory { get; set; } = new();

    /// <summary>
    /// Wiki 工具组选项（opt-in）。
    /// </summary>
    public WikiToolOptions Wiki { get; set; } = new();
}
