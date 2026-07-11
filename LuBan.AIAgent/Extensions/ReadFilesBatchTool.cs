using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 批量读取文件工具，用于在单个工具调用中读取配置的文件系统根目录内的多个文本文件
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
/// <param name="options">文件系统工具选项</param>
public class ReadFilesBatchTool(FileSystemPathGuard pathGuard, FileSystemToolOptions options) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "read_files_batch";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Read multiple text files inside the configured filesystem root in a single tool call.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            paths = new
            {
                type = "array",
                description = "Root-relative file paths to read.",
                items = new { type = "string" }
            }
        },
        required = new[] { "paths" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<ReadFilesBatchRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid read_files_batch arguments.");

        if (request.Paths is null || request.Paths.Count == 0)
        {
            throw new InvalidOperationException("read_files_batch requires at least one file path.");
        }

        var builder = new StringBuilder();
        foreach (var path in request.Paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var resolvedPath = pathGuard.ResolvePath(path);
            if (!File.Exists(resolvedPath))
            {
                throw new InvalidOperationException($"File '{path}' was not found.");
            }

            var content = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
            builder.AppendLine($"Path: {pathGuard.ToRelativePath(resolvedPath)}");
            builder.AppendLine();
            if (content.Length > options.MaxReadCharacters)
            {
                builder.Append(content[..options.MaxReadCharacters]);
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine($"[Output truncated to {options.MaxReadCharacters} characters]");
            }
            else
            {
                builder.AppendLine(content);
            }

            builder.AppendLine();
            builder.AppendLine("---");
            builder.AppendLine();
        }

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = builder.ToString().TrimEnd(),
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 批量读取文件请求
    /// </summary>
    /// <param name="Paths">文件路径列表</param>
    private sealed record ReadFilesBatchRequest(IReadOnlyList<string> Paths);
}
