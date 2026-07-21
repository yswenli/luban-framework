/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm
*文件名： LuBanOrm
*版本号： V1.0.0.0
*唯一标识：4040cd45-d1ac-46b8-b646-cfb3de1841cf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/30 18:37:53
*描述：数据库配置
*
*=================================================
*修改标记
*修改时间：2024/7/30 18:37:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据库配置
*
*****************************************************************************/

namespace LuBan.Orm;

/// <summary>
/// 数据库配置，
/// 基于sqlsugar的ORM框架封装;
/// https://www.donet5.com/home/Doc?typeId=1180
/// </summary>
public static class LuBanOrm
{
    /// <summary>
    /// ORM数据库操作主体类
    /// </summary>
    public static SqlSugarScope SqlSugarScope { get; set; }
    /// <summary>
    /// 缓存全局查询过滤器（内存缓存）
    /// </summary>
    static readonly MemoryCache _cache = MemoryCache.Instance;

    /// <summary>
    /// 数据库配置
    /// </summary>
    public static DbConnectionOptions DbConnectionOptions { get; private set; }

    /// <summary>
    /// 测试数据库连接是否有效
    /// </summary>
    /// <param name="hasApiService"></param>
    /// <returns></returns>
    public static bool IsValidConnection(bool hasApiService = true)
    {
        if (hasApiService)
        {
            return ServiceProviderUtil.GetRequiredService<ISqlSugarClient>().Ado.IsValidConnection();
        }
        else
        {
            return SqlSugarScope.Ado.IsValidConnection();
        }
    }

    /// <summary>
    /// 数据库配置
    /// </summary>
    /// <exception cref="Exception"></exception>
    static LuBanOrm()
    {
        var idConfig = ConfigUtil.Read<IdGeneratorOptions>("SnowId") ?? throw new Exception("SnowId未配置");
        //注册雪花Id
        YitIdHelper.SetIdGenerator(idConfig);
        //SqlSugar 雪花ID算法
        SnowFlakeSingle.WorkId = idConfig.WorkerId;
        StaticConfig.CustomSnowFlakeFunc = () =>
        {
            return YitIdHelper.NextId();
        };
        DbConnectionOptions = ConfigUtil.Read<DbConnectionOptions>() ?? throw new Exception("未配置数据库");
        //根据appsettings配置项初骀化SqlSugar的配置
        DbConnectionOptions.ConnectionConfigs.ForEach(config =>
        {
            SetSqlSugarConfig(config);
        });
    }

    /// <summary>
    /// 初始化数据库表结构及种子数据
    /// </summary>
    /// <param name="configAction"></param>
    public static void Init(Action<SqlSugarClient>? configAction = null)
    {
        using var locker = LockerBuilder.Default.Create("SqlSugarOrm.Init");
        if (SqlSugarScope != null) return;
        try
        {
            SqlSugarScope sqlSugarScope;
            //某些情况下未配置LuBanOrm初始化（非aspnetcore初始化，例如单元测试等场景），则使用默认配置
            if (configAction == null)
            {
                sqlSugarScope = new([.. DbConnectionOptions.ConnectionConfigs], dbClient =>
                {
                    DbConnectionOptions.ConnectionConfigs.ForEach(dbConfig =>
                    {
                        if (dbConfig.ConfigId == null) return;
                        var dbProvider = dbClient.GetConnectionScope(dbConfig.ConfigId);
                        //初始化过滤器
                        SetEntityFilter(dbProvider);
                        //orm执行操作时值处理，例如ID、CreateTime、UpdateTime等
                        SetDefaultValue(dbProvider);
                        //配置日志
                        SetSqlSugarLogs(dbProvider, DbConnectionOptions.EnableConsoleSql, dbConfig);
                        //初始化表结构及种子数据
                        InitDatabase(dbProvider, dbConfig);
                    });
                });
            }
            else
            {
                sqlSugarScope = new([.. DbConnectionOptions.ConnectionConfigs], configAction);
            }
            SqlSugarScope = sqlSugarScope;
        }
        catch (Exception ex)
        {
            Logger.ErrorWithOutEvent("LuBanOrm 初始化失败", ex);
            throw;
        }
    }


