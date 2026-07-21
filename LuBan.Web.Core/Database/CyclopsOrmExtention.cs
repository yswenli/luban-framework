/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： DBDataSetup
*版本号： V1.0.0.0
*唯一标识：82f9dca2-ea6a-45f3-95f0-07abb74f6785
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 18:31:29
*描述：LuBanOrm相关初始化
*
*=================================================
*修改标记
*修改时间：2023/12/1 18:31:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanOrm相关初始化
*
*****************************************************************************/
namespace LuBan.Web.Core.Database;

/// <summary>
/// LuBanOrm相关初始化
/// </summary>
internal static class LuBanOrmExtention
{
    volatile static bool _isFirstValide = true;

    /// <summary>
    /// 初始化LuBanOrm
    /// </summary>
    /// <param name="services"></param>
    internal static void InitDataBaseOrm(this IServiceCollection services)
    {
        //检查数据库连接
        if (NacosConfigUtil.Read<DbConnectionOptions>() != null)
        {
            ConsoleUtil.WriteLineWithCount("正在初始化LuBanOrm", color: ConsoleColor.Green);
            LuBanOrm.Init((dbClient) =>
            {
                LuBanOrm.DbConnectionOptions.ConnectionConfigs.ForEach(config =>
                {
                    var dbProvider = dbClient.GetConnectionScope(config.ConfigId);
                    //初始化过滤器
                    LuBanOrm.SetEntityFilter(dbProvider);
                    //orm执行操作时值处理，例如ID、CreateTime、UpdateTime等
                    SetDefaultValue(dbProvider);
                    //配置日志
                    LuBanOrm.SetSqlSugarLogs(dbProvider, LuBanOrm.DbConnectionOptions.EnableConsoleSql, config);
                    //检查数据库连接配置
                    CheckDbConnection();
                    //初始化表结构及种子数据
                    LuBanOrm.InitDatabase(dbProvider, config);
                });
            });

            //注册工作单元服务到di容器（单例，SqlSugarScope本身是线程安全的）
            services.AddSingleton<ISqlSugarClient>(LuBanOrm.SqlSugarScope);
            //事务与工作单元注册到di容器,配合在控制器上方法的UnitOfWorkAttribute使用（瞬时）
            services.AddTransient<IUnitOfWork, UnitOfWork>();
        }
    }

    /// <summary>
    /// 检查数据库连接
    /// </summary>
    /// <returns></returns>
    public static void CheckDbConnection()
    {
        if (_isFirstValide)
        {
            _isFirstValide = false;
            try
            {
                if (NacosConfigUtil.Read<DbConnectionOptions>() != null)
                {
                    var isValide = ConsoleUtil.PrintProcess(() => LuBanOrm.IsValidConnection(), "正在检查数据库连接有效性...", "yellow");
                    if (!isValide)
                    {
                        var errorMsg = "数据库连接失败，请检查appsetttings.json中的DbConnectionOptions数据库配置";
                        Logger.ErrorWithOutEvent($"An exception occurred during the initialization of cylopsORM.", new Exception(errorMsg));
                        Thread.Sleep(3000);
                        Environment.Exit(-1);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorWithOutEvent("orm中间件处理出现异常", ex);
            }
        }
    }

    /// <summary>
    /// orm执行操作时值处理，例如ID、CreateTime、UpdateTime等
    /// </summary>
    /// <param name="dbProvider"></param>
    static void SetDefaultValue(SqlSugarScopeProvider dbProvider)
    {
        dbProvider.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            try
            {
                if (entityInfo.OperationType == DataFilterType.InsertByObject)
                {
                    if (entityInfo.EntityColumnInfo.IsPrimarykey && !entityInfo.EntityColumnInfo.IsIdentity && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                    {
                        var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                        if (id == null || (long)id == 0)
                            entityInfo.SetValue(YitIdHelper.NextId());
                    }
                    if (entityInfo.PropertyName == nameof(EntityBase.CreateTime))
                    {
                        var createTime = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue)!;
                        if (createTime == null || createTime.Equals(DateTime.MinValue))
                            entityInfo.SetValue(DateTime.Now);
                    }
                    if (entityInfo.PropertyName == nameof(EntityTenantId.TenantId))
                    {
                        var tenantId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                        if (tenantId == null || (long)tenantId == 0)
                            entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.TenantId, LuBanOrmConst.DefaultTenantId));
                    }
                    if (entityInfo.PropertyName == nameof(EntityBase.CreateUserId))
                    {
                        var createUserId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                        if (createUserId == null || (long)createUserId == 0)
                            entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.UserId, LuBanOrmConst.DefaultSeedId));
                    }
                    if (entityInfo.PropertyName == nameof(EntityBase.CreateUserName))
                    {
                        var createUserName = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as string;
                        if (string.IsNullOrEmpty(createUserName))
                            entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.RealName, "系统"));
                    }
                    var safeUserId = GetSafeSessionValue(() => SessionUser.UserId, 0L);
                    if (safeUserId > 0)
                    {
                        if (entityInfo.PropertyName == nameof(EntityeDataScoreBase.CreateOrgId))
                        {
                            var createOrgId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                            if (createOrgId == null || (long)createOrgId == 0)
                                entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.OrgId, 0L));
                        }
                        if (entityInfo.PropertyName == nameof(EntityeDataScoreBase.CreateOrgName))
                        {
                            var createOrgName = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as string;
                            if (string.IsNullOrEmpty(createOrgName))
                                entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.OrgName, ""));
                        }
                    }
                }
                else if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                {
                    if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                        entityInfo.SetValue(DateTime.Now);
                    if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserId))
                        entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.UserId, LuBanOrmConst.DefaultSeedId));
                    if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserName))
                        entityInfo.SetValue(GetSafeSessionValue(() => SessionUser.RealName, string.Empty));
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SetDefaultValue error: {ex.Message}, Property: {entityInfo.PropertyName}, Type: {entityInfo.EntityValue?.GetType().Name}");
            }
        };
    }

    /// <summary>
    /// 安全获取SessionUser值，当HttpContext失效或后台线程执行时返回默认值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="getter"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    static T GetSafeSessionValue<T>(Func<T> getter, T defaultValue)
    {
        try
        {
            if (WebApp.HttpContext == null) return defaultValue;
            return getter();
        }
        catch
        {
            return defaultValue;
        }
    }


    /// <summary>
    /// 数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
    /// </summary>
    /// <param name="app"></param>
    internal static void UseDataScopePermission(this IApplicationBuilder app)
    {
        app.UseMiddleware<LuBanOrmMiddleware>();
    }
}