/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumStatus
*版本号： V1.0.0.0
*唯一标识：37883ad6-311e-4f84-a855-45ec5cc89f95
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 13:42:56
*描述：通用状态枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 13:42:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：通用状态枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 通用状态枚举
/// </summary>
[Description("通用状态枚举")]
public enum EnumEnableStatus
{
    /// <summary>
    /// 启用
    /// </summary>
    [Description("启用")]
    Enable = 1,

    /// <summary>
    /// 停用
    /// </summary>
    [Description("停用")]
    Disable = 2,
}
