/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ToolResult
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：工具执行结果封装，支持 LLM 感知成功/失败状态与结构化数据
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工具执行结果封装
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行结果基类，用于向 LLM 返回带成功/失败标志的结构化数据
/// </summary>
public class ToolResult
{
    /// <summary>
    /// 是否执行成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 结果说明（成功时的提示，或失败时的错误信息）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 是否为用户主动取消（拒绝确认或按 ESC）。
    /// 为 true 时，AI 应停止重试同类操作，向用户说明情况后再决定下一步。
    /// </summary>
    public bool UserCancelled { get; set; }

    /// <summary>
    /// 创建一个成功结果
    /// </summary>
    public static ToolResult<T> Ok<T>(T data, string? message = null)
        => new() { IsSuccess = true, Data = data, Message = message };

    /// <summary>
    /// 创建一个失败结果
    /// </summary>
    public static ToolResult<T> Fail<T>(string message, T? data = default)
        => new() { IsSuccess = false, Message = message, Data = data };

    /// <summary>
    /// 创建一个用户取消的失败结果。
    /// Message 中明确包含停止重试指令，供 AI 遵循。
    /// </summary>
    public static ToolResult<T> Cancelled<T>()
        => new()
        {
            IsSuccess = false,
            UserCancelled = true,
            Message = "操作已被用户拒绝。请停止尝试同类操作，向用户说明情况，等待用户指示后再继续。"
        };
}

/// <summary>
/// 工具执行结果，携带具体数据类型
/// </summary>
/// <typeparam name="T">携带的数据类型</typeparam>
public class ToolResult<T> : ToolResult
{
    /// <summary>
    /// 工具返回的实际数据
    /// </summary>
    public T? Data { get; set; }
}
