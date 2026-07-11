/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow
*文件名： QingflowOptions
*版本号： V1.0.0.0
*唯一标识：9332a27e-a866-478b-97e7-93a170e3c886
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 16:04:55
*描述：轻流配置
*
*=================================================
*修改标记
*修改时间：2024/12/23 16:04:55
*修改人： yswenli
*版本号： V1.0.0.0
*描述：轻流配置
*
*****************************************************************************/
namespace LuBan.Qingflow;

/// <summary>
/// 轻流配置
/// </summary>
public class QingflowOptions
{
    /// <summary>
    /// 域名,公有云、私有云、钉钉等等
    /// </summary>
    public string Domain { get; set; } = "https://api.qingflow.com";

    /// <summary>
    /// 工作区id：可直接在轻流系统中获取
    /// </summary>
    public string WorkspaceId { get; set; }

    /// <summary>
    /// accesstoken/Secret：可直接在轻流系统中获取
    /// </summary>
    public string WorkspaceSecret { get; set; }
    /// <summary>
    /// 超级管理员token：可直接在轻流系统中获取
    /// </summary>
    public string SuperAdminToken { get; set; }
    /// <summary>
    /// 轻流应用配置
    /// </summary>
    public List<QingflowAppOptions> QingflowApps { get; set; } = [];
}

/// <summary>
/// 轻流应用配置
/// </summary>
public class QingflowAppOptions
{
    /// <summary>
    /// 应用id：可直接在轻流系统中获取
    /// </summary>
    public string AppId { get; set; }
    /// <summary>
    /// 应用名称：可直接在轻流系统中获取
    /// </summary>
    public string AppName { get; set; }
}
