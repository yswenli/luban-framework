using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 搜索文件工具，用于在配置的文件系统根目录下搜索包含特定关键字或短语的文本文件
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
public class SearchFilesTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "search_files";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Search text files under the configured filesystem root for a keyword or phrase.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative directory to search from. Use . for the configured root." },
            query = new { type = "string", description = "Case-insensitive text to search for." },
            limit = new { type = "integer", description = "Maximum number of matching files to return.", minimum = 1, maximum = 50 }
        },
        required = new[] { "path", "query" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<SearchFilesRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid search_files arguments.");

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new InvalidOperationException("search_files requires a non-empty query.");
        }

        var resolvedPath = pathGuard.ResolvePath(request.Path);
        if (!Directory.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"Directory '{request.Path}' was not found.");
        }

        var limit = request.Limit is > 0 and <= 50 ? request.Limit.Value : 10;
        var results = new List<string>();
        foreach (var file in Directory.EnumerateFiles(resolvedPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string content;
            try
            {
                content = await File.ReadAllTextAsync(file, cancellationToken);
            }
            catch
            {
                continue;
            }

            if (!content.Contains(request.Query, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            results.Add(pathGuard.ToRelativePath(file));
            if (results.Count >= limit)
            {
                break;
            }
        }

        var output = results.Count == 0
            ? $"No files under {pathGuard.ToRelativePath(resolvedPath)} matched '{request.Query}'."
            : string.Join(Environment.NewLine, results);

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = output,
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 搜索文件请求
    /// </summary>
    /// <param name="Path">搜索目录路径</param>
    /// <param name="Query">搜索查询文本</param>
    /// <param name="Limit">结果数量限制</param>
    private sealed record SearchFilesRequest(string Path, string Query, int? Limit);
}
