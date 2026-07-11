/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： QAppInfo
*版本号： V1.0.0.0
*唯一标识：edc99560-0556-459a-a1e0-59a54137fbaf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/24 15:35:20
*描述：应用信息
*
*=================================================
*修改标记
*修改时间：2024/12/24 15:35:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：应用信息
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;
/// <summary>
/// 应用信息
/// </summary>
public class QAppInfo
{
    /// <summary>
    /// 应用key
    /// </summary>
    public string AppKey { get; set; }
    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; }
}
/// <summary>
/// 应用信息列表
/// </summary>
public class AppInfoList
{
    /// <summary>
    /// 应用信息列表
    /// </summary>
    public List<QAppInfo> AppList { get; set; }
}
