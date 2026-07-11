/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Models
*文件名： VisualDb
*版本号： V1.0.0.0
*唯一标识：3c0b2ec8-1a62-4a1c-80b4-819bf3e37bd5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 15:07:34
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/23 15:07:34
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Orm.Models;


/// <summary>
/// 可视化数据库配置类
/// </summary>
public class VisualDb
{
    /// <summary>
    /// 配置ID，用于唯一标识数据库连接配置
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 数据库昵称，用于显示的友好名称
    /// </summary>
    public string DbNickName { get; set; }
}

/// <summary>
/// 库表可视化
/// </summary>
public class VisualDbTable
{
    /// <summary>
    /// 可视化表列表，包含数据库中所有需要可视化的表信息
    /// </summary>
    public List<VisualTable> VisualTableList { get; set; }

    /// <summary>
    /// 可视化列列表，包含所有表的列信息
    /// </summary>
    public List<VisualColumn> VisualColumnList { get; set; }

    /// <summary>
    /// 列关系列表，包含表之间的外键关系
    /// </summary>
    public List<ColumnRelation> ColumnRelationList { get; set; }
}

/// <summary>
/// 可视化表信息类
/// </summary>
public class VisualTable
{
    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 表备注信息
    /// </summary>
    public string TableComents { get; set; }

    /// <summary>
    /// 可视化界面中的X坐标
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// 可视化界面中的Y坐标
    /// </summary>
    public int Y { get; set; }
}

/// <summary>
/// 可视化列信息类
/// </summary>
public class VisualColumn
{
    /// <summary>
    /// 所属表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名称
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 数据长度
    /// </summary>
    public string DataLength { get; set; }

    /// <summary>
    /// 列描述信息
    /// </summary>
    public string ColumnDescription { get; set; }
}

/// <summary>
/// 列关系信息类，用于描述表之间的外键关系
/// </summary>
public class ColumnRelation
{
    /// <summary>
    /// 源表名称
    /// </summary>
    public string SourceTableName { get; set; }

    /// <summary>
    /// 源表列名称
    /// </summary>
    public string SourceColumnName { get; set; }

    /// <summary>
    /// 关系类型
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 目标表名称
    /// </summary>
    public string TargetTableName { get; set; }

    /// <summary>
    /// 目标表列名称
    /// </summary>
    public string TargetColumnName { get; set; }
}



/// <summary>
/// 数据库列输出信息类
/// </summary>
public class DbColumnOutput
{
    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 表ID
    /// </summary>
    public int TableId { get; set; }

    /// <summary>
    /// 数据库列名
    /// </summary>
    public string DbColumnName { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 属性类型
    /// </summary>
    public object PropertyType { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 是否可为空
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// 是否为自增
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// 是否为主键
    /// </summary>
    public bool IsPrimarykey { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// 小数位数
    /// </summary>
    public int DecimalDigits { get; set; }

    /// <summary>
    /// 精度
    /// </summary>
    public int Scale { get; set; }

    /// <summary>
    /// 是否为数组
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// 是否为Json
    /// </summary>
    public bool IsJson { get; set; }

    /// <summary>
    /// 是否为无符号
    /// </summary>
    public bool? IsUnsigned { get; set; }

    /// <summary>
    /// 创建表时字段排序
    /// </summary>
    public int CreateTableFieldSort { get; set; }

    /// <summary>
    /// SQL参数数据库类型
    /// </summary>
    internal object SqlParameterDbType { get; set; }
}



/// <summary>
/// 数据库列输入信息类
/// </summary>
public class DbColumnInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 数据库列名
    /// </summary>
    public string DbColumnName { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 是否可为空(0=非空,1=可空)
    /// </summary>
    public int IsNullable { get; set; }

    /// <summary>
    /// 是否为自增(0=非自增,1=自增)
    /// </summary>
    public int IsIdentity { get; set; }

    /// <summary>
    /// 是否为主键(0=非主键,1=主键)
    /// </summary>
    public int IsPrimarykey { get; set; }

    /// <summary>
    /// 小数位数
    /// </summary>
    public int DecimalDigits { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }
}

/// <summary>
/// 更新数据库列输入信息类
/// </summary>
public class UpdateDbColumnInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 新列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 旧列名
    /// </summary>
    public string OldColumnName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }
}

/// <summary>
/// 移动数据库列输入信息类
/// </summary>
public class MoveDbColumnInput
{
    /// <summary>
    /// 数据库配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 目标表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    ///要移动的列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 移动到该列后方（为空时移动到首列）
    /// </summary>
    public string AfterColumnName { get; set; }
}

/// <summary>
/// 删除数据库列输入信息类
/// </summary>
public class DeleteDbColumnInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 数据库列名
    /// </summary>
    public string DbColumnName { get; set; }
}



/// <summary>
/// 数据库表输入信息类
/// </summary>
public class DbTableInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 表描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 列信息列表
    /// </summary>
    public List<DbColumnInput> DbColumnInfoList { get; set; }
}

/// <summary>
/// 更新数据库表输入信息类
/// </summary>
public class UpdateDbTableInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 新表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 旧表名
    /// </summary>
    public string OldTableName { get; set; }

    /// <summary>
    /// 表描述
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// 删除数据库表输入信息类
/// </summary>
public class DeleteDbTableInput
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string TableName { get; set; }
}


/// <summary>
/// 创建实体输入信息类
/// </summary>
public class CreateEntityInput
{
    /// <summary>
    /// 表名
    /// </summary>
    /// <example>student</example>
    public string TableName { get; set; }

    /// <summary>
    /// 实体名
    /// </summary>
    /// <example>Student</example>
    public string EntityName { get; set; }

    /// <summary>
    /// 基类名
    /// </summary>
    /// <example>AutoIncrementEntity</example>
    public string BaseClassName { get; set; }

    /// <summary>
    /// 导出位置
    /// </summary>
    /// <example>Web.Application</example>
    public string Position { get; set; }

    /// <summary>
    /// 库标识
    /// </summary>
    public string ConfigId { get; set; }
}

/// <summary>
/// 创建种子数据输入信息类
/// </summary>
public class CreateSeedDataInput
{
    /// <summary>
    /// 库标识
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    /// <example>student</example>
    public string TableName { get; set; }

    /// <summary>
    /// 实体名称
    /// </summary>
    /// <example>Student</example>
    public string EntityName { get; set; }

    /// <summary>
    /// 种子名称
    /// </summary>
    /// <example>Student</example>
    public string SeedDataName { get; set; }

    /// <summary>
    /// 导出位置
    /// </summary>
    /// <example>Web.Application</example>
    public string Position { get; set; }

    /// <summary>
    /// 后缀
    /// </summary>
    /// <example>Web.Application</example>
    public string Suffix { get; set; }

    /// <summary>
    /// 过滤已有数据
    /// </summary>
    /// <remarks>
    /// 如果数据在其它不同名的已有的种子类型的数据中出现过，就不生成这个数据
    /// 主要用于生成菜单功能，菜单功能往往与子项目绑定，如果生成完整数据就会导致菜单项多处理重复。
    /// </remarks>
    public bool FilterExistingData { get; set; }
}