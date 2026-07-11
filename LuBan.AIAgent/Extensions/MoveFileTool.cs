using System.Text.Json;
using System.Text.Json.Serialization;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 移动文件工具，用于在配置的文件系统根目录内移动或重命名文件或目录
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
[NeedApproval]
public class MoveFileTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "move_file";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Move or rename a file or directory within the configured filesystem root.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            source_path = new { type = "string", description = "Root-relative path of the file or directory to move." },
            destination_path = new { type = "string", description = "Root-relative destination path. Parent directories are created as needed." }
        },
        required = new[] { "source_path", "destination_path" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<MoveFileRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid move_file arguments.");

        var sourcePath = pathGuard.ResolvePath(request.SourcePath);
        var destPath = pathGuard.ResolvePath(request.DestinationPath);

        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
        {
            throw new InvalidOperationException($"Source path '{request.SourcePath}' does not exist.");
        }

        var destDirectory = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrWhiteSpace(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        if (File.Exists(sourcePath))
        {
            File.Move(sourcePath, destPath);
        }
        else
        {
            Directory.Move(sourcePath, destPath);
        }

        return Task.FromResult(new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"Moved '{pathGuard.ToRelativePath(sourcePath)}' to '{pathGuard.ToRelativePath(destPath)}'.",
            IsSuccess = true
        });
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 移动文件请求
    /// </summary>
    private sealed class MoveFileRequest
    {
        /// <summary>
        /// 源文件或目录路径
        /// </summary>
        [JsonPropertyName("source_path")]
        public string SourcePath { get; init; } = string.Empty;

        /// <summary>
        /// 目标文件或目录路径
        /// </summary>
        [JsonPropertyName("destination_path")]
        public string DestinationPath { get; init; } = string.Empty;
    }
}
