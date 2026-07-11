using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 写入文件工具，用于向配置文件系统根目录内写入文本文件
/// </summary>
[NeedApproval]
public class WriteFileTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "write_file";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "向配置文件系统根目录内写入文本文件。";

    /// <summary>
    /// 参数 schema，用于描述工具参数的结构
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "相对于根目录的文件路径。" },
            content = new { type = "string", description = "要写入文件的文本内容。" }
        },
        required = new[] { "path", "content" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<WriteFileRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("无效的write_file参数。");

        var resolvedPath = pathGuard.ResolvePath(request.Path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(resolvedPath, request.Content ?? string.Empty, cancellationToken);
        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"已向 {pathGuard.ToRelativePath(resolvedPath)} 写入 {request.Content?.Length ?? 0} 个字符。",
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 写入文件请求
    /// </summary>
    private sealed record WriteFileRequest(string Path, string Content);
}
