/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm
*文件名： DataBase
*版本号： V1.0.0.0
*唯一标识：4040cd45-d1ac-46b8-b646-cfb3de1841cf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/30 18:37:53
*描述：
*
*=================================================
*修改标记
*修改时间：2024/7/30 18:37:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Orm;

/// <summary>
/// 数据库操作类
/// </summary>
public static class SqlUtil
{
    static ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 数据库操作类
    /// </summary>
    static SqlUtil()
    {
        var tenant = ServiceProviderUtil.GetRequiredService<ISqlSugarClient>().AsTenant();
        _sqlSugarClient = tenant.GetConnectionScope(LuBanOrmConst.MainConfigId);
    }

    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<int> ExecuteSqlAsync(string sql, params SugarParameter[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            return await _sqlSugarClient.Ado.ExecuteCommandAsync(sql);
        }
        var sqlsugarParameters = new List<SugarParameter>();
        foreach (var item in parameters)
        {
            sqlsugarParameters.Add(item);
        }
        return await _sqlSugarClient.Ado.ExecuteCommandAsync(sql, sqlsugarParameters);
    }
    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<int> ExecuteSqlAsync(string sql, Dictionary<string, object> parameters)
    {
        return await ExecuteSqlAsync(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static async Task<bool> InsertAsync<T>(T t) where T : EntityBase, new()
    {
        return await _sqlSugarClient.Insertable(t).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 更新非空字段
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static async Task<bool> UpdataAsync<T>(T t) where T : EntityBase, new()
    {
        if (t.Id < 1) throw FriendlyError.Ex("请输入参数id");
        return await _sqlSugarClient.Updateable(t).IgnoreColumns(true).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 逻辑删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteAsync<T>(T t) where T : EntityBase, new()
    {
        t.IsDelete = true;
        return await UpdataAsync(t);
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<T> GetAsync<T>(string sql, params SugarParameter[] parameters) where T : class, new()
    {
        var sqlsugarParameters = new List<SugarParameter>();
        if (parameters != null && parameters.Length > 0)
            foreach (var item in parameters)
            {
                sqlsugarParameters.Add(item);
            }
        return await _sqlSugarClient.Ado.SqlQuerySingleAsync<T>(sql, sqlsugarParameters);
    }
    /// <summary>
    /// 获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<T> GetAsync<T>(string sql, Dictionary<string, object> parameters) where T : class, new()
    {
        return await GetAsync<T>(sql, parameters.ToSugarParameters());
    }
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<List<T>> GetListAsync<T>(string sql, params SugarParameter[] parameters) where T : class, new()
    {
        var sqlsugarParameters = new List<SugarParameter>();
        if (parameters != null && parameters.Length > 0)
            foreach (var item in parameters)
            {
                sqlsugarParameters.Add(item);
            }
        return await _sqlSugarClient.Ado.SqlQueryAsync<T>(sql, sqlsugarParameters);
    }
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<List<T>> GetListAsync<T>(string sql, Dictionary<string, object> parameters) where T : class, new()
    {
        return await GetListAsync<T>(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(string tableName, string columnName, dynamic value) where T : class, new()
    {
        if (value == null) return default;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName}=@value";
        List<SugarParameter> parameters = [new SugarParameter("@value", value)];
        return await _sqlSugarClient.Ado.SqlQuerySingleAsync<T>(sql, parameters);
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<List<T>?> GetListAsync<T>(string tableName, string columnName, dynamic value) where T : class, new()
    {
        if (value == null) return default;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName}=@value";
        List<SugarParameter> parameters = [new SugarParameter("@value", value)];
        return await _sqlSugarClient.Ado.SqlQueryAsync<T>(sql, parameters);
    }

    /// <summary>
    /// 数据是否存在
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(string tableName, string columnName, dynamic value)
    {
        if (value == null) return default;
        var sql = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName}=@value";
        List<SugarParameter> parameters = [new SugarParameter("@value", value)];
        return await _sqlSugarClient.Ado.GetLongAsync(sql, parameters) > 0;
    }

    /// <summary>
    /// 数据是否存在
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Exists(string tableName, string columnName, dynamic value)
    {
        if (value == null) return default;
        var sql = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName}=@value";
        List<SugarParameter> parameters = [new SugarParameter("@value", value)];
        return _sqlSugarClient.Ado.GetLong(sql, parameters) > 0;
    }

    /// <summary>
    /// 获取SQL IN语句的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="sqlParamas">Dictionary[string, object]参数</param>
    /// <param name="sqlObject">查询对象，例如a.ID</param>
    /// <param name="isAnd">连接符</param>
    /// <returns></returns>
    public static string GetSqlInString<T>(this List<T> data, ref Dictionary<string, object> sqlParamas, string sqlObject, bool isAnd = true)
    {
        if (data == null || data.Count < 1) return string.Empty;
        var sp = new StringPlus("AND(");
        if (!isAnd)
        {
            sp = new StringPlus("OR(");
        }
        var rnd = RandomUtil.GetRndCodeStr(10, 4);
        var pNames = new List<string>();
        for (int i = 0; i < data.Count; i++)
        {
            var pName = $"{rnd}{i}";
            var pValue = data[i];
            if (pValue == null) continue;
            sqlParamas.Add(pName, pValue);
            pNames.Add(pName);
        }
        sp.Append($"{sqlObject} IN ({string.Join(",", pNames)})) ");
        return sp.ToString();
    }

    /// <summary>
    /// 获取SQL IN语句的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="sqlParamas">Dictionary[string, object]参数</param>
    /// <param name="sqlObject">查询对象，例如a.ID</param>
    /// <param name="isAnd">连接符</param>
    /// <returns></returns>
    public static string GetSqlInString<T>(this T[] data, ref Dictionary<string, object> sqlParamas, string sqlObject, bool isAnd = true)
    {
        return GetSqlInString(data.ToList(), ref sqlParamas, sqlObject, isAnd);
    }

    /// <summary>
    /// 获取SQL OR语句的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="sqlParamas">Dictionary[string, object]参数</param>
    /// <param name="sqlObject">查询对象，例如a.ID</param>
    /// <param name="isAnd">连接符</param>
    /// <returns></returns>
    public static string GetSqlOrString<T>(this List<T> data, ref Dictionary<string, object> sqlParamas, string sqlObject, bool isAnd = true)
    {
        if (data == null || data.Count < 1) return string.Empty;
        var sp = new StringPlus("AND(");
        if (!isAnd)
        {
            sp = new StringPlus("OR(");
        }
        var rnd = RandomUtil.GetRndCodeStr(10, 4);
        for (int i = 0; i < data.Count; i++)
        {
            var pName = $"{rnd}{i}";
            var pValue = data[i];
            if (pValue == null) continue;
            sqlParamas.Add(pName, pValue);
            sp.Append($"{sqlObject}=@{pName} ");
            if (i < data.Count - 1)
            {
                sp.Append("OR ");
            }
        }
        sp.Append(") ");
        return sp.ToString();
    }

    /// <summary>
    /// 获取SQL OR语句的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="sqlParamas">Dictionary[string, object]参数</param>
    /// <param name="sqlObject">查询对象，例如a.ID</param>
    /// <param name="isAnd">连接符</param>
    /// <returns></returns>
    public static string GetSqlOrString<T>(this T[] data, ref Dictionary<string, object> sqlParamas, string sqlObject, bool isAnd = true)
    {
        return GetSqlOrString(data.ToList(), ref sqlParamas, sqlObject, isAnd);
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<int> ExecuteStoredProcedureAsync(string procedureName, params SugarParameter[] parameters)
    {
        return await _sqlSugarClient.Ado.UseStoredProcedure().ExecuteCommandAsync(procedureName, parameters);
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params SugarParameter[] parameters)
    {
        return await _sqlSugarClient.Ado.UseStoredProcedure().SqlQueryAsync<T>(procedureName, parameters);
    }


    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, Dictionary<string, object> parameters)
    {
        return await ExecuteStoredProcedureAsync<T>(procedureName, parameters.ToSugarParameters());
    }



    /// <summary>
    /// 根据数据库类型来处理对应的数据字段类型
    /// </summary>
    /// <param name="dbColumnInfo"></param>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public static string ConvertDataType(DbColumnInfo dbColumnInfo, DbType dbType = DbType.Custom)
    {
        if (dbType == DbType.Custom)
            dbType = NacosConfigUtil.Read<DbConnectionOptions>()!.ConnectionConfigs[0].DbType;

        var dataType = dbType switch
        {
            DbType.Oracle => ConvertDataTypeOracleSql(string.IsNullOrEmpty(dbColumnInfo.OracleDataType) ? dbColumnInfo.DataType : dbColumnInfo.OracleDataType, dbColumnInfo.Length, dbColumnInfo.Scale),
            DbType.PostgreSQL => ConvertDataTypePostgresSql(dbColumnInfo.DataType),
            _ => ConvertDataTypeDefault(dbColumnInfo.DataType),
        };
        return dataType + (dbColumnInfo.IsNullable ? "?" : "");
    }

    /// <summary>
    /// 默认数据类型转换
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="length"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static string ConvertDataTypeOracleSql(string dataType, int? length, int? scale)
    {
        switch (dataType.ToLower())
        {
            case "interval year to month": return "int";

            case "interval day to second": return "TimeSpan";

            case "smallint": return "Int16";

            case "int":
            case "integer": return "int";

            case "long": return "long";

            case "float": return "float";

            case "decimal": return "decimal";

            case "number":
                if (length == null) return "decimal";
                return scale switch
                {
                    > 0 => "decimal",
                    0 or null when length is > 1 and < 12 => "int",
                    0 or null when length > 11 => "long",
                    _ => length == 1 ? "bool" : "decimal"
                };

            case "char":
            case "clob":
            case "nclob":
            case "nchar":
            case "nvarchar":
            case "varchar":
            case "nvarchar2":
            case "varchar2":
            case "rowid":
                return "string";

            case "timestamp":
            case "timestamp with time zone":
            case "timestamptz":
            case "timestamp without time zone":
            case "date":
            case "time":
            case "time with time zone":
            case "timetz":
            case "time without time zone":
                return "DateTime";

            case "bfile":
            case "blob":
            case "raw":
                return "byte[]";

            default:
                return "object";
        }
    }

    /// <summary>
    /// PostgresSQL数据类型对应的字段类型
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string ConvertDataTypePostgresSql(string dataType)
    {
        switch (dataType)
        {
            case "int2":
            case "smallint":
                return "Int16";

            case "int4":
            case "integer":
                return "int";

            case "int8":
            case "bigint":
                return "long";

            case "float4":
            case "real":
                return "float";

            case "float8":
            case "double precision":
                return "double";

            case "numeric":
            case "decimal":
            case "path":
            case "point":
            case "polygon":
            case "interval":
            case "lseg":
            case "macaddr":
            case "money":
                return "decimal";

            case "boolean":
            case "bool":
            case "box":
            case "bytea":
                return "bool";

            case "varchar":
            case "character varying":
            case "geometry":
            case "name":
            case "text":
            case "char":
            case "character":
            case "cidr":
            case "circle":
            case "tsquery":
            case "tsvector":
            case "txid_snapshot":
            case "xml":
            case "json":
                return "string";

            case "uuid":
                return "Guid";

            case "timestamp":
            case "timestamp with time zone":
            case "timestamptz":
            case "timestamp without time zone":
            case "date":
            case "time":
            case "time with time zone":
            case "timetz":
            case "time without time zone":
                return "DateTime";

            case "bit":
            case "bit varying":
                return "byte[]";

            case "varbit":
                return "byte";

            default:
                return "object";
        }
    }

    /// <summary>
    /// Mysql数据类型对应的字段类型
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string ConvertDataTypeDefault(string dataType)
    {
        return dataType.ToLower() switch
        {
            "tinytext" or "mediumtext" or "longtext" or "mid" or "text" or "varchar" or "char" or "nvarchar" or "nchar" or "string" or "timestamp" => "string",
            "int" or "integer" or "int32" => "int",
            "smallint" => "Int16",
            //"tinyint" => "byte",
            "tinyint" => "bool",    // MYSQL
            "bigint" or "int64" => "long",
            "bit" or "boolean" => "bool",
            "money" or "smallmoney" or "numeric" or "decimal" => "decimal",
            "real" => "Single",
            "datetime" or "datetime2" or "smalldatetime" => "DateTime",
            "float" or "double" => "double",
            "image" or "binary" or "varbinary" => "byte[]",
            "uniqueidentifier" => "Guid",
            _ => "object",
        };
    }

    /// <summary>
    /// 数据类型转显示类型
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string DataTypeToEff(string dataType)
    {
        if (string.IsNullOrEmpty(dataType)) return "";
        return dataType.TrimEnd('?') switch
        {
            "string" => "Input",
            "int" => "InputNumber",
            "long" => "Input",
            "float" => "InputNumber",
            "double" => "InputNumber",
            "decimal" => "InputNumber",
            "bool" => "Switch",
            "Guid" => "Input",
            "DateTime" => "DatePicker",
            _ => "Input",
        };
    }

    /// <summary>
    /// 是否通用字段
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static bool IsCommonColumn(string columnName)
    {
        var columnList = new List<string>()
        {
            "OrgId",
            nameof(EntityTenant.TenantId),
            nameof(EntityBase.CreateTime),
            nameof(EntityBase.UpdateTime),
            nameof(EntityBase.CreateUserId),
            nameof(EntityBase.UpdateUserId),
            nameof(EntityBase.CreateUserName),
            nameof(EntityBase.UpdateUserName),
            nameof(EntityBase.IsDelete)
        };
        return columnList.Contains(columnName);
    }

}
