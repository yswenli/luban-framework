using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 创建目录工具，用于在配置的文件系统根目录内创建目录
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
[NeedApproval]
public class CreateDirectoryTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "create_directory";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Create a directory inside the configured filesystem root. Creates parent directories as needed.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative directory path to create. Creates all parent directories as needed." }
        },
        required = new[] { "path" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<CreateDirectoryRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid create_directory arguments.");

        var resolvedPath = pathGuard.ResolvePath(request.Path);
        
        // Create the directory and all parent directories
        Directory.CreateDirectory(resolvedPath);

        var relativePath = pathGuard.ToRelativePath(resolvedPath);
        return Task.FromResult(new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"Created directory: {relativePath}",
            IsSuccess = true
        });
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 创建目录请求
    /// </summary>
    /// <param name="Path">目录路径</param>
    private sealed record CreateDirectoryRequest(string Path);
}
