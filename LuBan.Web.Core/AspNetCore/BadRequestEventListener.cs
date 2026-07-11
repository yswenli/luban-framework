/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： BadRequestEventListener
*版本号： V1.0.0.0
*唯一标识：c6798f76-b9b0-40e4-8ef3-df3f7105ed3e
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/11/3 13:58:33
*描述：异常请求事件监视器
*
*=================================================
*修改标记
*修改时间：2022/11/3 13:58:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：异常请求事件监视器
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 异常请求事件监视器
/// </summary>
internal class BadRequestEventListener : IObserver<KeyValuePair<string, object>>, IDisposable
{
    private readonly IDisposable _subscription;

    private readonly Action<IBadRequestExceptionFeature> _callback;

    /// <summary>
    /// 异常请求事件监视器
    /// </summary>
    /// <param name="diagnosticListener">诊断监听器</param>
    /// <param name="callback">回调函数</param>
    public BadRequestEventListener(DiagnosticListener diagnosticListener, Action<IBadRequestExceptionFeature> callback)
    {
        _subscription = diagnosticListener.Subscribe(this!, (provider) => provider switch
        {
            "Kestrel bad request" => true,
            _ => false
        });
        _callback = callback;
    }

    /// <summary>
    /// 处理下一个事件
    /// </summary>
    /// <param name="pair">事件数据</param>
    public void OnNext(KeyValuePair<string, object> pair)
    {
        if (pair.Value is IFeatureCollection featureCollection)
        {
            var badRequestFeature = featureCollection.Get<IBadRequestExceptionFeature>();

            if (badRequestFeature is not null)
            {
                _callback(badRequestFeature);
            }
        }
    }

    /// <summary>
    /// 处理错误事件
    /// </summary>
    /// <param name="error">异常信息</param>
    public void OnError(Exception error) { }

    /// <summary>
    /// 处理完成事件
    /// </summary>
    public void OnCompleted() { }

    /// <summary>
    /// 释放资源
    /// </summary>
    public virtual void Dispose() => _subscription.Dispose();
}
