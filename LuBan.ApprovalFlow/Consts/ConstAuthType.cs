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
    /// API密钥认证（通过请求头或查询参数传递 API Key）
    /// </summary>
    /// <remarks>
    /// 常量名与取值一致（"apiKey"）容易造成混淆：此处的 "apiKey" 是认证方式的标识符，
    /// 并非某个具体的密钥值。配置 <c>AuthConfig.Type</c> 时使用本常量。
    /// </remarks>
    public const string ApiKey = "apiKey";
}