namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 认证类型常量，用于定义HTTP节点的认证方式
/// </summary>
public class ConstAuthType
{
    /// <summary>
    /// 无认证
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// 基础认证（Basic Authentication）
    /// </summary>
    public const string Basic = "basic";

    /// <summary>
    /// Bearer Token认证
    /// </summary>
    public const string Bearer = "bearer";

    /// <summary>
    /// API密钥认证
    /// </summary>
    public const string ApiKey = "apiKey";
}