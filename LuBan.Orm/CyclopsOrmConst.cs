/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： SqlSugarConst
*版本号： V1.0.0.0
*唯一标识：461c0e6e-0c17-4c07-ad75-2840cb997e35
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 18:43:13
*描述：SqlSugar相关常量
*
*=================================================
*修改标记
*修改时间：2023/12/1 18:43:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SqlSugar相关常量
*
*****************************************************************************/
namespace LuBan.Orm;

/// <summary>
/// SqlSugar相关常量
/// </summary>
public class LuBanOrmConst
{
    /// <summary>
    /// 默认表主键值
    /// </summary>
    public const long DefaultId = 1300000000001;
    /// <summary>
    /// 默认超管账号Id
    /// </summary>
    public const long SuperAdminId = 1300000000101;
    /// <summary>
    /// 默认超管账号角色Id
    /// </summary>
    public const long SuperAdminRoleId = 1300000000101;
    /// <summary>
    /// 默认种子数据Id
    /// </summary>
    public const long DefaultSeedId = 1300000000101;

    /// <summary>
    /// 默认租户
    /// </summary>
    public const long DefaultTenantId = 1300000000001;

    /// <summary>
    /// 默认数据库标识（默认租户）
    /// </summary>
    public const string MainConfigId = "1300000000001";

    /// <summary>
    /// 日志数据库标识
    /// </summary>
    public const string LogConfigId = "1300000000002";

    /// <summary>
    /// 默认表主键
    /// </summary>
    public const string PrimaryKey = "Id";



}


/// <summary>
/// 数据库类型常量定义类
/// 用于定义系统支持的各种数据库类型的字符串常量
/// </summary>
public class DatabaseType
{
    /// <summary>
    /// Microsoft SQL Server 数据库
    /// </summary>
    public const string SqlServer = "SqlServer";

    /// <summary>
    /// MySQL 数据库
    /// </summary>
    public const string MySql = "MySql";

    /// <summary>
    /// Oracle 数据库
    /// </summary>
    public const string Oracle = "Oracle";

    /// <summary>
    /// PostgreSQL 数据库
    /// </summary>
    public const string PostgreSQL = "PostgreSQL";

    /// <summary>
    /// SQLite 数据库
    /// </summary>
    public const string Sqlite = "Sqlite";

    /// <summary>
    /// 达梦数据库
    /// </summary>
    public const string Dm = "Dm";

    /// <summary>
    /// 人大金仓数据库
    /// </summary>
    public const string Kdbndp = "Kdbndp";

    /// <summary>
    /// 神通数据库
    /// </summary>
    public const string Oscar = "Oscar";

    /// <summary>
    /// MySQL Connector 数据库连接器
    /// </summary>
    public const string MySqlConnector = "MySqlConnector";

    /// <summary>
    /// Microsoft Access 数据库
    /// </summary>
    public const string Access = "Access";

    /// <summary>
    /// OpenGauss 数据库
    /// </summary>
    public const string OpenGauss = "OpenGauss";

    /// <summary>
    /// QuestDB 时序数据库
    /// </summary>
    public const string QuestDB = "QuestDB";

    /// <summary>
    /// 瀚高数据库
    /// </summary>
    public const string HG = "HG";

    /// <summary>
    /// ClickHouse 列式数据库
    /// </summary>
    public const string ClickHouse = "ClickHouse";

    /// <summary>
    /// 南大通用数据库
    /// </summary>
    public const string GBase = "GBase";

    /// <summary>
    /// ODBC 数据源
    /// </summary>
    public const string Odbc = "Odbc";

    /// <summary>
    /// 自定义数据库类型
    /// </summary>
    public const string Custom = "Custom";
}
