/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： BlockInput.cs
*版本号： V1.0.0.0
*唯一标识：7af72896-834c-4b94-b8bc-ddb66e15b27e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：BlockInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：BlockInput 类
*
*****************************************************************************/

using WebApplication1.Models.Enums;

namespace WebApplication1.Models.Vos;


/// <summary>
/// 栏目搜索分页
/// </summary>
public class BlockInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Key { get; set; }
    /// <summary>
    /// 级别
    /// </summary>
    public int? Level { get; set; }
    /// <summary>
    /// ID
    /// </summary>
    public long? Id { get; set; }
    /// <summary>
    /// 父级ID
    /// </summary>
    public long? Pid { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public EnumBlockStatus? Status { get; set; }
}


/// <summary>
/// 栏目搜索分页
/// </summary>
public class BlockPagedInput : BasePageInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Key { get; set; }
    /// <summary>
    /// 级别
    /// </summary>
    public int? Level { get; set; }
    /// <summary>
    /// ID
    /// </summary>
    public long? Id { get; set; }
    /// <summary>
    /// 父级ID
    /// </summary>
    public long? Pid { get; set; }
    /// <summary>
    /// 栏目类型
    /// </summary>
    public string BlockType { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public EnumBlockStatus? Status { get; set; }
}

public class BlockInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public long? Id { get; set; }
    /// <summary>
    /// 上级ID
    /// </summary>
    public long? Pid { get; set; }

    /// <summary>
    /// 栏目名称
    /// </summary>
    public string? BlockName { get; set; }

    /// <summary>
    /// 栏目封面
    /// </summary>
    public string? BlockImg { get; set; }

    /// <summary>
    /// 级别
    /// </summary>
    public int? Level { get; set; }
    /// <summary>
    /// 栏目类型
    /// </summary>
    public string BlockType { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int? Sort { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    public EnumBlockStatus? Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}