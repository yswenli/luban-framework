namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能清单记录，包含技能的详细信息
/// </summary>
public record SkillManifest
{
    /// <summary>
    /// 技能名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// 技能描述
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// 技能版本
    /// </summary>
    public string? Version { get; init; }
    
    /// <summary>
    /// 入口模式
    /// </summary>
    public string? EntryMode { get; init; }
    
    /// <summary>
    /// 根目录路径
    /// </summary>
    public string RootDirectory { get; init; } = string.Empty;
    
    /// <summary>
    /// 技能Markdown文件路径
    /// </summary>
    public string SkillMarkdownPath { get; init; } = string.Empty;
    
    /// <summary>
    /// 触发器列表
    /// </summary>
    public IReadOnlyList<string> Triggers { get; init; } = [];
    
    /// <summary>
    /// 文件列表
    /// </summary>
    public IReadOnlyList<string> Files { get; init; } = [];
    
    /// <summary>
    /// 元数据
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
    
    /// <summary>
    /// 指令内容
    /// </summary>
    public string InstructionBody { get; init; } = string.Empty;
}
