/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Generating
*文件名： CodeGenerator
*版本号： V1.0.0.0
*唯一标识：a0aa9edf-53a8-46ee-92ca-d20d1e0ae43d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/15 15:24:25
*描述：代码生成器
*
*=================================================
*修改标记
*修改时间：2025/1/15 15:24:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代码生成器
*
*****************************************************************************/
namespace LuBan.Orm.Generating;

/// <summary>
/// 代码生成器
/// </summary>
public static class CodeGenerator
{

    /// <summary>
    /// 从数据库生成CSharp代码
    /// </summary>
    /// <returns></returns>
    public static string GetEntityCSCodeFilePathBySqlSugar()
    {
        var classPath = PathUtil.GetRootFilePath("DBFirstClasses");
        new BaseRepository<DbUser>().AsSugarClient().DbFirst
            .IsCreateAttribute()
            .StringNullable()
            .FormatClassName((x) => x.ConvertToPropertyName())
            .FormatFileName((x) => x.ConvertToPropertyName())
            .FormatPropertyName((x) => x.ConvertToPropertyName())
            .SettingPropertyDescriptionTemplate((q) => q)
            .CreateClassFile(classPath, "LuBan.Models.Entities");
        return classPath;
    }

    /// <summary>
    /// 获取实体代码目录
    /// </summary>
    /// <returns></returns>
    public static string GetEntityCSCodeFilePath()
    {
        var tables = LuBanOrm.GetTableNames();
        if (tables == null || tables.Count < 1) return string.Empty;
        var classPath = PathUtil.GetRootFilePath("DBFirstClasses");
        foreach (var table in tables)
        {
            var code = GetEntityCSCode(table);
            if (code.IsNullOrEmpty()) continue;
            var filePath = Path.Combine(classPath, $"{table.Name.ConvertToPropertyName()}.cs");
            FileUtil.WriteString(filePath, code);
        }
        return classPath;
    }

    /// <summary>
    /// 获取实体代码
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static string GetEntityCSCode(string tableName)
    {
        var table = LuBanOrm.GetTableNames()?.Where(x => x.Name == tableName).FirstOrDefault() ?? null;
        return GetEntityCSCode(table);
    }


    /// <summary>
    /// 获取实体代码
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    static string GetEntityCSCode(DbTableInfo? table)
    {
        if (table == null) return string.Empty;
        if (table.DbObjectType != DbObjectType.Table && table.DbObjectType != DbObjectType.View) return string.Empty;
        var tempTxt = GetEntityTemplateText(table);
        var codeTxt = GetEntityColumnCSCode(table.Name);
        if (codeTxt.IsNullOrEmpty()) return string.Empty;
        return tempTxt.Replace(CSTemplateConst.DataCode, codeTxt);
    }


