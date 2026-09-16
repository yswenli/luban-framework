/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Configuration
*文件名： HeuristicFilterOptions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：启发式预过滤配置
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 启发式预过滤配置。仅在「输入长度落在 [MinLength, MaxLength] 且包含复合任务关键词」时才进入 planner，
/// 其余情况直接走主 Agent 对话，既节省 LLM 调用，又避免把普通长问题误判为复合任务而绕开记忆召回。
/// </summary>
public class HeuristicFilterOptions
{
    /// <summary>
    /// 获取或设置是否启用启发式预过滤。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置输入长度下限（字符数，按去首尾空白后计算）。输入长度小于该值时跳过 planner。
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    /// 获取或设置输入长度上限（字符数，按去首尾空白后计算）。输入长度大于该值时跳过 planner，避免超长文本误触发编排。
    /// </summary>
    public int MaxLength { get; set; } = 200;

    /// <summary>
    /// 获取或设置是否必须命中关键词才进入 planner。为 true 时未命中任一关键词一律跳过 planner。
    /// </summary>
    public bool RequireKeyword { get; set; } = true;

    /// <summary>
    /// 获取或设置复合任务关键词列表。
    /// </summary>
    public List<string> Keywords { get; set; } = new() { "和", "同时", "然后", "并且", "另外", "还有", "分析并", "搜索并" };

    /// <summary>
    /// 判定是否应跳过 planner（直接走主 Agent 对话）。
    /// </summary>
    /// <param name="input">用户原始输入。</param>
    /// <returns>true 表示跳过 planner。</returns>
    public bool ShouldSkipPlanning(string input)
    {
        if (!Enabled)
            return false;

        if (string.IsNullOrWhiteSpace(input))
            return true;

        var length = input.Trim().Length;
        if (length < MinLength || length > MaxLength)
            return true;

        var hasKeyword = Keywords.Any(kw => !string.IsNullOrWhiteSpace(kw)
            && input.Contains(kw, StringComparison.OrdinalIgnoreCase));

        return RequireKeyword && !hasKeyword;
    }
}
