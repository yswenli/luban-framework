using System.Text.Json;
using System.Text.Json.Serialization;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 补丁文件工具，用于更新现有文件的内容，在修改前创建备份
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
[NeedApproval]
public class PatchFileTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "patch_file";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Patch (update) an existing file with new content. Creates a backup before modifying. Fails if file doesn't exist unless create_if_missing is true.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative path of the file to patch." },
            content = new { type = "string", description = "New content to write to the file." },
            create_if_missing = new { type = "boolean", description = "If true, creates the file if it doesn't exist. Default is false." }
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
        var request = JsonSerializer.Deserialize<PatchFileRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid patch_file arguments.");

        var resolvedPath = pathGuard.ResolvePath(request.Path);

        if (!File.Exists(resolvedPath))
        {
            if (request.CreateIfMissing)
            {
                var directory = Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllTextAsync(resolvedPath, request.Content ?? string.Empty, cancellationToken);
                return new ToolResult
                {
                    ToolCallId = context.ToolCall.Id,
                    Content = $"Created and wrote {request.Content?.Length ?? 0} characters to {pathGuard.ToRelativePath(resolvedPath)}.",
                    IsSuccess = true
                };
            }
            throw new InvalidOperationException($"File '{request.Path}' does not exist. Use create_if_missing=true to create it.");
        }

        var oldContent = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
        var oldLineCount = oldContent.Split('\n').Length;
        var newLineCount = (request.Content ?? string.Empty).Split('\n').Length;
        var lineDiff = newLineCount - oldLineCount;

        var backupPath = resolvedPath + ".bak";
        await File.WriteAllTextAsync(backupPath, oldContent, cancellationToken);

        await File.WriteAllTextAsync(resolvedPath, request.Content ?? string.Empty, cancellationToken);

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"Patched {pathGuard.ToRelativePath(resolvedPath)}. " +
                      $"Lines: {oldLineCount} → {newLineCount} ({lineDiff:+#;-#;0}). " +
                      $"Backup saved to {pathGuard.ToRelativePath(backupPath)}.",
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 补丁文件请求
    /// </summary>
    private sealed class PatchFileRequest
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; init; } = string.Empty;
        
        /// <summary>
        /// 新内容
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// 如果文件不存在是否创建
        /// </summary>
        [JsonPropertyName("create_if_missing")]
        public bool CreateIfMissing { get; init; }
    }
}
