using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 读取文件工具，用于读取配置文件系统根目录内的文本文件
/// </summary>
public class ReadFileTool(FileSystemPathGuard pathGuard, FileSystemToolOptions options) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "read_file";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "读取配置文件系统根目录内的文本文件。";

    /// <summary>
    /// 参数 schema，用于描述工具参数的结构
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "相对于根目录的文件路径。" }
        },
        required = new[] { "path" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<ReadFileRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("无效的read_file参数。");

        var resolvedPath = pathGuard.ResolvePath(request.Path);
        if (!File.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"文件 '{request.Path}' 未找到。");
        }

        var content = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine($"Path: {pathGuard.ToRelativePath(resolvedPath)}");
        builder.AppendLine();
        if (content.Length > options.MaxReadCharacters)
        {
            builder.Append(content[..options.MaxReadCharacters]);
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine($"[输出已截断至 {options.MaxReadCharacters} 个字符]");
        }
        else
        {
            builder.Append(content);
        }

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = builder.ToString(),
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 读取文件请求
    /// </summary>
    private sealed record ReadFileRequest(string Path);
}
