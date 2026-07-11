namespace LuBan.AIFlow.Models.Common;

/// <summary>
/// 通用响应模型
/// </summary>
public class CommonResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => Code == 0;
}