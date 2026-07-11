using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 删除目录工具，用于在配置的文件系统根目录内删除目录，支持软删除（移动到回收站）和强制删除
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
[NeedApproval]
public class DeleteDirectoryTool(FileSystemPathGuard pathGuard) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "delete_directory";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Delete a directory inside the configured filesystem root. Supports soft delete (recycle bin) with optional force flag.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative path of the directory to delete." },
            force = new { type = "boolean", description = "If true, permanently deletes instead of moving to recycle bin. Default is false (soft delete)." }
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
        var request = JsonSerializer.Deserialize<DeleteDirectoryRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid delete_directory arguments.");

        var resolvedPath = pathGuard.ResolvePath(request.Path);

        if (!Directory.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"Directory '{request.Path}' does not exist.");
        }

        if (request.Force)
        {
            Directory.Delete(resolvedPath, recursive: true);
            return new ToolResult
            {
                ToolCallId = context.ToolCall.Id,
                Content = $"Permanently deleted directory {pathGuard.ToRelativePath(resolvedPath)}.",
                IsSuccess = true
            };
        }

        var recycleBinPath = GetRecycleBinPath(resolvedPath);
        var relativeRecyclePath = pathGuard.ToRelativePath(recycleBinPath);
        Directory.Move(resolvedPath, recycleBinPath);

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"Moved to Recycle Bin: {pathGuard.ToRelativePath(resolvedPath)} → {relativeRecyclePath}",
            IsSuccess = true
        };
    }

    /// <summary>
    /// 获取回收站路径
    /// </summary>
    /// <param name="originalPath">原始路径</param>
    /// <returns>回收站路径</returns>
    private string GetRecycleBinPath(string originalPath)
    {
        var dirName = Path.GetFileName(originalPath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var recycleDir = Path.Combine(Path.GetTempPath(), "AgileAI_RecycleBin", timestamp);
        Directory.CreateDirectory(recycleDir);
        return Path.Combine(recycleDir, dirName);
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 删除目录请求
    /// </summary>
    private sealed class DeleteDirectoryRequest
    {
        /// <summary>
        /// 目录路径
        /// </summary>
        public string Path { get; init; } = string.Empty;
        
        /// <summary>
        /// 是否强制删除
        /// </summary>
        public bool Force { get; init; }
    }
}
