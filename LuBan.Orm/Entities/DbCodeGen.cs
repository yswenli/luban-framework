/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： DbCodeGen
*版本号： V1.0.0.0
*唯一标识：3967020c-5e9e-48b3-a1da-810d19e7db54
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 14:09:38
*描述：代码生成表
*
*=================================================
*修改标记
*修改时间：2025/10/23 14:09:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代码生成表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 代码生成表
/// </summary>
[SugarTable("db_code_gen", "代码生成表")]
[SysTable]
[SugarIndex("index_{table}_B", nameof(BusName), OrderByType.Asc)]
[SugarIndex("index_{table}_T", nameof(TableName), OrderByType.Asc)]
public class DbCodeGen : EntityBase
{
    /// <summary>
    /// 作者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "作者姓名", Length = 32)]
    [MaxLength(32)]
    public string? AuthorName { get; set; }

    /// <summary>
    /// 是否移除表前缀
    /// </summary>
    [SugarColumn(ColumnDescription = "是否移除表前缀", Length = 8)]
    [MaxLength(8)]
    public string? TablePrefix { get; set; }

    /// <summary>
    /// 生成方式
    /// </summary>
    [SugarColumn(ColumnDescription = "生成方式", Length = 32)]
    [MaxLength(32)]
    public string? GenerateType { get; set; }

    /// <summary>
    /// 库定位器名
    /// </summary>
    [SugarColumn(ColumnDescription = "库定位器名", Length = 64)]
    [MaxLength(64)]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 库名
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string DbNickName
    {
        get
        {
            try
            {
                var dbOptions = NacosConfigUtil.Read<DbConnectionOptions>("DbConnection");
                if (dbOptions == null)
                {
                    return string.Empty;
                }
                var config = dbOptions.ConnectionConfigs.FirstOrDefault(m => m.ConfigId.ToString() == ConfigId);
                return config?.DbNickName ?? "";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 数据库名(保留字段)
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库库名", Length = 64)]
    [MaxLength(64)]
    public string? DbName { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型", Length = 64)]
    [MaxLength(64)]
    public string? DbType { get; set; }

    /// <summary>
    /// 数据库链接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库链接", Length = 256)]
    [MaxLength(256)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 数据库表名
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库表名", Length = 128)]
    [MaxLength(128)]
    public string? TableName { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    [SugarColumn(ColumnDescription = "命名空间", Length = 128)]
    [MaxLength(128)]
    public string? NameSpace { get; set; }

    /// <summary>
    /// 业务名
    /// </summary>
    [SugarColumn(ColumnDescription = "业务名", Length = 128)]
    [MaxLength(128)]
    public string? BusName { get; set; }

    /// <summary>
    /// 表唯一字段配置
    /// </summary>
    [SugarColumn(ColumnDescription = "表唯一字段配置", Length = 512)]
    [MaxLength(128)]
    public string? TableUniqueConfig { get; set; }

    /// <summary>
    /// 是否生成菜单
    /// </summary>
    [SugarColumn(ColumnDescription = "是否生成菜单")]
    public bool GenerateMenu { get; set; } = true;

    /// <summary>
    /// 菜单图标
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单图标", Length = 32)]
    public string? MenuIcon { get; set; } = "ele-Menu";

    /// <summary>
    /// 菜单编码
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单编码")]
    public long? MenuPid { get; set; }

    /// <summary>
    /// 页面目录
    /// </summary>
    [SugarColumn(ColumnDescription = "页面目录", Length = 32)]
    public string? PagePath { get; set; }

    /// <summary>
    /// 支持打印类型
    /// </summary>
    [SugarColumn(ColumnDescription = "支持打印类型", Length = 32)]
    [MaxLength(32)]
    public string? PrintType { get; set; }

    /// <summary>
    /// 打印模版名称
    /// </summary>
    [SugarColumn(ColumnDescription = "打印模版名称", Length = 32)]
    [MaxLength(32)]
    public string? PrintName { get; set; }

    /// <summary>
    /// 表唯一字段列表
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<TableUniqueConfigItem> TableUniqueList => (TableUniqueConfig.IsNullOrWhiteSpace() ? null : SerializeUtil.Deserialize<List<TableUniqueConfigItem>>(TableUniqueConfig)) ?? new List<TableUniqueConfigItem>();
}


/// <summary>
/// 表唯一配置项
/// </summary>
public class TableUniqueConfigItem
{
    /// <summary>
    /// 字段列表
    /// </summary>
    public List<string> Columns { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 格式化查询条件
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="format">模板字符串</param>
    /// <returns></returns>
    public string Format(string separator, string format) => string.Join(separator, Columns.Select(name => string.Format(format, name)));
}