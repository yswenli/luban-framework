using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 列出目录工具，用于列出配置的文件系统根目录内的文件和子目录
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
public class ListDirectoryTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "list_directory";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "List files and directories inside the configured filesystem root.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative directory path. Use . for the configured root." }
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
        var request = JsonSerializer.Deserialize<ListDirectoryRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid list_directory arguments.");

        var resolvedPath = pathGuard.ResolvePath(request.Path);
        if (!Directory.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"Directory '{request.Path}' was not found.");
        }

        var directories = Directory.GetDirectories(resolvedPath)
            .Select(pathGuard.ToRelativePath)
            .Select(path => path + "/");
        var files = Directory.GetFiles(resolvedPath)
            .Select(pathGuard.ToRelativePath);

        var output = string.Join(Environment.NewLine, directories.Concat(files).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(output))
        {
            output = "Directory is empty.";
        }

        return Task.FromResult(new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = output,
            IsSuccess = true
        });
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 列出目录请求
    /// </summary>
    /// <param name="Path">目录路径</param>
    private sealed record ListDirectoryRequest(string Path);
}
