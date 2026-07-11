using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 删除文件工具，用于在配置的文件系统根目录内删除文件，支持软删除（移动到回收站）和强制删除
/// </summary>
/// <param name="pathGuard">文件系统路径守卫</param>
/// <param name="options">文件系统工具选项</param>
[NeedApproval]
public class DeleteFileTool(FileSystemPathGuard pathGuard, FileSystemToolOptions options) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "delete_file";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Delete a file inside the configured filesystem root. Supports soft delete (recycle bin) with optional force flag.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            path = new { type = "string", description = "Root-relative path of the file to delete." },
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
        var request = JsonSerializer.Deserialize<DeleteFileRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid delete_file arguments.");

        var resolvedPath = pathGuard.ResolvePath(request.Path);

        if (!File.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"File '{request.Path}' does not exist.");
        }

        if (!IsAllowedExtension(resolvedPath))
        {
            throw new InvalidOperationException($"Deletion of '{request.Path}' is not allowed. File extension is not in the whitelist.");
        }

        if (request.Force)
        {
            File.Delete(resolvedPath);
            return new ToolResult
            {
                ToolCallId = context.ToolCall.Id,
                Content = $"Permanently deleted {pathGuard.ToRelativePath(resolvedPath)}.",
                IsSuccess = true
            };
        }

        var recycleBinPath = GetRecycleBinPath(resolvedPath);
        var relativeRecyclePath = pathGuard.ToRelativePath(recycleBinPath);
        File.Move(resolvedPath, recycleBinPath, overwrite: true);

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            Content = $"Moved to Recycle Bin: {pathGuard.ToRelativePath(resolvedPath)} → {relativeRecyclePath}",
            IsSuccess = true
        };
    }

    /// <summary>
    /// 检查文件扩展名是否允许删除
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否允许删除</returns>
    private bool IsAllowedExtension(string filePath)
    {
        var allowed = options.AllowedDeleteExtensions;
        if (allowed == null || allowed.Length == 0)
        {
            return true;
        }
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return allowed.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取回收站路径
    /// </summary>
    /// <param name="originalPath">原始路径</param>
    /// <returns>回收站路径</returns>
    private string GetRecycleBinPath(string originalPath)
    {
        var fileName = Path.GetFileName(originalPath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var recycleDir = Path.Combine(Path.GetTempPath(), "AgileAI_RecycleBin", timestamp);
        Directory.CreateDirectory(recycleDir);
        return Path.Combine(recycleDir, fileName);
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 删除文件请求
    /// </summary>
    /// <param name="Path">文件路径</param>
    /// <param name="Force">是否强制删除</param>
    private sealed record DeleteFileRequest(string Path, bool Force = false);
}
