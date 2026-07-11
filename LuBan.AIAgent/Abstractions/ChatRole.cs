namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天角色枚举，定义了聊天消息的不同角色类型
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// 系统角色，用于提供系统级别的指令和上下文
    /// </summary>
    System,
    
    /// <summary>
    /// 用户角色，代表用户的输入
    /// </summary>
    User,
    
    /// <summary>
    /// 助手角色，代表AI助手的回复
    /// </summary>
    Assistant,
    
    /// <summary>
    /// 工具角色，代表工具执行的结果
    /// </summary>
    Tool
}
