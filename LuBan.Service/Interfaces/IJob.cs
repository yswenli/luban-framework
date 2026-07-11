/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.AspnetCore
*文件名： BaseBackGroundService
*版本号： V1.0.0.0
*唯一标识：b6c190aa-2b43-4d71-9ef7-8adc69d1ddef
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/10/11 16:29:36
*描述：后台工作基类
*
*=================================================
*修改标记
*修改时间：2022/10/11 16:29:36
*修改人： yswenli
*版本号： V1.0.0.0
*描述：后台工作基类
*
*****************************************************************************/

namespace LuBan.Service.Interfaces;

/// <summary>
/// 后台服务接口。
/// </summary>
public interface IJob
{
    /// <summary>
    /// 获取或设置一个值，该值指示服务是否正在运行。
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 运行服务。
    /// </summary>
    void Run();
    /// <summary>
    /// 异步运行服务
    /// </summary>
    /// <returns></returns>
    Task RunAsync();

    /// <summary>
    /// 启动服务。
    /// </summary>
    void Start();

    /// <summary>
    /// 停止服务。
    /// </summary>
    void Stop();

    /// <summary>
    /// 停止服务（支持取消）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    void Stop(CancellationToken cancellationToken);
}
