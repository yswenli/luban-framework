/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： GlobalLubanAgentPath
*版本号： V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/4
*描述：用户级（全局）`.luban-agent` 目录解析。所有用户级自定义 skill/rule/mcp
*       统一落盘到用户主目录下的 `.luban-agent`，与框架 Playwright 驱动缓存
*       （`~/.luban-agent/playwright-driver`）约定一致。
*
*****************************************************************************/

namespace LuBan.AIAgent;

/// <summary>
/// 用户级（全局）`.luban-agent` 目录解析。
/// 路径根固定为用户主目录下的 <c>.luban-agent</c>，即
/// <c>%USERPROFILE%/.luban-agent</c>（Windows）或 <c>$HOME/.luban-agent</c>（Linux/macOS）。
/// 该目录承载跨工作区通用的自定义 skill / rule / mcp 配置。
/// </summary>
public static class GlobalLubanAgentPath
{
    /// <summary>
    /// 用户级 `.luban-agent` 根目录。
    /// </summary>
    public static string Root { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".luban-agent");

    /// <summary>
    /// 用户级 skills 目录：<c>~/.luban-agent/skills</c>。
    /// </summary>
    public static string SkillsDir { get; } = Path.Combine(Root, "skills");

    /// <summary>
    /// 用户级 rules 目录：<c>~/.luban-agent/rules</c>。
    /// </summary>
    public static string RulesDir { get; } = Path.Combine(Root, "rules");

    /// <summary>
    /// 用户级 mcps 目录：<c>~/.luban-agent/mcps</c>。
    /// </summary>
    public static string McpsDir { get; } = Path.Combine(Root, "mcps");
}
