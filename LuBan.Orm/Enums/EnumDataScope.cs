/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumDataScope
*版本号： V1.0.0.0
*唯一标识：3492b8fd-7c2f-4f76-9a95-c1ff3af18742
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:07:57
*描述：角色数据范围枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:07:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：角色数据范围枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 角色数据范围枚举
/// </summary>
[Description("角色数据范围枚举")]
public enum EnumDataScope
{

    /// <summary>
    /// 未定义
    /// </summary>
    [Description("未定义")]
    Undefined = 0,

    /// <summary>
    /// 全部数据
    /// </summary>
    [Description("全部数据")]
    All = 1,

    /// <summary>
    /// 本部门及以下数据
    /// </summary>
    [Description("本部门及以下数据")]
    DeptChild = 2,

    /// <summary>
    /// 本部门数据
    /// </summary>
    [Description("本部门数据")]
    Dept = 3,

    /// <summary>
    /// 仅本人数据
    /// </summary>
    [Description("仅本人数据")]
    Self = 4,

    /// <summary>
    /// 自定义数据
    /// </summary>
    [Description("自定义数据")]
    Define = 5
}
