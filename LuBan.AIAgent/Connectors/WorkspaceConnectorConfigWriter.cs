/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： WorkspaceConnectorConfigWriter
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：默认连接器配置写入器 —— 落盘到 .luban-agent/connectors/<name>.json
*
*****************************************************************************/
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 默认连接器配置写入器：把连接器声明落盘到工作区 <c>.luban-agent/connectors/&lt;name&gt;.json</c>。
/// </summary>
/// <remarks>
/// 纯文件写入，不依赖任何宿主持久层；产品可注册自己的实现覆盖本类（如需缓存失效 / UI 通知）。
/// 写入前执行「明文凭据」门禁（见 <see cref="ConnectorSettingsSecurityValidator"/>）。
/// </remarks>
public sealed class WorkspaceConnectorConfigWriter : IConnectorConfigWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public async Task<ToolResult<string>> WriteAsync(
        ConnectorWorkspaceEntry entry,
        bool overwrite,
        string workspaceRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (string.IsNullOrWhiteSpace(workspaceRoot))
            return ToolResult.Fail<string>("工作区根目录不能为空。");

        var name = entry.ResolveName(string.Empty);
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("连接器名称不能为空。");

        if (!IsValidFileName(name))
            return ToolResult.Fail<string>("连接器名称不能包含路径分隔符或非法文件名字符。");

        var securityError = ConnectorSettingsSecurityValidator.Validate(entry.Settings);
        if (securityError is not null)
            return ToolResult.Fail<string>(securityError);

        var configFile = Path.Combine(workspaceRoot, ".luban-agent", "connectors", $"{name}.json");

        if (File.Exists(configFile) && !overwrite)
            return ToolResult.Fail<string>($"连接器 '{name}' 已存在，需设置 overwrite=true 覆盖。");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configFile)!);
            var json = Serialize(entry, name);
            await File.WriteAllTextAsync(configFile, json, cancellationToken).ConfigureAwait(false);

            return ToolResult.Ok(configFile, $"连接器 '{name}' 配置已写入 {configFile}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"写入连接器配置失败: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<ToolResult<string>> DeleteAsync(
        string name,
        string workspaceRoot,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(ToolResult.Fail<string>("连接器名称不能为空。"));

        if (!IsValidFileName(name))
            return Task.FromResult(ToolResult.Fail<string>("连接器名称不能包含路径分隔符或非法文件名字符。"));

        var configFile = Path.Combine(workspaceRoot, ".luban-agent", "connectors", $"{name.Trim()}.json");
        if (!File.Exists(configFile))
            return Task.FromResult(ToolResult.Fail<string>($"连接器 '{name}' 配置文件不存在。"));

        try
        {
            File.Delete(configFile);
            return Task.FromResult(ToolResult.Ok(configFile, $"连接器 '{name}' 已删除。"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Fail<string>($"删除连接器失败: {ex.Message}"));
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "JSON 序列化仅用于简单配置类型，已通过 JsonSerializerOptions 处理")]
    private static string Serialize(ConnectorWorkspaceEntry entry, string name)
    {
        var normalized = new ConnectorWorkspaceEntry
        {
            Name = name,
            Provider = string.IsNullOrWhiteSpace(entry.Provider) ? null : entry.Provider!.Trim(),
            Settings = entry.Settings is null || entry.Settings.Count == 0
                ? null
                : new Dictionary<string, string?>(entry.Settings, StringComparer.OrdinalIgnoreCase),
            Enabled = entry.Enabled
        };

        return JsonSerializer.Serialize(normalized, JsonOptions);
    }

    /// <summary>
    /// 校验名称可作为配置文件名（防路径穿越）。
    /// </summary>
    /// <param name="name">连接器名称。</param>
    /// <returns>合法返回 <c>true</c>。</returns>
    private static bool IsValidFileName(string name)
        => name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
           && !name.Contains("..", StringComparison.Ordinal);
}