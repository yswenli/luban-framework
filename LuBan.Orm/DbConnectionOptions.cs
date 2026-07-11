/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： DbConnectionConfig
*版本号： V1.0.0.0
*唯一标识：a16b3eba-1340-403e-aa6a-691af96a949b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 18:40:52
*描述：数据库连接配置
*
*=================================================
*修改标记
*修改时间：2023/12/1 18:40:52
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据库连接配置
*
*****************************************************************************/


namespace LuBan.Orm;


/// <summary>
/// 数据库配置选项
/// </summary>
public sealed class DbConnectionOptions
{
    /// <summary>
    /// 启用控制台打印SQL
    /// </summary>
    public bool EnableConsoleSql { get; set; } = false;

    /// <summary>
    /// 是否启用数据库日志
    /// </summary>
    public bool EnableDbLogs { get; set; } = true;

    /// <summary>
    /// 数据库日志配置
    /// </summary>
    public DbLogOptions DbLogOptions { get; set; } = new DbLogOptions();

    /// <summary>
    /// 数据库集合
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; }

    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(DbConnectionOptions options, IConfiguration configuration)
    {
        foreach (DbConnectionConfig dbConfig in options.ConnectionConfigs)
        {
            if (dbConfig.ConfigId == null || dbConfig.ConfigId.ToString().IsNullOrEmpty())
                dbConfig.ConfigId = LuBanOrmConst.MainConfigId;
        }
    }
}

/// <summary>
/// 数据库连接配置
/// </summary>
public sealed class DbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DbNickName { get; set; } = string.Empty;
    /// <summary>
    /// 数据库设置
    /// </summary>
    public DbSettings DbSettings { get; set; }

    /// <summary>
    /// 表设置
    /// </summary>
    public TableSettings TableSettings { get; set; }

    /// <summary>
    /// 种子设置
    /// </summary>
    public SeedSettings SeedSettings { get; set; }

    /// <summary>
    /// 隔离方式
    /// </summary>
    public EnumTenantType TenantType { get; set; } = EnumTenantType.Id;

    /// <summary>
    /// 数据库存储目录（仅SqlServer支持指定目录创建）
    /// </summary>
    public string DatabaseDirectory { get; set; }
}

/// <summary>
/// 数据库设置
/// </summary>
public sealed class DbSettings
{
    /// <summary>
    /// 启用库表初始化
    /// </summary>
    public bool EnableInitDb { get; set; } = false;

    /// <summary>
    /// 启用视图初始化
    /// </summary>
    public bool EnableInitView { get; set; } = false;

    /// <summary>
    /// 启用库表差异日志
    /// </summary>
    public bool EnableDiffLog { get; set; } = false;

    /// <summary>
    /// 启用驼峰转下划线
    /// </summary>
    public bool EnableUnderLine { get; set; } = true;
}

/// <summary>
/// 表设置
/// </summary>
public sealed class TableSettings
{
    /// <summary>
    /// 启用表初始化
    /// </summary>
    public bool EnableInitTable { get; set; } = false;

    /// <summary>
    /// 启用表增量更新
    /// </summary>
    public bool EnableIncreTable { get; set; } = true;
}

/// <summary>
/// 种子设置
/// </summary>
public sealed class SeedSettings
{
    /// <summary>
    /// 启用种子初始化
    /// </summary>
    public bool EnableInitSeed { get; set; } = false;

    /// <summary>
    /// 启用种子增量更新
    /// </summary>
    public bool EnableIncreSeed { get; set; } = true;
}
