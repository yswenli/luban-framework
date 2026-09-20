/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ILuBanToolPlugin
*版本号： V1.0.0.0
*唯一标识：f6ea8926-6c19-44c1-a1a2-31b956e5b9b9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：工具插件接口定义
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工具插件接口定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// LuBan 工具插件接口，用于定义工具分组和提供工具函数
/// </summary>
public interface ILuBanToolPlugin
{
    /// <summary>
    /// 工具分组名称，如 "browser"、"filesystem"、"script" 等
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// 工具分组描述，供 Agent 理解用途
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 获取该分组下的所有工具函数
    /// </summary>
    /// <param name="sp">服务提供者，用于解析工具依赖</param>
    /// <param name="toolsOptions">
    /// 运行期工具参数覆盖。语义为「整组替换」：传非 null 时，本插件按 <c>toolsOptions</c> 中对应的组实例构造工具，
    /// 该组未显式赋值的成员取框架默认值（不会回落到宿主配置）；传 null 才使用宿主配置。
    /// 因此调用方应传入包含全部所需成员的对象，或保持为 null。
    /// </param>
    /// <returns>AIFunction 工具函数列表</returns>
    IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null);

    /// <summary>
    /// 是否为 opt-in 工具组：为 true 时，仅当 profile 显式在 ToolGroups 中点名才注入；
    /// ToolGroups 为 null（表示"全部"）时不包含本组。默认 false。
    /// </summary>
    bool IsOptIn => false;

    /// <summary>
    /// 根据配置判断该插件是否启用
    /// </summary>
    /// <param name="options">LuBan Agent 配置选项</param>
    /// <returns>是否启用</returns>
    bool IsEnabled(LuBanAgentOptions options);
}
