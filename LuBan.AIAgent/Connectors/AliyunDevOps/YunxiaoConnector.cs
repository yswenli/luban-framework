/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps
*文件名： YunxiaoConnector
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：云效连接器门面类
*
*****************************************************************************/

namespace LuBan.AIAgent.Connectors.AliyunDevOps;

/// <summary>
/// 云效连接器门面类 - 聚合 CodeupClient 和 FlowClient，统一 PAT 取用
/// </summary>
public class YunxiaoConnector : IDisposable
{
    private readonly YunxiaoApiOptions _options;
    private readonly IYunxiaoCredentialProvider _credentialProvider;
    private readonly ProcessRunner? _processRunner;
    private readonly PluginPermissionGuard _guard;

    private CodeupClient? _codeupClient;
    private FlowClient? _flowClient;
    private bool _disposed;

    /// <summary>
    /// 创建云效连接器
    /// </summary>
    /// <param name="options">API 配置</param>
    /// <param name="credentialProvider">凭据提供者</param>
    /// <param name="processRunner">进程执行器（用于 Git 操作，可选）</param>
    /// <param name="permissionGuard">插件权限门禁（可选；不传则用进程级兜底实例）。</param>
    public YunxiaoConnector(
        YunxiaoApiOptions options,
        IYunxiaoCredentialProvider credentialProvider,
        ProcessRunner? processRunner = null,
        PluginPermissionGuard? permissionGuard = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        _processRunner = processRunner;
        _guard = permissionGuard ?? PluginPermissionGuard.Default;
    }

    /// <summary>
    /// 获取代码管理客户端
    /// </summary>
    public CodeupClient GetCodeupClient()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(YunxiaoConnector));

        return _codeupClient ??= new CodeupClient(_options, _credentialProvider, _processRunner, _guard);
    }

    /// <summary>
    /// 获取流水线客户端
    /// </summary>
    public FlowClient GetFlowClient()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(YunxiaoConnector));

        return _flowClient ??= new FlowClient(_options, _credentialProvider, _guard);
    }

    /// <summary>
    /// 获取凭据提供者
    /// </summary>
    public IYunxiaoCredentialProvider GetCredentialProvider() => _credentialProvider;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _codeupClient?.Dispose();
        _flowClient?.Dispose();
    }
}