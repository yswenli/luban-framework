/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP.BuiltIn
*文件名： FileSystemMCPClient
*版本号： V1.0.0.0
*唯一标识：9f0cad7c-83fc-4c25-b27c-a6f72416281c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：文件系统 MCP 客户端（示例实现）
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件系统 MCP 客户端（示例实现）
*
*****************************************************************************/

namespace LuBan.AIAgent.MCP.BuiltIn;

/// <summary>
/// 文件系统 MCP 客户端 - 提供文件系统操作能力
/// </summary>
public class FileSystemMCPClient : MCPClientBase
{
    private readonly List<string> _allowedRoots;

    /// <summary>
    /// 服务器名称
    /// </summary>
    public override string Name => "filesystem";

    /// <summary>
    /// 服务器描述
    /// </summary>
    public override string Description => "文件系统操作工具";

    /// <summary>
    /// 创建文件系统 MCP 客户端
    /// </summary>
    /// <param name="allowedRoots">允许访问的根目录</param>
    public FileSystemMCPClient(IEnumerable<string>? allowedRoots = null)
    {
        _allowedRoots = (allowedRoots ?? Array.Empty<string>()).ToList();
    }

    /// <summary>
    /// 连接（验证配置）
    /// </summary>
    public override Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证允许的根目录是否存在
            foreach (var root in _allowedRoots)
            {
                if (!Directory.Exists(root) && !File.Exists(root))
                {
                    // 目录不存在也允许连接
                }
            }
            IsConnected = true;
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 获取工具列表
    /// </summary>
    public override Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var tools = new List<MCPTool>
        {
            new MCPTool
            {
                Name = "read_file",
                Description = "读取文件内容",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "文件路径" }
                    },
                    required = new[] { "path" }
                }.ToJson()
            },
            new MCPTool
            {
                Name = "write_file",
                Description = "写入文件内容",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "文件路径" },
                        content = new { type = "string", description = "文件内容" }
                    },
                    required = new[] { "path", "content" }
                }.ToJson()
            },
            new MCPTool
            {
                Name = "list_directory",
                Description = "列出目录内容",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "目录路径" }
                    },
                    required = new[] { "path" }
                }.ToJson()
            }
        };

        return Task.FromResult<IEnumerable<MCPTool>>(tools);
    }

    /// <summary>
    /// 调用工具
    /// </summary>
    public override async Task<MCPToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            switch (toolName)
            {
                case "read_file":
                    return await ReadFileAsync(arguments).ConfigureAwait(false);

                case "write_file":
                    return await WriteFileAsync(arguments).ConfigureAwait(false);

                case "list_directory":
                    return await ListDirectoryAsync(arguments).ConfigureAwait(false);

                default:
                    return Fail($"未知工具: {toolName}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("FileSystemMCPClient 执行工具失败", ex, toolName);
            return Fail(ex.Message);
        }
    }

    private async Task<MCPToolResult> ReadFileAsync(Dictionary<string, object?> arguments)
    {
        var path = arguments.GetValueOrDefault("path")?.ToString();
        if (string.IsNullOrEmpty(path))
            return Fail("缺少参数: path");

        if (!IsPathAllowed(path))
            return Fail($"路径不在允许访问范围内: {path}");

        if (Directory.Exists(path))
            return Fail($"路径是目录: {path}。读取文件需要文件路径，请改为传入目录下的具体文件（可用 list_directory 查看目录内容）。");

        if (!File.Exists(path))
            return Fail($"未找到文件: {path}。请检查路径是否正确。");

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > 50 * 1024 * 1024)
            return Fail($"文件过大: {fileInfo.Length / 1024 / 1024}MB，最大支持 50MB");

        // 行级切片：与内置 FileSystem 工具保持一致，避免把大文件（日志/源码）整体塞进对话上下文撑爆 token。
        // 单次最多返回 readFileMaxLines 行 / readFileMaxChars 字符，超出则截断并提示改用 Grep 或 LLM Wiki 检索。
        const int readFileMaxLines = 2000;
        const int readFileMaxChars = 256 * 1024;

        var sb = new StringBuilder();
        var truncated = false;
        var lineNo = 0;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNo++;
            if (lineNo > readFileMaxLines)
            {
                truncated = true;
                break;
            }

            if (sb.Length + line.Length + Environment.NewLine.Length > readFileMaxChars)
            {
                var budget = readFileMaxChars - sb.Length;
                if (budget > 0)
                    sb.AppendLine(line.Substring(0, Math.Min(line.Length, budget)));
                truncated = true;
                break;
            }

            sb.AppendLine(line);
        }

        if (truncated)
        {
            sb.AppendLine();
            sb.AppendLine($"[内容已截断] 源文件大小 {fileInfo.Length / 1024}KB，本次仅返回前 {Math.Min(lineNo, readFileMaxLines)} 行（约 {readFileMaxChars / 1024}KB）。");
            sb.AppendLine("如需检索特定内容请用 Grep 工具（按正则/关键字匹配，按行返回）；超大文件建议先建立 LLM Wiki 索引再提问。");
        }

        return Ok(sb.ToString());
    }

    private async Task<MCPToolResult> WriteFileAsync(Dictionary<string, object?> arguments)
    {
        var path = arguments.GetValueOrDefault("path")?.ToString();
        var content = arguments.GetValueOrDefault("content")?.ToString();

        if (string.IsNullOrEmpty(path))
            return Fail("缺少参数: path");
        if (content == null)
            return Fail("缺少参数: content");

        if (!IsPathAllowed(path))
            return Fail($"路径不在允许访问范围内: {path}");

        if (Directory.Exists(path))
            return Fail($"路径是目录: {path}。写入文件需要包含文件名的文件路径，请改为传入目录下的具体文件（可用 list_directory 查看目录内容）。");

        await File.WriteAllTextAsync(path, content);
        return Ok($"已写入文件: {path}");
    }

    private Task<MCPToolResult> ListDirectoryAsync(Dictionary<string, object?> arguments)
    {
        var path = arguments.GetValueOrDefault("path")?.ToString();
        if (string.IsNullOrEmpty(path))
            return Task.FromResult(Fail("缺少参数: path"));

        if (!IsPathAllowed(path))
            return Task.FromResult(Fail($"路径不在允许访问范围内: {path}"));

        if (!Directory.Exists(path))
            return Task.FromResult(Fail($"未找到目录: {path}。请检查路径是否正确。"));

        var entries = Directory.EnumerateFileSystemEntries(path)
            .Select(e => Path.GetFileName(e));

        return Task.FromResult(Ok(string.Join("\n", entries)));
    }

    private bool IsPathAllowed(string path)
    {
        if (_allowedRoots.Count == 0)
            return true;

        var fullPath = Path.GetFullPath(path);
        return _allowedRoots.Any(root =>
        {
            var normalizedRoot = Path.GetFullPath(root);
            if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalizedRoot += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
                   fullPath.Equals(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        });
    }
}
