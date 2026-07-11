using Models.Enums;

namespace Models.Dto;


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