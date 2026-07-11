namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天回合执行类型枚举，定义了不同的聊天回合执行模式
/// </summary>
public enum ChatTurnExecutionKind
{
    /// <summary>
    /// 会话回合，代表一个完整的用户输入和AI回复
    /// </summary>
    SessionTurn = 0,
    
    /// <summary>
    /// 会话继续，代表在现有会话基础上继续执行
    /// </summary>
    SessionContinuation = 1,
    
    /// <summary>
    /// 提示技能，代表执行提示技能
    /// </summary>
    PromptSkill = 2
}
