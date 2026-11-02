/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： SkillManageToolOptions
*版本号： V1.0.0.0
*唯一标识：981769cc-ab36-4e59-8df7-aadad2fd7289
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 管理工具组配置（绑定到 LuBanAgent:Tools:SkillManage）
*
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