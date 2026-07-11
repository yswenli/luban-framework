/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Models
*文件名： OrmProvider
*版本号： V1.0.0.0
*唯一标识：c32e593b-06d7-47ec-ad07-500b7009a60c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/4 18:05:21
*描述：OrmProvider
*
*=================================================
*修改标记
*修改时间：2024/12/4 18:05:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OrmProvider
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// OrmProvider
/// </summary>
public class OrmProvider
{
    /// <summary>
    /// Tenant
    /// </summary>
    public ITenant Tenant { get; set; }
    /// <summary>
    /// Provider
    /// </summary>
    public SqlSugarScopeProvider Provider { get; set; }
}
