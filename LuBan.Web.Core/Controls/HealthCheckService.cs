/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Controls
*文件名： HealthCheckService
*版本号： V1.0.0.0
*唯一标识：157ef796-e47d-493f-93ef-5b58a6b05622
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/14 11:07:10
*描述：健康检查服务
*
*=================================================
*修改标记
*修改时间：2024/9/14 11:07:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：健康检查服务
*
*****************************************************************************/
namespace LuBan.Web.Core.Controls;

/// <summary>
/// 健康检查服务
/// </summary>
public class HealthCheckService : BaseJobService
{
    HttpClientProxy _httpClientUtil;
    HostingOptions _hostOptions;
    string _domain;
    string _path;
    static int _errorCount;


    /// <summary>
    /// 健康检查服务（5分钟）
    /// </summary>
    public HealthCheckService() : base(5 * 60 * 1000)
    {
        _hostOptions = WebApp.HostingOptions;
        _domain = _hostOptions.Domain.Substring(0, _hostOptions.Domain.LastIndexOf("/"));
        _path = _hostOptions.Domain.Substring(_hostOptions.Domain.LastIndexOf("/"));
        _httpClientUtil = HttpClientProxy.Create(_domain);
        _errorCount = 0;
    }

    /// <summary>
    /// 执行
    /// </summary>
    public override void Run()
    {
        if (!_hostOptions.EnableHealthCheck) return;

        Result<string>? data = null;
        try
        {
            data = _httpClientUtil.Get<Result<string>>(_path);
            _errorCount = 0;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        if (data == null || data.Code != 200)
        {
            _errorCount++;
        }
        if (_errorCount >= 3)
        {
            var msg = $"【健康检测】\r\n[时间]{DateTime.Now:F}\r\n[服务]{_hostOptions.ServiceName}\r\n[域名]{_domain}\r\n[消息]当前服务已在连续3分钟内检测到联通性异常，请核查";
            WeChatRobot.Instance.SendMsg(msg);
            _errorCount = 0;
        }
    }
}
