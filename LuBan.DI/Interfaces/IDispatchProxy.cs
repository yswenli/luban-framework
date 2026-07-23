/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.DI
*文件名： IDispatchProxy
*版本号： V1.0.0.0
*唯一标识：fc89fc90-f5a6-47fd-b4c8-45af7dd6587a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 15:16:06
*描述：代理拦截依赖接口
*
*=================================================
*修改标记
*修改时间：2023/12/6 15:16:06
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代理拦截依赖接口
*
*****************************************************************************/
namespace LuBan.DI.Interfaces;

/// <summary>
/// 代理拦截依赖接口
/// </summary>
public interface IDispatchProxy
{
    /// <summary>
    /// 实例
    /// </summary>
    object Target { get; set; }

    /// <summary>
    /// 服务提供器
    /// </summary>
    IServiceProvider Services { get; set; }
}