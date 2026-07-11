/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Interfaces
*文件名： ITenantIdFilter
*版本号： V1.0.0.0
*唯一标识：be01d891-2361-49f2-a6f6-d419c8f204f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:20:25
*描述：租户Id接口过滤器
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:20:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：租户Id接口过滤器
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces;

/// <summary>
/// 租户Id接口过滤器
/// </summary>
public interface ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    long? TenantId { get; set; }
}
