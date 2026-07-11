/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumMenuType
*版本号： V1.0.0.0
*唯一标识：85a1f3f0-4c6f-4d39-a6ae-239ec76d02d4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:14:14
*描述：系统菜单类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:14:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统菜单类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 系统菜单类型枚举
/// </summary>
[Description("系统菜单类型枚举")]
public enum EnumMenuType
{
    /// <summary>
    /// 目录
    /// </summary>
    [Description("目录")]
    Dir = 1,

    /// <summary>
    /// 菜单
    /// </summary>
    [Description("菜单")]
    Menu = 2,

    /// <summary>
    /// 按钮
    /// </summary>
    [Description("按钮")]
    Btn = 3
}
