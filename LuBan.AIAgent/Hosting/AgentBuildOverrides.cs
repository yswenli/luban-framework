/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： AgentBuildOverrides
*版本号： V1.0.0.0
*唯一标识：f3c7b2a1-4d69-4e08-8a5b-1f9c6e3d0a72
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：建 Agent 时的本轮覆盖项（人设 / 工具开关）
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：建 Agent 时的本轮覆盖项（人设 / 工具开关）
*
*****************************************************************************/

namespace LuBan.AIAgent.Hosting;

/// <summary>建 Agent 时的本轮覆盖项。</summary>
/// <remarks>
/// 用 <c>record</c> 值相等做「是否需要重建」的判断，不做字符串拼接。
/// 覆盖项未变时复用缓存的 Agent，避免每轮重连 Skills/MCP。
/// </remarks>
public sealed record AgentBuildOverrides
{
    /// <summary>人设段覆盖；null 用 Profile 默认。</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>false 时不装配任何工具，并压掉「教模型用工具」的提示词段落。</summary>
    public bool EnableTools { get; init; } = true;

    /// <summary>是否等同于「不覆盖」（人设为 null 且启用工具）。</summary>
    /// <remarks>
    /// <see cref="AgentHost"/> 用它把「全默认」的覆盖项归一化成 <c>null</c>：
    /// 否则每轮都会拿 non-null 去比缓存里的 null，触发无意义重建。
    /// </remarks>
    public bool IsDefault => SystemPrompt is null && EnableTools;

    /// <summary>是否与另一份覆盖项等价。</summary>
    /// <param name="other">另一份覆盖项。</param>
    /// <returns>等价返回 true。</returns>
    public bool Matches(AgentBuildOverrides? other) => Equals(other);
}