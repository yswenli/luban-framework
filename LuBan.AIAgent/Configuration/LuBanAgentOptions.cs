/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： LuBanAgentOptions
*版本号： V1.0.0.0
*唯一标识：fde975de-ebb3-461b-b27c-bd1c7851c625
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：LuBan Agent 配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan Agent 配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// LuBan Agent 配置选项
/// </summary>
public class LuBanAgentOptions
{
    /// <summary>
    /// 默认模型名称，格式 "provider:model"
    /// </summary>
    public string? DefaultModel { get; set; }

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Agent 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 最大工具循环迭代次数
    /// </summary>
    public int MaxToolLoopIterations { get; set; } = 10;

    /// <summary>
    /// 模型端点配置字典
    /// </summary>
    public Dictionary<string, ModelEndpointOptions> Models { get; set; } = new();

    /// <summary>
    /// 工具组配置
    /// </summary>
    public ToolGroupOptions Tools { get; set; } = new();

    /// <summary>
    /// 外部插件程序集列表
    /// </summary>
    public List<string> ExternalPlugins { get; set; } = new();

    /// <summary>
    /// Session 历史与压缩配置
    /// </summary>
    public SessionOptions Session { get; set; } = new();

    /// <summary>
    /// 获取或设置编排子系统配置。
    /// </summary>
    public OrchestrationOptions? Orchestration { get; set; }

    /// <summary>
    /// Base 行为规则内容覆盖（缺省用内置中文默认文本）
    /// </summary>
    public string? BaseBehavior { get; set; }

    /// <summary>
    /// 当前工作区根目录（由宿主在初始化 Agent 时显式设置）。
    /// 用于向子代理注入路径上下文，避免子代理因不知道工作区根目录而漏传
    /// <c>rootPath</c>/<c>path</c> 等必填参数。null 表示未设置。
    /// </summary>
    public string? WorkspaceRoot { get; set; }

    /// <summary>
    /// 工具调用确认策略配置（工具名分类集合，可在配置文件中覆盖以适配自定义工具）
    /// </summary>
    public ToolConfirmationOptions Confirmation { get; set; } = new();
}

/// <summary>
/// 工具调用确认策略配置。
/// 三个数组均以工具方法名（如 RunShellAsync）为元素，用于对工具做分类筛选。
/// 语义：<b>留空 = 使用服务内置默认集合；配置了任意项 = 整体替换默认集合</b>
/// （配置绑定器对已有初始值的属性是追加而非替换，故此处默认留空，默认值由
/// <see cref="Abstractions.ToolConfirmationService"/> 持有并回退）。
/// 若需要"故意清空某类"，可在运行期改写
/// <see cref="Abstractions.IToolConfirmationService"/> 上的同名读写属性。
/// </summary>
public class ToolConfirmationOptions
{
    /// <summary>
    /// 免确认名单：命中则在需要人工确认的环节直接放行，不再打断用户。
    /// 典型用途是把受信任的 MCP 工具（名称形如 <c>mcp_{client}_{tool}</c>）或
    /// 低风险的浏览器/记忆类操作排除在确认之外。默认留空，即全部按规则确认。
    /// 注意：本名单只免除"询问用户"，不改变 Plan 模式语义——
    /// Plan 下有副作用的工具仍只记录计划项、不执行。
    /// </summary>
    public string[] AutoConfirmTools { get; set; } = [];

    /// <summary>
    /// 删除类工具集合：无论路径是否在工作区内、无论何种放行策略，都必须确认。
    /// 留空时使用内置默认集合。
    /// </summary>
    public string[] AlwaysConfirmTools { get; set; } = [];

    /// <summary>
    /// 只读工具集合：Plan 模式下这些工具无副作用，直接放行，
    /// 保证 Agent 能读取上下文产出计划；其余工具记录为计划项且不执行。
    /// 留空时使用内置默认集合。
    /// </summary>
    public string[] ReadOnlyTools { get; set; } = [];
}

/// <summary>
/// Session 历史与压缩配置
/// </summary>
public class SessionOptions
{
    /// <summary>
    /// 压缩后保留的消息数（默认 20）
    /// </summary>
    public int CompactTargetMessages { get; set; } = 20;

    /// <summary>
    /// 超出保留数多少条后触发压缩（默认 10，即超过 30 条触发）
    /// </summary>
    public int CompactThreshold { get; set; } = 10;
}