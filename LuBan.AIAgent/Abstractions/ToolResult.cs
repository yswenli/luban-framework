/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
    /// 是否为 Plan 模式下"已记录计划项、本次未执行"。
    /// 为 true 时，AI 不应重试该操作，也不要将其表述为用户拒绝，
    /// 而应继续完成剩余计划说明，等待用户确认后再执行。
    /// </summary>
    public bool Planned { get; set; }

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

    /// <summary>
    /// 创建"子代理场景需用户授权、已自动阻止"的失败结果。
    /// 与 <see cref="Cancelled{T}"/> 区分：<b>不是用户拒绝</b>（UserCancelled=false），
    /// 而是子代理无权代确认、由框架确认服务按本轮已允许集合代判后阻止。
    /// </summary>
    public static ToolResult<T> SubAgentDenied<T>()
        => new()
        {
            IsSuccess = false,
            Message = "该操作需要用户授权，子代理场景已自动阻止本次调用。"
                + "请改用无需人工确认的操作（如工作区内读写），或把该步骤交由主代理直接执行。"
        };

    /// <summary>
    /// 按当前执行作用域生成拒绝结果：子代理作用域返回 <see cref="SubAgentDenied{T}"/>，
    /// 否则返回 <see cref="Cancelled{T}"/>（用户拒绝）。
    /// </summary>
    public static ToolResult<T> Denied<T>()
        => ToolConfirmationContext.IsInSubAgentScope ? SubAgentDenied<T>() : Cancelled<T>();

    /// <summary>
    /// 判断工具调用结果是否为“用户拒绝”。兼容两种形态：
    /// ① 工厂直传的原始 <see cref="ToolResult"/>；
    /// ② Microsoft.Extensions.AI 的 <c>AIFunctionFactory</c> 默认序列化后的 <see cref="JsonElement"/>
    ///    （camelCase <c>userCancelled</c>，兼容 PascalCase）。
    /// </summary>
    /// <param name="result"><c>FunctionResultContent.Result</c>。</param>
    /// <returns>用户拒绝返回 true。</returns>
    public static bool IsUserCancelled(object? result)
    {
        if (result is ToolResult toolResult)
            return toolResult.UserCancelled;

        if (result is JsonElement { ValueKind: JsonValueKind.Object } element)
        {
            if (element.TryGetProperty("userCancelled", out var camel) && camel.ValueKind == JsonValueKind.True)
                return true;
            if (element.TryGetProperty("UserCancelled", out var pascal) && pascal.ValueKind == JsonValueKind.True)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 创建 Plan 模式下的"已记录计划、未执行"结果。
    /// 与 <see cref="Cancelled{T}"/> 区分开，避免 AI 误以为用户拒绝了操作。
    /// </summary>
    public static ToolResult<T> Plan<T>()
        => new()
        {
            IsSuccess = false,
            Planned = true,
            Message = "当前处于 Plan 模式，该操作已被记录为计划项，本次未执行。"
                + "这不是用户拒绝。请不要重试该操作，继续说明剩余计划，等待用户确认后再执行。"
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