    /// <summary>
    /// 配置连接属性
    /// </summary>
    /// <param name="config"></param>
    static DbConnectionConfig SetSqlSugarConfig(DbConnectionConfig config)
    {
        var configureExternalServices = new ConfigureExternalServices
        {
            EntityNameService = (type, entity) => // 处理表
            {
                entity.IsDisabledDelete = true; // 禁止删除非 sqlsugar 创建的列
                                                // 只处理贴了特性[SugarTable]表
                if (!type.GetCustomAttributes<SugarTable>().Any())
                    return;
                if (config.DbSettings.EnableUnderLine && !entity.DbTableName.Contains('_'))
                    entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName); // 驼峰转下划线
            },
            EntityService = (property, column) => // 处理列
            {
                // 只处理贴了特性[SugarColumn]列
                if (property == null) return;
                if (column.IsIgnore) return;

                var attrs = property.GetCustomAttributes<SugarColumn>();
                if (attrs == null || attrs.Count < 1) return;

                if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable) column.IsNullable = true;

                // 驼峰转下划线
                if (config.DbSettings.EnableUnderLine && !column.DbColumnName.Contains('_')) column.DbColumnName = UtilMethods.ToUnderLine(column.DbColumnName);
            }
        };
        config.ConfigureExternalServices = configureExternalServices;
        config.InitKeyType = InitKeyType.Attribute;
        config.IsAutoCloseConnection = true;
        config.MoreSettings = new ConnMoreSettings
        {
            IsAutoRemoveDataCache = true,
            IsAutoDeleteQueryFilter = true, // 启用删除查询过滤器，启用后，需要new DBRepository<T>()，否则会报错
            IsAutoUpdateQueryFilter = true, // 启用更新查询过滤器
            SqlServerCodeFirstNvarchar = true // 采用Nvarchar
        };
        return config;
    }

    /// <summary>
    /// 配置日志
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="enableConsoleLog"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static SqlSugarScopeProvider SetSqlSugarLogs(SqlSugarScopeProvider dbProvider, bool enableConsoleLog, DbConnectionConfig config)
    {
        // 设置超时时间
        dbProvider.Ado.CommandTimeOut = 180;
        // 打印SQL语句
        if (enableConsoleLog)
        {
            dbProvider.Aop.OnLogExecuting = (sql, pars) =>
            {
                ConsoleUtil.WriteLine("【" + DateTime.Now + "执行的SQL语句】\r\n" + UtilMethods.GetSqlString(config.DbType, sql, pars) + "\r\n", color: ConsoleColor.Green);
                Logger.Info("SqlSugar", "Info", sql + "\r\n" + dbProvider.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };
            dbProvider.Aop.OnError = ex =>
            {
                if (ex.Parametres == null) return;
                var pars = dbProvider.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                ConsoleUtil.WriteLine("【" + DateTime.Now + "出现异常的SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n", color: ConsoleColor.Red);
                Logger.Error("SqlSugar", $"{ex.Message}{Environment.NewLine}{ex.Sql}{pars}{Environment.NewLine}");
            };
            dbProvider.Aop.OnLogExecuted = (sql, pars) =>
            {
                // 执行时间超过5秒
                if (dbProvider.Ado.SqlExecutionTime.TotalSeconds > 5)
                {
                    var fileName = dbProvider.Ado.SqlStackTrace.FirstFileName; // 文件名
                    var fileLine = dbProvider.Ado.SqlStackTrace.FirstLine; // 行号
                    var firstMethodName = dbProvider.Ado.SqlStackTrace.FirstMethodName; // 方法名
                    var log = $"【所在文件名】：{fileName}\r\n【代码行数】：{fileLine}\r\n【方法名】：{firstMethodName}\r\n" + $"【sql语句】：{UtilMethods.GetSqlString(config.DbType, sql, pars)}";
                    Logger.Warn("执行时间超过5秒的Sql", new Exception(log));
                    ConsoleUtil.WriteLine(log, color: ConsoleColor.DarkYellow);
                }
            };
        }
        // 差异日志
        if (!config.DbSettings.EnableDiffLog) return dbProvider;
        dbProvider.Aop.OnDiffLogEvent = async u =>
        {
            try
            {
                var logDiff = new DbLogDiff
                {
                    // 操作后记录（字段描述、列名、值、表名、表描述）
                    AfterData = u.AfterData.ToJson(),
                    // 操作前记录（字段描述、列名、值、表名、表描述）
                    BeforeData = u.BeforeData.ToJson(),
                    // 传进来的对象
                    BusinessData = u.BusinessData.ToJson(),
                    // 枚举（insert、update、delete）
                    DiffType = u.DiffType.ToString(),
                    Sql = UtilMethods.GetSqlString(config.DbType, u.Sql, u.Parameters),
                    Parameters = u.Parameters.ToJson(),
                    Elapsed = u.Time == null ? 0 : (long)u.Time.Value.TotalMilliseconds
                };
                await dbProvider.Insertable(logDiff).ExecuteCommandAsync();
                ConsoleUtil.WriteLine(DateTime.Now + $"\r\n*****LuBan.Orm 差异日志开始*****\r\n{Environment.NewLine}{logDiff.ToJson()}{Environment.NewLine}*****LuBan.Orm 差异日志结束*****\r\n", color: ConsoleColor.DarkYellow);
                Logger.Info(logDiff.ToJson() ?? "");
            }
            catch (Exception ex)
            {
                Logger.ErrorWithOutEvent(ex);
            }
        };
        return dbProvider;
    }

    /// <summary>
    /// 配置自定义实体过滤器，
    /// IEntityFilter，TableFilterItem
    /// </summary>
    /// <param name="dbProvider"></param>
    public static void SetEntityFilter(SqlSugarScopeProvider dbProvider)
    {
        try
        {
            dbProvider.QueryFilter.AddTableFilter<IDeletedFilter>(u => u.IsDelete == false);
        }
        catch (Exception ex)
        {
            Logger.Error($"SetEntityFilter AddTableFilter error: {ex.Message}");
        }
        var cacheKey = $"db:{dbProvider.CurrentConnectionConfig.ConfigId}:custom";
        var tableFilterItemList = _cache.Get<List<TableFilterItem<object>>>(cacheKey);
        if (tableFilterItemList == null)
        {
            try
            {
                var entityFilterTypes = RuntimeUtil.GetTypes()?.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                    && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(IEntityFilter))));
                if (entityFilterTypes == null || !entityFilterTypes.Any()) return;

                var tableFilterItems = new List<TableFilterItem<object>>();
                foreach (var entityFilter in entityFilterTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(entityFilter);
                        var entityFilterMethod = entityFilter.GetMethod("AddEntityFilter");
                        var entityFilters = (entityFilterMethod?.Invoke(instance, null) as IList)?.Cast<object>();
                        if (entityFilters == null) continue;

                        foreach (var u in entityFilters)
                        {
                            var tableFilterItem = (TableFilterItem<object>)u;
                            var entityType = tableFilterItem.GetType().GetProperty("type", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(tableFilterItem, null) as Type;
                            if (entityType != null)
                            {
                                var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                                if (tAtt != null && dbProvider.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString() ||
                                    tAtt == null && dbProvider.CurrentConnectionConfig.ConfigId.ToString() != LuBanOrmConst.MainConfigId)
                                    continue;
                            }
                            tableFilterItems.Add(tableFilterItem);
                            try
                            {
                                dbProvider.QueryFilter.Add(tableFilterItem);
                            }
                            catch (Exception ex2)
                            {
                                Logger.Error($"QueryFilter.Add error: {ex2.Message}, EntityType: {entityType?.Name}");
                            }
                        }
                    }
                    catch (Exception ex3)
                    {
                        Logger.Error($"EntityFilter process error: {ex3.Message}, FilterType: {entityFilter?.Name}");
                    }
                }
                _cache.Set(cacheKey, tableFilterItems);
            }
            catch (Exception ex4)
            {
                Logger.Error($"SetEntityFilter process error: {ex4.Message}");
            }
        }
        else
        {
            try
            {
                tableFilterItemList.ForEach(u =>
                {
                    dbProvider.QueryFilter.Add(u);
                });
            }
            catch (Exception ex5)
            {
                Logger.Error($"SetEntityFilter cached items error: {ex5.Message}");
            }
        }
    }

    /// <summary>
    /// 获取所有表实体类型列表
    /// </summary>
    /// <param name="enableIncreTable"></param>
    /// <returns></returns>
    public static List<Type> GetTableEntityTypes(bool enableIncreTable)
    {
        var types = RuntimeUtil.GetTypes(fromCache: false)!;
        return types.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                .WhereIF(enableIncreTable, u => u.IsDefined(typeof(IncreTableAttribute), false))
                .Where(u => !u.GetCustomAttributes<IgnoreTableAttribute>().Any())
                .ToList();
    }

    /// <summary>
    /// 获取所有视图类型列表
    /// </summary>
    /// <returns></returns>
    public static List<Type> GetViewEntityTypes()
    {
        var types = RuntimeUtil.GetTypes(fromCache: false)!;
        return types.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ILuBanOrmView)))).ToList();
    }

    /// <summary>
    /// 获取所有种子数据类型列表
    /// </summary>
    /// <param name="enableIncreSeed"></param>
    /// <returns></returns>
    public static List<Type>? GetTableSeedTypes(bool enableIncreSeed)
    {
        var types = RuntimeUtil.GetTypes(fromCache: false);
        if (types == null) return null;
        return types.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ISeedData<>))))
            .WhereIF(enableIncreSeed, u => u.IsDefined(typeof(IncreSeedAttribute), false))
            .Where(u => !u.GetCustomAttributes<IgnoreTableAttribute>().Any())
            .ToList();
    }


    /// <summary>
    /// 根据配置项初始化数据库结构和数据
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="dbConfig"></param>
    public static void InitDatabase(SqlSugarScopeProvider dbProvider, DbConnectionConfig dbConfig)
    {
        using var locker = LockerBuilder.Default.Create("SqlSugarOrm.InitTableAndSeeds");
        if (IsInitTableAndDataComplete == true) return;
        IsInitTableAndDataComplete = false;
        InitDb(dbProvider, dbConfig);
        var configId = dbConfig.ConfigId?.ToString() ?? "";
        InitTables(dbProvider, dbConfig, configId);
        InitViews(dbProvider, dbConfig, configId);
        InitSeeds(dbProvider, dbConfig, configId);
        IsInitTableAndDataComplete = true;
    }

    /// <summary>
    /// 初始化/创建数据库
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="dbConfig"></param>
    /// <exception cref="Exception"></exception>
    static void InitDb(SqlSugarScopeProvider dbProvider, DbConnectionConfig dbConfig)
    {
        if (dbConfig.DbSettings.EnableInitDb)
        {
            //Oracle、sqlserver、达梦不支持建库，需要手动建库
            if (dbConfig.DbType == DbType.Oracle || dbConfig.DbType == DbType.Dm || dbConfig.DbType == DbType.SqlServer) throw new Exception("Oracle、sqlserver、达梦不支持建库，需要手动建库");
            if (dbConfig.DbType == DbType.MySql)
            {
                StaticConfig.CodeFirst_MySqlCollate = "utf8mb4_general_ci";
            }
            dbProvider.DbMaintenance.CreateDatabase();
        }
    }

    /// <summary>
    /// 初始化表结构
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="dbConfig"></param>
    /// <param name="configId"></param>
    static void InitTables(SqlSugarScopeProvider dbProvider, DbConnectionConfig dbConfig, string configId)
    {
        if (dbConfig.TableSettings.EnableInitTable)
        {
            ConsoleUtil.WriteLineWithCount("正在初始化数据库表结构", color: ConsoleColor.Green);
            var entityTypes = GetTableEntityTypes(dbConfig.TableSettings.EnableIncreTable);

            if (configId == LuBanOrmConst.MainConfigId) // 默认库（有系统表特性、没有日志表和租户表特性）
                entityTypes = entityTypes.Where(u => u.GetCustomAttributes<SysTableAttribute>().Any() || !u.GetCustomAttributes<LogTableAttribute>().Any() && !u.GetCustomAttributes<TenantAttribute>().Any()).ToList();
            else if (configId == LuBanOrmConst.LogConfigId) // 日志库
                entityTypes = entityTypes.Where(u => u.GetCustomAttributes<LogTableAttribute>().Any()).ToList();
            else
                entityTypes = [.. entityTypes.Where(u => u.GetCustomAttribute<TenantAttribute>()?.configId.ToString() == configId)]; // 自定义的库

            ConsoleUtil.PrintProcess(entityTypes, (entityType) =>
            {
                if (entityType.GetCustomAttribute<SplitTableAttribute>() == null)
                    dbProvider.CodeFirst.InitTables(entityType);
                else
                    dbProvider.CodeFirst.SplitTables().InitTables(entityType);
            }, "[yellow]初始化数据库表结构[/]");

            ConsoleUtil.WriteLine($"已完成初始化表结构，共计{entityTypes.Count}张表", color: ConsoleColor.Green);
        }
    }

    /// <summary>
    /// 始化数据库视图
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="dbConfig"></param>
    /// <param name="configId"></param>
    static void InitViews(SqlSugarScopeProvider dbProvider, DbConnectionConfig dbConfig, string configId)
    {
        if (dbConfig.DbSettings.EnableInitView)
        {
            ConsoleUtil.WriteLineWithCount("正在初始化数据库视图", color: ConsoleColor.Green);
            var viewTypes = GetViewEntityTypes();

            ConsoleUtil.PrintProcess(viewTypes, (viewType) =>
            {
                // 获取视图实体和配置信息
                var entityInfo = dbProvider.EntityMaintenance.GetEntityInfo(viewType) ?? throw new Exception("获取视图实体配置有误");

                var viewName = $"View_{entityInfo.DbTableName}";

                // 如果视图存在，则删除视图
                if (dbProvider.DbMaintenance.GetViewInfoList(false).Any(it => it.Name.EqualIgnoreCase(viewName)))
                    dbProvider.DbMaintenance.DropView(viewName);
                // 获取初始化视图查询SQL
                var sql = viewType.GetMethod(nameof(ILuBanOrmView.GetViewSql))?.Invoke(Activator.CreateInstance(viewType), [dbProvider]) as string;
                if (string.IsNullOrWhiteSpace(sql)) throw new Exception("视图初始化Sql语句不能为空");

                // 创建视图
                dbProvider.Ado.ExecuteCommand($"CREATE VIEW {viewName} AS " + Environment.NewLine + " " + sql);
            }, "[yellow]初始化数据库视图[/]");

            ConsoleUtil.WriteLine($"已完成初始化数据库视图，共计{viewTypes.Count}张视图", color: ConsoleColor.Green);
        }
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    /// <param name="dbProvider"></param>
    /// <param name="dbConfig"></param>
    /// <param name="configId"></param>
    static void InitSeeds(SqlSugarScopeProvider dbProvider, DbConnectionConfig dbConfig, string configId)
    {
        if (dbConfig.SeedSettings.EnableInitSeed)
        {
            ConsoleUtil.WriteLineWithCount("正在初始化数据库种子数据", color: ConsoleColor.Green);

            var seedDataTypes = GetTableSeedTypes(dbConfig.SeedSettings.EnableIncreSeed);
            if (seedDataTypes != null && seedDataTypes.Count > 0)
            {
                var seedDataTotal = 0;
                ConsoleUtil.PrintProcess(seedDataTypes, (seedType) =>
                {
                    var entityType = seedType.GetInterfaces().First().GetGenericArguments().First();
                    if (configId == LuBanOrmConst.MainConfigId) // 默认库（有系统表特性、没有日志表和租户表特性）
                    {
                        if (entityType.GetCustomAttribute<SysTableAttribute>() == null && (entityType.GetCustomAttribute<LogTableAttribute>() != null || entityType.GetCustomAttribute<TenantAttribute>() != null))
                            return;
                    }
                    else if (configId == LuBanOrmConst.LogConfigId) // 日志库
                    {
                        if (entityType.GetCustomAttribute<LogTableAttribute>() == null)
                            return;
                    }
                    else
                    {
                        var att = entityType.GetCustomAttribute<TenantAttribute>(); // 自定义的库
                        if (att == null || att.configId.ToString() != configId) return;
                    }

                    var instance = Activator.CreateInstance(seedType);
                    var initDataMethod = seedType.GetMethod("InitData");
                    var seedData = (initDataMethod?.Invoke(instance, null) as IEnumerable)?.Cast<object>();
                    if (seedData == null) return;
                    seedDataTotal += seedData.Count();

                    var entityInfo = dbProvider.EntityMaintenance.GetEntityInfo(entityType);
                    if (entityInfo.Columns.Any(u => u.IsPrimarykey))
                    {
                        // 按主键进行批量增加和更新
                        var storage = dbProvider.StorageableByObject(seedData.ToList()).ToStorage();
                        storage.AsInsertable.ExecuteCommand();
                        storage.AsUpdateable.ExecuteCommand();
                    }
                    else
                    {
                        // 无主键则只进行插入
                        if (!dbProvider.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any())
                            dbProvider.InsertableByObject(seedData.ToList()).ExecuteCommand();
                    }
                }, "[yellow]初始化种子数据[/]");

                ConsoleUtil.WriteLine($"已完成种子数据写入，共计{seedDataTypes.Count}张表{seedDataTotal}条数据", color: ConsoleColor.Green);
            }
        }
    }

    /// <summary>
    /// 是否初始化表和数据完成
    /// </summary>
    public static bool IsInitTableAndDataComplete
    {
        get; set;
    } = false;

    /// <summary>
    /// 初始化默认值
    /// </summary>
    /// <param name="dbProvider"></param>
    static void SetDefaultValue(SqlSugarScopeProvider dbProvider)
    {
        dbProvider.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                if (entityInfo.EntityColumnInfo.IsPrimarykey && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                    if (id == null || (long)id == 0)
                        entityInfo.SetValue(YitIdHelper.NextId());
                }
                if (entityInfo.PropertyName == nameof(EntityTenantId.TenantId))
                {
                    var tenantId = ((dynamic)entityInfo.EntityValue).TenantId;
                    if (tenantId == null || tenantId == 0)
                        entityInfo.SetValue(LuBanOrmConst.MainConfigId);
                }
                if (entityInfo.PropertyName == nameof(EntityBase.CreateUserId))
                {
                    var createUserId = ((dynamic)entityInfo.EntityValue).CreateUserId;
                    if (createUserId == 0 || createUserId == null)
                        entityInfo.SetValue(LuBanOrmConst.DefaultSeedId);
                }
                if (entityInfo.PropertyName == nameof(EntityBase.CreateUserName))
                {
                    var createUserName = ((dynamic)entityInfo.EntityValue).CreateUserName;
                    if (string.IsNullOrEmpty(createUserName))
                        entityInfo.SetValue("系统");
                }
                if (entityInfo.PropertyName == nameof(EntityeDataScoreBase.CreateTime))
                {
                    entityInfo.SetValue(DateTime.Now);
                }
            }
            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                    entityInfo.SetValue(DateTime.Now);
                if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserId))
                    entityInfo.SetValue(LuBanOrmConst.DefaultSeedId);
                if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserName))
                    entityInfo.SetValue(string.Empty);
            }
        };
    }

    #region 获取OrmProvider

    /// <summary>
    /// 获取OrmProvider
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static OrmProvider GetProvider<TEntity>(string tenantId = LuBanOrmConst.MainConfigId)
        where TEntity : EntityBase, IDeletedFilter, new()
    {
        if (SqlSugarScope == null) throw new Exception("LuBanOrm未初始化，请先调用LuBanOrm.Init(configAction)");

        ITenant tenant = SqlSugarScope.AsTenant();
        SqlSugarScopeProvider provider = tenant.GetConnectionScope(tenantId);
        if (typeof(TEntity).IsDefined(typeof(TenantAttribute), false))
        {
            provider = tenant.GetConnectionScopeWithAttr<TEntity>();
        }
        if (typeof(TEntity).IsDefined(typeof(LogTableAttribute), false))
        {
            if (tenant.IsAnyConnection(LuBanOrmConst.LogConfigId))
            {
                provider = tenant.GetConnectionScope(LuBanOrmConst.LogConfigId);
            }
        }
        return new OrmProvider()
        {
            Tenant = tenant,
            Provider = provider
        };
    }

    #endregion

    static ConcurrentDictionary<string, string> _tableNames = new();

    /// <summary>
    /// 获取表属性列表
    /// </summary>
    /// <returns></returns>
    public static List<DbTableInfo> GetTableNames()
    {
        return SqlSugarScope.DbMaintenance.GetTableInfoList();
    }
    /// <summary>
    /// 获取表属性列表
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static List<DbColumnInfo> GetColumns(string tableName)
    {
        return SqlSugarScope.DbMaintenance.GetColumnInfosByTableName(tableName).ToList();
    }

    /// <summary>
    /// 获取基础实体字段
    /// </summary>
    /// <returns></returns>
    public static List<string>? GetEntityBaseFields()
    {
        return MemoryCache.Instance.GetOrSet<List<string>>($"{CacheConst.KeySystem}EntityBase:Fields", (k) => typeof(EntityBase).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToList());
    }

    /// <summary>
    /// 获取表名
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetTableName<T>() where T : EntityBase, new()
    {
        return _tableNames.GetOrAdd(typeof(T).Name, (k) =>
        {
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<SugarTable>()?.TableName ?? "";
            if (tableName.IsNullOrEmpty())
            {
                tableName = type.Name;
            }
            if (!tableName.Contains('_'))
            {
                tableName = UtilMethods.ToUnderLine(tableName);
            }
            return tableName;
        });
    }

    /// <summary>
    /// 获取表名
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    public static string GetTableName(string className)
    {
        if (className.IsNullOrEmpty()) return className;
        return _tableNames.GetOrAdd(className, (k) =>
        {
            var tableName = className;
            if (!tableName.Contains('_'))
            {
                tableName = UtilMethods.ToUnderLine(tableName);
            }
            return tableName;
        });
    }

    /// <summary>
    /// 获取列名
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static string GetColumnName(string propertyName)
    {
        if (propertyName.IsNullOrEmpty()) return propertyName;
        var columnName = propertyName;
        if (!columnName.Contains('_'))
        {
            columnName = UtilMethods.ToUnderLine(columnName);
        }
        return columnName;
    }




    /// <summary>
    /// 配置连接属性
    /// </summary>
    /// <param name="config"></param>
    public static void SetDbConfig(DbConnectionConfig config)
    {
        var configureExternalServices = new ConfigureExternalServices
        {
            EntityNameService = (type, entity) => // 处理表
            {
                entity.IsDisabledDelete = true; // 禁止删除非 sqlsugar 创建的列
                // 只处理贴了特性[SugarTable]表
                if (!type.GetCustomAttributes<SugarTable>().Any())
                    return;
                if (config.DbSettings.EnableUnderLine && !entity.DbTableName.Contains('_'))
                    entity.DbTableName = entity.DbTableName.ToUnderLine(); // 驼峰转下划线
            },
            EntityService = (type, column) => // 处理列
            {
                // 只处理贴了特性[SugarColumn]列
                var sugarColumnAttrs = type.GetCustomAttributes<SugarColumn>();
                if (sugarColumnAttrs == null || !sugarColumnAttrs.Any()) return;
                if (new NullabilityInfoContext().Create(type).WriteState is NullabilityState.Nullable)
                    column.IsNullable = true;
                if (config.DbSettings.EnableUnderLine && !column.IsIgnore && !column.DbColumnName.Contains('_'))
                    column.DbColumnName = column.DbColumnName.ToUnderLine(); // 驼峰转下划线
            }
        };
        config.ConfigureExternalServices = configureExternalServices;
        config.InitKeyType = InitKeyType.Attribute;
        config.IsAutoCloseConnection = true;
        config.MoreSettings = new ConnMoreSettings
        {
            IsAutoRemoveDataCache = true, // 启用自动删除缓存，所有增删改会自动调用.RemoveDataCache()
            IsAutoDeleteQueryFilter = true, // 启用删除查询过滤器
            IsAutoUpdateQueryFilter = true, // 启用更新查询过滤器
            SqlServerCodeFirstNvarchar = true // 采用Nvarchar
        };

        // 若库类型是人大金仓则默认设置PG模式
        if (config.DbType == DbType.Kdbndp)
            config.MoreSettings.DatabaseModel = DbType.PostgreSQL; // 配置PG模式主要是兼容系统表差异

        // 若库类型是Oracle则默认主键名字和参数名字最大长度
        if (config.DbType == DbType.Oracle)
            config.MoreSettings.MaxParameterNameLength = 30;
    }

    /// <summary>
    /// 初始化租户业务数据库
    /// </summary>
    /// <param name="iTenant"></param>
    /// <param name="config"></param>
    public static void CreateTenantDatabase(ITenant iTenant, DbConnectionConfig config)
    {
        SetDbConfig(config);

        if (!iTenant.IsAnyConnection(config.ConfigId.ToString()))
            iTenant.AddConnection(config);
        var db = iTenant.GetConnectionScope(config.ConfigId.ToString());
        db.DbMaintenance.CreateDatabase();

        // 获取所有业务表-初始化租户库表结构（排除系统表、日志表、特定库表）
        var entityTypes = RuntimeUtil.GetTypes()!
            .Where(u => !u.GetCustomAttributes<IgnoreTableAttribute>().Any())
            .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false) &&
            !u.IsDefined(typeof(SysTableAttribute), false) && !u.IsDefined(typeof(LogTableAttribute), false) && !u.IsDefined(typeof(TenantAttribute), false)).ToList();
        if (entityTypes.Count == 0) return;

        foreach (var entityType in entityTypes)
        {
            var splitTable = entityType.GetCustomAttribute<SplitTableAttribute>();
            if (splitTable == null)
                db.CodeFirst.InitTables(entityType);
            else
                db.CodeFirst.SplitTables().InitTables(entityType);
        }
    }
}
