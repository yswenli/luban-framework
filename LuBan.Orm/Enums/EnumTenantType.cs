/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumTenantType
*版本号： V1.0.0.0
*唯一标识：27472d26-1daa-43a4-aa85-b2ea480a0683
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:17:01
*描述：租户类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:17:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：租户类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 租户类型枚举
/// </summary>
[Description("租户类型枚举")]
public enum EnumTenantType
{
    /// <summary>
    /// Id隔离
    /// </summary>
    [Description("Id隔离")]
    Id = 0,

    /// <summary>
    /// 库隔离
    /// </summary>
    [Description("库隔离")]
    Db = 1,
}
