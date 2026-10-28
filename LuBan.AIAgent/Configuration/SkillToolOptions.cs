/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： SkillToolOptions
*版本号： V1.0.0.0
*唯一标识：c3f67a16-f111-4ee8-af98-cc7efc22f2ea
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 消费工具组配置（绑定到 LuBanAgent:Tools:Skill）
*
*****************************************************************************/

namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Skill 消费工具组配置。绑定到 <c>LuBanAgent:Tools:Skill</c>。
/// </summary>
public class SkillToolOptions
{
    /// <summary>是否启用 Skill 消费工具组。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>是否在系统提示词注入 Skill 清单。</summary>
    public bool CatalogEnabled { get; set; } = true;

    /// <summary>清单最多列出的 Skill 条数。</summary>
    public int CatalogMaxEntries { get; set; } = 50;
}