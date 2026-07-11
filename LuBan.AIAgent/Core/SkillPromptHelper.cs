using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 技能提示助手，用于构建和管理技能相关的提示信息
/// </summary>
public static class SkillPromptHelper
{
    /// <summary>
    /// 标记前缀
    /// </summary>
    public const string MarkerPrefix = "[LuBan.AIAgent Skill Prompt]";

    /// <summary>
    /// 构建系统提示
    /// </summary>
    /// <param name="manifest">技能清单</param>
    /// <returns>系统提示字符串</returns>
    public static string BuildSystemPrompt(SkillManifest manifest)
    {
        return $"""
{MarkerPrefix} Name={manifest.Name}
You are executing the LuBan.AIAgent local skill: {manifest.Name}

Skill name: {manifest.Name}
Skill description: {manifest.Description}
Skill version: {manifest.Version}
Skill entry mode: {manifest.EntryMode}
Skill root directory: {manifest.RootDirectory}
Skill markdown path: {manifest.SkillMarkdownPath}

Skill instructions:
{manifest.InstructionBody}

Use these instructions as task-specific behavior. If the skill references relative files, resolve them relative to the skill root directory.
""";
    }

    /// <summary>
    /// 判断消息是否为生成的技能提示
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>是否为生成的技能提示</returns>
    public static bool IsGeneratedSkillPrompt(ChatMessage message)
    {
        return message.Role == ChatRole.System &&
               message.TextContent?.StartsWith(MarkerPrefix, StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// 判断消息是否为特定技能的生成提示
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <param name="skillName">技能名称</param>
    /// <returns>是否为特定技能的生成提示</returns>
    public static bool IsGeneratedSkillPromptFor(ChatMessage message, string skillName)
    {
        return message.Role == ChatRole.System &&
               message.TextContent?.StartsWith($"{MarkerPrefix} Name={skillName}", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// 为技能准备历史记录
    /// </summary>
    /// <param name="history">历史消息</param>
    /// <param name="manifest">技能清单</param>
    /// <returns>准备好的历史记录</returns>
    public static IReadOnlyList<ChatMessage> PrepareHistoryForSkill(IReadOnlyList<ChatMessage>? history, SkillManifest manifest)
    {
        var filtered = (history ?? [])
            .Where(m => !IsGeneratedSkillPrompt(m))
            .ToList();

        filtered.Insert(0, ChatMessage.System(BuildSystemPrompt(manifest)));
        return filtered;
    }
}
