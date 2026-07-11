/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumDataOpType
*版本号： V1.0.0.0
*唯一标识：0e0ad65c-38b5-433a-b385-488bcaad1946
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:07:07
*描述：数据操作类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:07:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据操作类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 数据操作类型枚举
/// </summary>
[Description("数据操作类型枚举")]
public enum EnumDataOpType
{
    /// <summary>
    /// 其它
    /// </summary>
    [Description("其它")]
    Other = 0,

    /// <summary>
    /// 增加
    /// </summary>
    [Description("增加")]
    Add = 1,

    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    Delete = 2,

    /// <summary>
    /// 编辑
    /// </summary>
    [Description("编辑")]
    Edit = 3,

    /// <summary>
    /// 更新
    /// </summary>
    [Description("更新")]
    Update = 4,

    /// <summary>
    /// 查询
    /// </summary>
    [Description("查询")]
    Query = 5,

    /// <summary>
    /// 详情
    /// </summary>
    [Description("详情")]
    Detail = 6,

    /// <summary>
    /// 树
    /// </summary>
    [Description("树")]
    Tree = 7,

    /// <summary>
    /// 导入
    /// </summary>
    [Description("导入")]
    Import = 8,

    /// <summary>
    /// 导出
    /// </summary>
    [Description("导出")]
    Export = 9,

    /// <summary>
    /// 授权
    /// </summary>
    [Description("授权")]
    Grant = 10,

    /// <summary>
    /// 强退
    /// </summary>
    [Description("强退")]
    Force = 11,

    /// <summary>
    /// 清空
    /// </summary>
    [Description("清空")]
    Clean = 12
}
