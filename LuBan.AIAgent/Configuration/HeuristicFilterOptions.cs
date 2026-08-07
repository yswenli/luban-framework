/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
/// 启发式预过滤配置。对短输入且无复合任务关键词的输入跳过 planner，节省 LLM 调用。
/// </summary>
public class HeuristicFilterOptions
{
    /// <summary>
    /// 获取或设置是否启用启发式预过滤。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置短输入阈值（字符数）。输入长度小于该值才可能被过滤。
    /// </summary>
    public int MaxLength { get; set; } = 20;

    /// <summary>
    /// 获取或设置复合任务关键词列表。短输入包含任一关键词时不跳过 planner。
    /// </summary>
    public List<string> Keywords { get; set; } = new() { "和", "同时", "然后", "并且", "另外", "还有", "分析并", "搜索并" };

    /// <summary>
    /// 判定是否应跳过 planner（直接走主 Agent 对话）。
    /// </summary>
    /// <param name="input">用户原始输入。</param>
    /// <returns>true 表示跳过 planner。</returns>
    public bool ShouldSkipPlanning(string input)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(input))
            return false;
        if (input.Length >= MaxLength)
            return false;
        return !Keywords.Any(kw => input.Contains(kw, StringComparison.OrdinalIgnoreCase));
    }
}
