/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Services.ApiServices
*文件名： SysDatabaseService
*版本号： V1.0.0.0
*唯一标识：d41ebae3-9d43-44b4-a0b1-10a88029ff6a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 15:05:00
*描述：库表管理业务
*
*=================================================
*修改标记
*修改时间：2025/10/23 15:05:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：库表管理业务
*
*****************************************************************************/
using LuBan.Orm.Attributes;
using LuBan.Orm.Interfaces;

using System.Collections;
using System.Linq.Dynamic;

//using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace WebApplication1.Services.ApiServices;

/// <summary>
/// 库表管理业务
/// </summary>
public class DatabaseService
{
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 库表管理业务
    /// </summary>
    public DatabaseService()
    {
        _db = ServiceProviderUtil.GetRequiredService<ISqlSugarClient>();
    }

    /// <summary>
    /// 获取库列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取库列表")]
    public List<VisualDb> GetList()
    {
        var configs = NacosConfigUtil.Read<DbConnectionOptions>()?.ConnectionConfigs;
        return configs?.Select(u => new VisualDb { ConfigId = u.ConfigId?.ToString() ?? string.Empty, DbNickName = u.DbNickName ?? string.Empty }).ToList() ?? new List<VisualDb>();
    }

    /// <summary>
    /// 获取可视化库表结构 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取可视化库表结构")]
    public VisualDbTable GetVisualDbTable()
    {
        var visualTableList = new List<VisualTable>();
        var visualColumnList = new List<VisualColumn>();
        var columnRelationList = new List<ColumnRelation>();
        var dbOptions = NacosConfigUtil.Read<DbConnectionOptions>()!.ConnectionConfigs.First(u => u.ConfigId.ToString() == LuBanOrmConst.MainConfigId);

        // 遍历所有实体获取所有库表结构
        var random = new Random();
        var entityTypes = RuntimeUtil.GetTypes()!.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false)).ToList();
        foreach (var entityType in entityTypes)
        {
            var entityInfo = _db.EntityMaintenance.GetEntityInfoNoCache(entityType);

            var visualTable = new VisualTable
            {
                TableName = entityInfo.DbTableName,
                TableComents = entityInfo.TableDescription + entityInfo.DbTableName,
                X = random.Next(5000),
                Y = random.Next(5000)
            };
            visualTableList.Add(visualTable);

            foreach (EntityColumnInfo columnInfo in entityInfo.Columns)
            {
                var visualColumn = new VisualColumn
                {
                    TableName = columnInfo.DbTableName,
                    ColumnName = dbOptions.DbSettings.EnableUnderLine ? columnInfo.DbColumnName.ToUnderLine() : columnInfo.DbColumnName,
                    DataType = columnInfo.PropertyInfo.PropertyType.Name,
                    DataLength = columnInfo.Length.ToString(),
                    ColumnDescription = columnInfo.ColumnDescription,
                };
                visualColumnList.Add(visualColumn);

                // 根据导航配置获取表之间关联关系
                if (columnInfo.Navigat != null)
                {
                    var name1 = columnInfo.Navigat.GetName();
                    var name2 = columnInfo.Navigat.GetName2();
                    var targetColumnName = string.IsNullOrEmpty(name2) ? "Id" : name2;
                    var relation = new ColumnRelation
                    {
                        SourceTableName = columnInfo.DbTableName,
                        SourceColumnName = dbOptions.DbSettings.EnableUnderLine ? name1.ToUnderLine() : name1,
                        Type = columnInfo.Navigat.GetNavigateType() == NavigateType.OneToOne ? "ONE_TO_ONE" : "ONE_TO_MANY",
                        TargetTableName = dbOptions.DbSettings.EnableUnderLine ? columnInfo.DbColumnName.ToUnderLine() : columnInfo.DbColumnName,
                        TargetColumnName = dbOptions.DbSettings.EnableUnderLine ? targetColumnName.ToUnderLine() : targetColumnName
                    };
                    columnRelationList.Add(relation);
                }
            }
        }

        return new VisualDbTable { VisualTableList = visualTableList, VisualColumnList = visualColumnList, ColumnRelationList = columnRelationList };
    }

    /// <summary>
    /// 获取字段列表 🔖
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="configId">ConfigId</param>
    /// <returns></returns>
    [DisplayName("获取字段列表")]
    public List<DbColumnOutput> GetColumnList(string tableName, string configId = LuBanOrmConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return string.IsNullOrWhiteSpace(tableName) ? new List<DbColumnOutput>() : db.DbMaintenance.GetColumnInfosByTableName(tableName, false).Adapt<List<DbColumnOutput>>();
    }

    /// <summary>
    /// 获取数据库数据类型列表 🔖
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    [DisplayName("获取数据库数据类型列表")]
    public List<string> GetDbTypeList(string configId = LuBanOrmConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetDbTypes().OrderBy(u => u).ToList();
    }

    /// <summary>
    /// 增加列 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("增加列")]
    public void AddColumn(DbColumnInput input)
    {
        var column = new DbColumnInfo
        {
            ColumnDescription = input.ColumnDescription,
            DbColumnName = input.DbColumnName,
            IsIdentity = input.IsIdentity == 1,
            IsNullable = input.IsNullable == 1,
            IsPrimarykey = input.IsPrimarykey == 1,
            Length = input.Length,
            DecimalDigits = input.DecimalDigits,
            DataType = input.DataType
        };
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.AddColumn(input.TableName, column);
        // 默认值直接添加报错
        if (!string.IsNullOrWhiteSpace(input.DefaultValue))
        {
            db.DbMaintenance.AddDefaultValue(input.TableName, column.DbColumnName, input.DefaultValue);
        }
        db.DbMaintenance.AddColumnRemark(input.DbColumnName, input.TableName, input.ColumnDescription);
        if (column.IsPrimarykey) db.DbMaintenance.AddPrimaryKey(input.TableName, input.DbColumnName);
    }

    /// <summary>
    /// 删除列 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("删除列")]
    public void DeleteColumn(DeleteDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.DropColumn(input.TableName, input.DbColumnName);
    }

    /// <summary>
    /// 编辑列 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("编辑列")]
    public void UpdateColumn(UpdateDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.RenameColumn(input.TableName, input.OldColumnName, input.ColumnName);
        if (!string.IsNullOrWhiteSpace(input.DefaultValue))
        {
            db.DbMaintenance.AddDefaultValue(input.TableName, input.ColumnName, input.DefaultValue);
        }
        if (db.DbMaintenance.IsAnyColumnRemark(input.ColumnName, input.TableName))
        {
            db.DbMaintenance.DeleteColumnRemark(input.ColumnName, input.TableName);
        }

        db.DbMaintenance.AddColumnRemark(input.ColumnName, input.TableName, string.IsNullOrWhiteSpace(input.Description) ? input.ColumnName : input.Description);
    }

    /// <summary>
    /// 移动列位置 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("移动列")]
    public void MoveColumn(MoveDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var dbMaintenance = db.DbMaintenance;

        var dbType = db.CurrentConnectionConfig.DbType;

        var columns = dbMaintenance.GetColumnInfosByTableName(input.TableName, false);
        var targetColumn = columns.FirstOrDefault(c =>
            c.DbColumnName.Equals(input.ColumnName, StringComparison.OrdinalIgnoreCase));

        if (targetColumn == null)
            throw new Exception($"列 {input.ColumnName} 在表 {input.TableName} 中不存在");

        switch (dbType)
        {
            case SqlSugar.DbType.MySql:
                MoveColumnInMySQL(db, input.TableName, input.ColumnName, input.AfterColumnName);
                break;

            default:
                throw new NotSupportedException($"暂不支持 {dbType} 数据库的列移动操作");
        }
    }

    /// <summary>
    /// 获取列定义
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="noDefault"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GetColumnDefinitionInMySQL(ISqlSugarClient db, string tableName, string columnName, bool noDefault = false)
    {
        var columnDef = db.Ado.SqlQuery<dynamic>(
            $"SHOW FULL COLUMNS FROM `{tableName}` WHERE Field = '{columnName}'"
        ).FirstOrDefault();

        if (columnDef == null)
            throw new Exception($"Column {columnName} not found");

        var definition = new StringBuilder();
        definition.Append($"`{columnName}` ");  // 列名
        definition.Append($"{columnDef.Type} "); // 数据类型

        // 处理约束条件
        definition.Append(columnDef.Null == "YES" ? "NULL " : "NOT NULL ");
        if (columnDef.Default != null && !noDefault)
            definition.Append($"DEFAULT '{columnDef.Default}' ");
        if (!string.IsNullOrEmpty(columnDef.Extra))
            definition.Append($"{columnDef.Extra} ");
        if (!string.IsNullOrEmpty(columnDef.Comment))
            definition.Append($"COMMENT '{columnDef.Comment.Replace("'", "''")}'");

        return definition.ToString();
    }

    /// <summary>
    /// MySQL 列移动实现
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="afterColumnName"></param>
    private void MoveColumnInMySQL(ISqlSugarClient db, string tableName, string columnName, string afterColumnName)
    {
        var definition = GetColumnDefinitionInMySQL(db, tableName, columnName);
        var sql = new StringBuilder();
        sql.Append($"ALTER TABLE `{tableName}` MODIFY COLUMN {definition}");

        if (string.IsNullOrEmpty(afterColumnName))
            sql.Append(" FIRST");
        else
            sql.Append($" AFTER `{afterColumnName}`");

        db.Ado.ExecuteCommand(sql.ToString());
    }

    /// <summary>
    /// 获取表列表 🔖
    /// </summary>
    /// <param name="configId">ConfigId</param>
    /// <returns></returns>
    [DisplayName("获取表列表")]
    public List<DbTableInfo> GetTableList(string configId = LuBanOrmConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetTableInfoList(false);
    }

    /// <summary>
    /// 增加表 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("增加表")]
    public void AddTable(DbTableInput input)
    {
        if (input.DbColumnInfoList == null || !input.DbColumnInfoList.Any())
            throw FriendlyError.Ex(EnumErrorCode.db1000);

        if (input.DbColumnInfoList.GroupBy(u => u.DbColumnName).Any(u => u.Count() > 1))
            throw FriendlyError.Ex(EnumErrorCode.db1002);

        var config = NacosConfigUtil.Read<DbConnectionOptions>()!.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == input.ConfigId);
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var typeBuilder = db.DynamicBuilder().CreateClass(input.TableName, new SugarTable() { TableName = input.TableName, TableDescription = input.Description });
        input.DbColumnInfoList.ForEach(u =>
        {
            var dbColumnName = config!.DbSettings.EnableUnderLine ? u.DbColumnName.Trim().ToUnderLine() : u.DbColumnName.Trim();
            // 虚拟类都默认string类型，具体以列数据类型为准
            typeBuilder.CreateProperty(dbColumnName, typeof(string), new SugarColumn()
            {
                IsPrimaryKey = u.IsPrimarykey == 1,
                IsIdentity = u.IsIdentity == 1,
                ColumnDataType = u.DataType,
                Length = u.Length,
                IsNullable = u.IsNullable == 1,
                DecimalDigits = u.DecimalDigits,
                ColumnDescription = u.ColumnDescription,
                DefaultValue = u.DefaultValue,
            });
        });
        db.CodeFirst.InitTables(typeBuilder.BuilderType());
    }

    /// <summary>
    /// 删除表 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("删除表")]
    public void DeleteTable(DeleteDbTableInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.DropTable(input.TableName);
    }

    /// <summary>
    /// 编辑表 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("编辑表")]
    public void UpdateTable(UpdateDbTableInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.RenameTable(input.OldTableName, input.TableName);
        try
        {
            if (db.DbMaintenance.IsAnyTableRemark(input.TableName))
                db.DbMaintenance.DeleteTableRemark(input.TableName);

            if (!string.IsNullOrWhiteSpace(input.Description))
                db.DbMaintenance.AddTableRemark(input.TableName, input.Description);
        }
        catch (NotSupportedException ex)
        {
            throw FriendlyError.Ex(ex.ToString());
        }
    }

    /// <summary>
    /// 创建实体 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("创建实体")]
    public void CreateEntity(CreateEntityInput input)
    {
        var config = NacosConfigUtil.Read<DbConnectionOptions>()!.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == input.ConfigId);
        input.Position = string.IsNullOrWhiteSpace(input.Position) ? "Admin.NET.Application" : input.Position;
        input.EntityName = string.IsNullOrWhiteSpace(input.EntityName) ? (config!.DbSettings.EnableUnderLine ? input.TableName.ConvertToCamelCase(null) : input.TableName) : input.EntityName;
        string[] dbColumnNames = Array.Empty<string>();
        // Entity.cs.vm中是允许创建没有基类的实体的，所以这里也要做出相同的判断
        if (!string.IsNullOrWhiteSpace(input.BaseClassName))
        {
            Assembly assembly = Assembly.Load("Admin.NET.Core");
            Type? type = assembly.GetType($"Admin.NET.Core.{input.BaseClassName}");
            if (type is null)
                throw FriendlyError.Ex("基类集合配置不存在此类型");
            dbColumnNames = ReflectionUtil.GetProperties(type)?.Select(p => p.Name).ToArray() ?? [];
            if (dbColumnNames is null || dbColumnNames is { Length: 0 })
                throw FriendlyError.Ex("基类中不存在任何字段");
        }
        //var templatePath = GetEntityTemplatePath();
        //var targetPath = GetEntityTargetPath(input);
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        DbTableInfo dbTableInfo = db.DbMaintenance.GetTableInfoList(false).FirstOrDefault(u => u.Name == input.TableName || u.Name == input.TableName.ToLower()) ?? throw FriendlyError.Ex(EnumErrorCode.db1001);
        List<DbColumnInfo> dbColumnInfos = db.DbMaintenance.GetColumnInfosByTableName(input.TableName, false);
        dbColumnInfos.ForEach(u =>
        {
            if (u.DbColumnName.ToUpper() == u.DbColumnName)
            {
                throw new Exception($"错误：{u.DbColumnName} 字段全是大写字母，这样生成的代码会有bug！请更改为大写字母开头的驼峰式命名!");
            }
            u.PropertyName = config!.DbSettings.EnableUnderLine ? u.DbColumnName.ConvertToCamelCase(dbColumnNames) : u.DbColumnName; // 转下划线后的列名需要再转回来
            u.DataType = SqlUtil.ConvertDataType(u, config.DbType);
        });
        //if (_codeGenOptions.BaseEntityNames.Contains(input.BaseClassName, StringComparer.OrdinalIgnoreCase))
        //    dbColumnInfos = dbColumnInfos.Where(u => !dbColumnNames.Contains(u.PropertyName, StringComparer.OrdinalIgnoreCase)).ToList();

        //var tContent = File.ReadAllText(templatePath);
        //var tResult = _viewEngine.RunCompileFromCached(tContent, new
        //{
        //    NameSpace = $"{input.Position}.Entity",
        //    input.TableName,
        //    input.EntityName,
        //    BaseClassName = string.IsNullOrWhiteSpace(input.BaseClassName) ? "" : $": {input.BaseClassName}",
        //    input.ConfigId,
        //    dbTableInfo.Description,
        //    TableField = dbColumnInfos
        //});
        //File.WriteAllText(targetPath, tResult, Encoding.UTF8);
    }

    /// <summary>
    /// 创建种子数据 🔖
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [DisplayName("创建种子数据")]
    public async Task CreateSeedData([FromBody] CreateSeedDataInput input)
    {
        var config = NacosConfigUtil.Read<DbConnectionOptions>()!.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == input.ConfigId);
        input.Position = string.IsNullOrWhiteSpace(input.Position) ? "Admin.NET.Core" : input.Position;

        //var templatePath = GetSeedDataTemplatePath();
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var tableInfo = db.DbMaintenance.GetTableInfoList(false).First(u => u.Name == input.TableName); // 表名
        List<DbColumnInfo> dbColumnInfos = db.DbMaintenance.GetColumnInfosByTableName(input.TableName, false); // 所有字段
        IEnumerable<EntityInfo> entityInfos = await GetEntityInfos();
        Type? entityType = null;
        foreach (var item in entityInfos)
        {
            if (tableInfo.Name.ToLower() != (config!.DbSettings.EnableUnderLine ? item.DbTableName.ToUnderLine() : item.DbTableName).ToLower()) continue;
            entityType = item.Type;
            break;
        }
        if (entityType == null) throw FriendlyError.Ex(EnumErrorCode.db1001);

        input.EntityName = entityType.Name;
        input.SeedDataName = entityType.Name + "SeedData";
        if (!string.IsNullOrWhiteSpace(input.Suffix)) input.SeedDataName += input.Suffix;

        // 查询所有数据
        var query = db.QueryableByObject(entityType);
        // 优先用创建时间排序
        DbColumnInfo? orderField = dbColumnInfos.FirstOrDefault(u => u.DbColumnName.ToLower() == "create_time" || u.DbColumnName.ToLower() == "createtime");
        if (orderField != null) query = query.OrderBy(orderField.DbColumnName);
        // 再使用第一个主键排序
        query = query.OrderBy(dbColumnInfos.First(u => u.IsPrimarykey).DbColumnName);
        IEnumerable records = (IEnumerable)(await query.ToListAsync());

        // 过滤已存在的数据
        if (input.FilterExistingData && records.Any())
        {
            // 获取实体类型-所有种数据数据类型
            var entityTypes = WebApp.Types!.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false) && u.FullName!.EndsWith("." + input.EntityName))
                .Where(u => !u.GetCustomAttributes<IgnoreTableAttribute>().Any())
                .ToList();
            if (entityTypes.Count == 1) // 只有一个实体匹配才能过滤
            {
                // 获取实体的主键对应的属性名称
                var pkInfo = entityTypes[0].GetProperties().FirstOrDefault(u => u.GetCustomAttribute<SugarColumn>()?.IsPrimaryKey == true);
                if (pkInfo != null)
                {
                    var seedDataTypes = RuntimeUtil.GetTypes()!
                        .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.GetInterfaces().Any(
                            i => i.HasImplementedRawGeneric(typeof(ISeedData<>)) && i.GenericTypeArguments[0] == entityTypes[0]
                            )
                        )
                        .ToList();
                    // 可能会重名的种子数据不作为过滤项
                    string doNotFilterFullName1 = $"{input.Position}.SeedData.{input.SeedDataName}";
                    string doNotFilterFullName2 = $"{input.Position}.{input.SeedDataName}"; // Core中的命名空间没有SeedData

                    PropertyInfo idPropertySeedData = records.First().GetType().GetProperty("Id");

                    for (int i = seedDataTypes.Count - 1; i >= 0; i--)
                    {
                        string? fullName = seedDataTypes[i].FullName;
                        if ((fullName == doNotFilterFullName1) || (fullName == doNotFilterFullName2)) continue;

                        // 删除重复数据
                        var instance = Activator.CreateInstance(seedDataTypes[i]);
                        var hasDataMethod = seedDataTypes[i].GetMethod("HasData");
                        var seedData = ((IEnumerable?)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
                        if (seedData == null) continue;

                        List<object> recordsToRemove = new();
                        foreach (var record in records)
                        {
                            object recordId = pkInfo.GetValue(record!)!;
                            if (seedData.Select(d1 => idPropertySeedData.GetValue(d1)).Any(dataId => recordId != null && dataId != null && recordId.Equals(dataId)))
                            {
                                recordsToRemove.Add(record);
                            }
                        }
                        foreach (var itemToRemove in recordsToRemove)
                        {
                            records.Remove(itemToRemove);
                        }
                    }
                }
            }
        }


        // 获取所有字段信息
        var propertyList = entityType.GetProperties().Where(x => false == (x.GetCustomAttribute<SugarColumn>()?.IsIgnore ?? false)).ToList();
        for (var i = 0; i < propertyList.Count; i++)
        {
            if (propertyList[i].Name != nameof(EntityBaseId.Id) || !(propertyList[i].GetCustomAttribute<SugarColumn>()?.IsPrimaryKey ?? true)) continue;
            var temp = propertyList[i];
            for (var j = i; j > 0; j--) propertyList[j] = propertyList[j - 1];
            propertyList[0] = temp;
        }
        // 拼接数据
        //var recordList = records.Select(obj => string.Join(", ", propertyList.Select(prop =>
        //{
        //    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        //    object value = prop.GetValue(obj);
        //    if (value == null) value = "null";
        //    else if (propType == typeof(string))
        //    {
        //        value = $"@\"{value}\"";
        //    }
        //    else if (propType.IsEnum)
        //    {
        //        value = $"{propType.Name}.{value}";
        //    }
        //    else if (propType == typeof(bool))
        //    {
        //        value = (bool)value ? "true" : "false";
        //    }
        //    else if (propType == typeof(DateTime))
        //    {
        //        value = $"DateTime.Parse(\"{((DateTime)value):yyyy-MM-dd HH:mm:ss.fff}\")";
        //    }
        //    return $"{prop.Name}={value}";
        //}))).ToList();

        //var tContent = await File.ReadAllTextAsync(templatePath);
        //var data = new
        //{
        //    NameSpace = $"{input.Position}.SeedData",
        //    EntityNameSpace = entityType.Namespace,
        //    input.TableName,
        //    input.EntityName,
        //    input.SeedDataName,
        //    input.ConfigId,
        //    tableInfo.Description,
        //    // JsonIgnoreInfo = jsonIgnoreInfo,
        //    RecordList = recordList
        //};
        //var tResult = await _viewEngine.RunCompileAsync(tContent, data, builderAction: builder =>
        //{
        //    builder.AddAssemblyReferenceByName("System.Linq");
        //    builder.AddAssemblyReferenceByName("System.Collections");
        //    builder.AddUsing("System.Collections.Generic");
        //    builder.AddUsing("System.Linq");
        //});

        //var targetPath = GetSeedDataTargetPath(input);
        //await File.WriteAllTextAsync(targetPath, tResult, Encoding.UTF8);

    }

    /// <summary>
    /// 获取库表信息
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<EntityInfo>> GetEntityInfos()
    {
        var entityInfos = new List<EntityInfo>();

        var type = typeof(SugarTable);
        var types = new List<Type>();
        //if (_codeGenOptions.EntityAssemblyNames != null)
        //{
        //    foreach (var asm in _codeGenOptions.EntityAssemblyNames.Select(Assembly.Load))
        //    {
        //        types.AddRange(asm.GetExportedTypes().ToList());
        //    }
        //}

        Type[] cosType = types.Where(o => IsMyAttribute(Attribute.GetCustomAttributes(o, true))).ToArray();

        foreach (var c in cosType)
        {
            var sugarAttribute = c.GetCustomAttributes(type, true)?.FirstOrDefault();

            var des = c.GetCustomAttributes(typeof(DescriptionAttribute), true);
            var description = "";
            if (des.Length > 0)
            {
                description = ((DescriptionAttribute)des[0]).Description;
            }
            entityInfos.Add(new EntityInfo()
            {
                EntityName = c.Name,
                DbTableName = sugarAttribute == null ? c.Name : ((SugarTable)sugarAttribute).TableName,
                TableDescription = description,
                Type = c
            });
        }
        return await Task.FromResult(entityInfos);

        bool IsMyAttribute(Attribute[] o)
        {
            return o.Any(a => a.GetType() == type);
        }
    }


}