    /// <summary>
    /// 获取模板数据
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <returns></returns>
    static string GetEntityTemplateText(DbTableInfo tableInfo)
    {
        var cacheTempTxt = MemoryCache.Instance.GetOrSet<string>($"{CacheConst.KeySystem}EntityTemplate:{tableInfo.Name}", (k) =>
        {
            var tempTxt = OrmResource.EntityTemplate;
            var entityTypeName = tableInfo.Name.ConvertToPropertyName();
            tempTxt = tempTxt.Replace(CSTemplateConst.EntityTypeName, entityTypeName);
            var className = entityTypeName;
            tempTxt = tempTxt.Replace(CSTemplateConst.ClassName, className);
            var description = tableInfo.Description ?? $"{tableInfo.Name}实体类";
            tempTxt = tempTxt.Replace(CSTemplateConst.Description, description);
            return tempTxt;
        }) ?? "";
        var dateTime = DateTime.Now;
        return cacheTempTxt.Replace(CSTemplateConst.DateTime, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }


    /// <summary>
    /// 获取实体代码
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    static string GetEntityColumnCSCode(string tableName)
    {
        var columns = LuBanOrm.GetColumns(tableName);
        if (columns == null || columns.Count < 1) return string.Empty;
        var sp = new StringPlus();
        foreach (var column in columns)
        {
            if (LuBanOrm.GetEntityBaseFields()?.Any(q => q == column.DbColumnName.ConvertToPropertyName()) ?? false) continue;
            sp.AppendLine();
            sp.AppendLine("    /// <summary>");
            sp.AppendLine($"    /// {column.DbColumnName.ConvertToPropertyName()}{column.ColumnDescription}");
            sp.AppendLine("    /// </summary>");

            var des = column.ColumnDescription ?? column.DbColumnName.ConvertToPropertyName();
            var columnType = column.DataType.ToLower();

            if (column.Length > 1 || columnType.EndsWith("text") || columnType.EndsWith("varchar(max)"))
            {
                if (column.Length > 4000 || columnType.EndsWith("text") || columnType.EndsWith("varchar(max)"))
                {
                    sp.AppendLine($"    [SugarColumn(ColumnDescription = \"{des}\", ColumnDataType = StaticConfig.CodeFirst_BigString)]");
                }
                else
                {
                    sp.AppendLine($"    [SugarColumn(ColumnDescription = \"{des}\", Length = {column.Length})]");
                }
                if (column.IsNullable)
                {
                    sp.AppendLine($"    public string? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                }
                else
                {
                    sp.AppendLine($"    public string {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                }
            }
            else
            {
                sp.AppendLine($"    [SugarColumn(ColumnDescription = \"{des}\")]");
                switch (columnType)
                {
                    case "tinyint":
                    case "bit":
                        if (column.IsNullable)
                            sp.AppendLine($"    public bool? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public bool {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "int":
                    case "integer":
                        if (column.IsNullable)
                            sp.AppendLine($"    public int? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public int {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "bigint":
                        if (column.IsNullable)
                            sp.AppendLine($"    public long? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public long {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "float":
                        if (column.IsNullable)
                            sp.AppendLine($"    public float? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public float {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "double":
                        if (column.IsNullable)
                            sp.AppendLine($"    public double? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public double {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "decimal":
                    case "numeric":
                        if (column.IsNullable)
                            sp.AppendLine($"    public decimal? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public decimal {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "date":
                    case "datetime":
                        if (column.IsNullable)
                            sp.AppendLine($"    public DateTime? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public DateTime {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "blob":
                    case "longblob":
                    case "mediumblob":
                    case "tinyblob":
                    case "binary":
                    case "image":
                        if (column.IsNullable)
                            sp.AppendLine($"    public byte[]? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public byte[] {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    case "uniqueidentifier":
                        if (column.IsNullable)
                            sp.AppendLine($"    public Guid? {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        else
                            sp.AppendLine($"    public Guid {column.DbColumnName.ConvertToPropertyName()} {{ get; set; }}");
                        break;
                    default:
                        break;

                }
            }
        }
        return sp.ToString();
    }


    /// <summary>
    /// 获取种子数据代码
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="where"></param>
    /// <returns></returns>
    public static string GetSeedDataCSCode<TEntity>(Expression<Func<TEntity, bool>> where)
        where TEntity : EntityBase, IDeletedFilter, new()
    {
        var resp = new BaseRepository<TEntity>();
        var data = resp.Where(where).ToList();
        if (data == null || data.Count < 1)
        {
            return string.Empty;
        }
        var entiyType = typeof(TEntity);
        var tempTxt = GetSeedDateTemplateText(entiyType);
        var entityDataCode = GetEntityDataCSCode(entiyType, data);
        return tempTxt.Replace(CSTemplateConst.DataCode, entityDataCode);
    }

    /// <summary>
    /// 获取种子数据代码
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static string GetSeedDataCSCode<TEntity>(ISugarQueryable<TEntity> queryable)
        where TEntity : EntityBase, IDeletedFilter, new()
    {
        var data = queryable.ToList();
        if (data == null || data.Count < 1)
        {
            return string.Empty;
        }
        var entiyType = typeof(TEntity);
        var tempTxt = GetSeedDateTemplateText(entiyType);
        var entityDataCode = GetEntityDataCSCode(entiyType, data);
        return tempTxt.Replace(CSTemplateConst.DataCode, entityDataCode);
    }

    /// <summary>
    /// 获取种子数据代码
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetSeedDataCSCode<TEntity>(IList<TEntity> data)
        where TEntity : EntityBase, IDeletedFilter, new()
    {
        if (data == null || data.Count < 1)
        {
            return string.Empty;
        }
        var entiyType = typeof(TEntity);
        var tempTxt = GetSeedDateTemplateText(entiyType);
        var entityDataCode = GetEntityDataCSCode(entiyType, data);
        return tempTxt.Replace(CSTemplateConst.DataCode, entityDataCode);
    }

    /// <summary>
    /// 获取基础种子数据代码
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetBaseSeedDataCSCode()
    {
        var result = new Dictionary<string, string>
        {
            { "DbConfig", GetSeedDataCSCode<DbConfig>(q => q.IsDelete == false) },
            { "DbDictType", GetSeedDataCSCode<DbDictType>(q => q.IsDelete == false) },
            { "DbDictData", GetSeedDataCSCode<DbDictData>(q => q.IsDelete == false) },
            { "DbRole", GetSeedDataCSCode<DbRole>(q => q.IsDelete == false) },
            { "DbMenu", GetSeedDataCSCode<DbMenu>(q => q.IsDelete == false) },
            { "DbRoleMenu", GetSeedDataCSCode<DbRoleMenu>(q => q.IsDelete == false) },
            { "DbPos", GetSeedDataCSCode<DbPos>(q => q.IsDelete == false) },
            { "DbOrg", GetSeedDataCSCode<DbOrg>(q => q.IsDelete == false) },
            { "DbUser", GetSeedDataCSCode<DbUser>(q => q.IsDelete == false) },
            { "DbUserRole", GetSeedDataCSCode<DbUserRole>(q => q.IsDelete == false) },
            { "DbUserExtOrg", GetSeedDataCSCode<DbUserExtOrg>(q => q.IsDelete == false) },
            { "DbTenant", GetSeedDataCSCode<DbTenant>(q => q.IsDelete == false) },
            { "DbRegion", GetSeedDataCSCode<DbRegion>(q => q.IsDelete == false) },
            { "DbOpenAccess", GetSeedDataCSCode<DbOpenAccess>(q => q.IsDelete == false) }
        };
        return result;
    }

    /// <summary>
    /// 获取模板数据
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    static string GetSeedDateTemplateText(Type entityType)
    {
        var cacheTempTxt = MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}seedData_template:{entityType.Name}_seedData", (k) =>
        {
            var tempTxt = OrmResource.SeedDataTemplate;
            var entityTypeName = entityType.Name;
            tempTxt = tempTxt.Replace(CSTemplateConst.EntityTypeName, entityTypeName);
            var className = $"{entityTypeName}SeedData";
            tempTxt = tempTxt.Replace(CSTemplateConst.ClassName, className);
            var description = $"{entityTypeName}种子数据";
            tempTxt = tempTxt.Replace(CSTemplateConst.Description, description);
            return tempTxt;
        }) ?? "";
        var dateTime = DateTime.Now;
        return cacheTempTxt.Replace(CSTemplateConst.DateTime, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    /// <summary>
    /// 获取实体数据代码
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityType"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    static string GetEntityDataCSCode<TEntity>(Type entityType, IList<TEntity> list)
        where TEntity : EntityBase, IDeletedFilter, new()
    {
        var sp = new StringPlus();
        foreach (var item in list)
        {
            sp.Append($"            new {entityType.Name} {{ ");
            var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties == null || properties.Length < 1) continue;
            foreach (var property in properties)
            {
                if (property.PropertyType.Name == typeof(IEnumerable<>).Name)
                {
                    continue;
                }
                var val = property.GetValue(item);
                if (val == null) continue;
                if (property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.IsNullable())
                    {
                        var genericType = property.PropertyType.GenericTypeArguments.First();
                        if (genericType.Name == nameof(String))
                        {
                            var text = val?.ToString() ?? "";
                            if (text.IsNotNullOrEmpty() && text.IndexOf("\"") > -1)
                            {
                                text = text.Replace("\"", "\\\"");
                            }
                            sp.Append($"{property.Name} = \"{text}\", ");
                        }
                        else if (genericType.Name == nameof(DateTime))
                        {
                            var dateTimeVal = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            if (dateTimeVal == "0.00")
                            {
                                continue;
                            }
                            sp.Append($"{property.Name} = DateTime.Parse(\"{dateTimeVal}\"), ");
                        }
                        else if (genericType.IsEnum)
                        {
                            sp.Append($"{property.Name} = {genericType.FullName}.{val}, ");
                        }
                        else if (genericType.Name == nameof(Boolean))
                        {
                            var boolVal = ((bool)val ? "true" : "false");
                            sp.Append($"{property.Name} = {boolVal}, ");
                        }
                        else
                        {
                            sp.Append($"{property.Name} = {val}, ");
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (property.PropertyType.Name == nameof(String))
                    {
                        var text = val?.ToString() ?? "";
                        if (text.IsNotNullOrEmpty() && text.IndexOf("\"") > -1)
                        {
                            text = text.Replace("\"", "\\\"");
                        }
                        sp.Append($"{property.Name} = \"{text}\", ");
                    }
                    else if (property.PropertyType.Name == nameof(DateTime))
                    {
                        var dateTimeVal = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        if (dateTimeVal == "0.00")
                        {
                            continue;
                        }
                        sp.Append($"{property.Name} = DateTime.Parse(\"{dateTimeVal}\"), ");
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        sp.Append($"{property.Name} = {property.PropertyType.FullName}.{val}, ");
                    }
                    else if (property.PropertyType.Name == nameof(Boolean))
                    {
                        var boolVal = ((bool)val ? "true" : "false");
                        sp.Append($"{property.Name} = {boolVal}, ");
                    }
                    else
                    {
                        sp.Append($"{property.Name} = {val}, ");
                    }
                }
            }
            sp.RemoveLast(2);
            sp.AppendLine(" },");
        }
        sp.RemoveLast(2);
        return sp.ToString();
    }
}
