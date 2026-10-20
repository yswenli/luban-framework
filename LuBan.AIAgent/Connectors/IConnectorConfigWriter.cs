/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： IConnectorConfigWriter
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：连接器配置写入抽象 —— 工作区连接器声明的落盘入口（宿主可替换）
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器配置写入抽象：把「对话生成 / 界面配置」的连接器声明落盘到工作区
/// <c>.luban-agent/connectors/&lt;name&gt;.json</c>。
/// </summary>
/// <remarks>
/// <para>
/// 框架提供默认实现 <see cref="WorkspaceConnectorConfigWriter"/>（纯文件写入）；
/// 产品若需要额外的缓存失效 / UI 通知 / 审计，可注册自己的实现覆盖默认（后注册者生效，
/// 框架用 <c>TryAddScoped</c> 注册默认实现）。
/// </para>
/// <para>
/// <b>凭据纪律</b>：写入前必须校验「疑似承载令牌的参数不得为明文」
/// （实现可复用 <see cref="ConnectorSettingsSecurityValidator.Validate"/>）。
/// </para>
/// </remarks>
public interface IConnectorConfigWriter
{
    /// <summary>
    /// 写入（或覆盖）一个工作区连接器声明。
    /// </summary>
    /// <param name="entry">连接器声明。</param>
    /// <param name="overwrite">是否允许覆盖同名声明。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>写入结果（<c>Data</c> 为配置文件的绝对路径）。</returns>
    Task<ToolResult<string>> WriteAsync(
        ConnectorWorkspaceEntry entry,
        bool overwrite,
        string workspaceRoot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除一个工作区连接器声明。
    /// </summary>
    /// <param name="name">连接器实例名。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>删除结果（<c>Data</c> 为配置文件路径）。</returns>
    Task<ToolResult<string>> DeleteAsync(
        string name,
        string workspaceRoot,
        CancellationToken cancellationToken = default);
}