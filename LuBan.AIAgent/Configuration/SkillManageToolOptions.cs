/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： SkillManageToolOptions
*描述：Skill 管理工具组配置（绑定到 LuBanAgent:Tools:SkillManage）
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Skill 管理工具组配置。绑定到 <c>LuBanAgent:Tools:SkillManage</c>。
/// </summary>
public class SkillManageToolOptions
{
    /// <summary>是否启用 Skill 管理工具组。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>单个 SKILL.md 文件大小上限（字节）。</summary>
    public int MaxSkillFileBytes { get; set; } = 256 * 1024;
}