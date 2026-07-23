/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm
*文件名： DBDataFilter
*版本号： V1.0.0.0
*唯一标识：87d32167-1ea1-4acc-b847-5c33c307e6ba
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:10:13
*描述：数据权限过滤器
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:10:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据权限过滤器
*
*****************************************************************************/

namespace LuBan.Web.Core.Database;

/// <summary>
/// 数据权限过滤器
/// </summary>
public static class DataScopePermissionFilter
{
    /// <summary>
    /// 缓存全局查询过滤器（内存缓存）
    /// </summary>
    static readonly IServiceCache _cache = WebApp.ServiceCache;


    // <summary>
    /// 根据用户Id获取机构Id集合
    /// </summary>
    /// <returns></returns>
    static List<long> GetUserOrgIdList()
    {
        var userId = SessionUser.UserId;
        List<long>? orgIdList = _cache.Get<List<long>>($"{CacheConst.KeyUserOrg}{userId}");
        if (orgIdList == null || orgIdList.Count < 1)
        {
            // 本人新建机构集合
            var orgList0 = new BaseRepository<DbOrg>().AsQueryable().Where(u => u.CreateUserId == userId).Select(u => u.Id).ToList();
            // 扩展机构集合
            var orgList1 = new BaseRepository<DbUserExtOrg>().GetList(u => u.UserId == userId);
            // 角色机构集合
            var orgList2 = GetUserRoleOrgIdList();
            // 机构并集
            orgIdList = orgList1.Select(u => u.OrgId).Union(orgList2).Union(orgList0).ToList();
            // 当前所属机构
            if (!orgIdList.Contains(SessionUser.OrgId))
                orgIdList.Add(SessionUser.OrgId);
            _cache.Set($"{CacheConst.KeyUserOrg}{userId}", orgIdList); // 存缓存
        }
        return orgIdList;
    }

    /// <summary>
    /// 根据用户Id获取机构Id集合
    /// </summary>
    /// <returns></returns>
    public static async Task<List<long>> GetUserOrgIdListAsync()
    {
        var userId = SessionUser.UserId;
        List<long>? orgIdList = _cache.Get<List<long>>($"{CacheConst.KeyUserOrg}{userId}");
        if (orgIdList == null || orgIdList.Count < 1)
        {
            // 本人新建机构集合
            var orgList0 = await new BaseRepository<DbOrg>().AsQueryable().Where(u => u.CreateUserId == userId).Select(u => u.Id).ToListAsync();
            // 扩展机构集合
            var orgList1 = await new BaseRepository<DbUserExtOrg>().GetListAsync(u => u.UserId == userId);
            // 角色机构集合
            var orgList2 = await GetUserRoleOrgIdListAsync();
            // 机构并集
            orgIdList = orgList1.Select(u => u.OrgId).Union(orgList2).Union(orgList0).ToList();
            // 当前所属机构
            if (!orgIdList.Contains(SessionUser.OrgId))
                orgIdList.Add(SessionUser.OrgId);
            _cache.Set($"{CacheConst.KeyUserOrg}{userId}", orgIdList); // 存缓存
        }
        return orgIdList;
    }

    /// <summary>
    /// 获取用户角色机构Id集合
    /// </summary>
    /// <returns></returns>
    public static List<long> GetUserRoleOrgIdList()
    {
        var roleList = GetUserRoleList(SessionUser.UserId);
        if (roleList.Count < 1)
            return []; // 空机构Id集合

        return GetUserOrgIdList(roleList);
    }

    /// <summary>
    /// 获取用户角色机构Id集合
    /// </summary>
    /// <returns></returns>
    static async Task<List<long>> GetUserRoleOrgIdListAsync()
    {
        var roleList = await GetUserRoleListAsync(SessionUser.UserId);
        if (roleList.Count < 1)
            return []; // 空机构Id集合

        return await GetUserOrgIdListAsync(roleList);
    }


    static List<DbRole> GetUserRoleList(long userId)
    {
        var sysUserRoleList = new BaseRepository<DbUserRole>().AsQueryable()
            .Mapper(u => u.SysRole, u => u.RoleId)
            .Where(u => u.UserId == userId).ToList();
        return sysUserRoleList.Where(u => u.SysRole != null).Select(u => u.SysRole).ToList();
    }


    static async Task<List<DbRole>> GetUserRoleListAsync(long userId)
    {
        var sysUserRoleList = await new BaseRepository<DbUserRole>().AsQueryable()
            .Mapper(u => u.SysRole, u => u.RoleId)
            .Where(u => u.UserId == userId).ToListAsync();
        return sysUserRoleList.Where(u => u.SysRole != null).Select(u => u.SysRole).ToList();
    }

    /// <summary>
    /// 根据角色Id集合获取机构Id集合
    /// </summary>
    /// <param name="roleList"></param>
    /// <returns></returns>
    private static List<long> GetUserOrgIdList(List<DbRole> roleList)
    {
        // 按最大范围策略设定(若同时拥有ALL和SELF权限，则结果ALL)
        int strongerDataScopeType = (int)EnumDataScope.Self;

        // 角色集合拥有的数据范围
        var customDataScopeRoleIdList = new List<long>();
        if (roleList != null && roleList.Count > 0)
        {
            roleList.ForEach(u =>
            {
                if (u.DataScope == EnumDataScope.Define)
                {
                    customDataScopeRoleIdList.Add(u.Id);
                    strongerDataScopeType = (int)u.DataScope; // 自定义数据权限时也要更新最大范围
                }
                else if ((int)u.DataScope <= strongerDataScopeType)
                    strongerDataScopeType = (int)u.DataScope;
            });
        }

        // 根据角色集合获取机构集合
        var orgIdList1 = new BaseRepository<DbRoleOrg>().Where(u => customDataScopeRoleIdList.Contains(u.RoleId))
            .Select(u => u.OrgId).ToList();
        // 根据数据范围获取机构集合
        var orgIdList2 = GetOrgIdListByDataScope(strongerDataScopeType);

        // 缓存当前用户最大角色数据范围
        _cache.Set(CacheConst.KeyRoleMaxDataScope + SessionUser.UserId, strongerDataScopeType);

        // 并集机构集合
        return orgIdList1.Union(orgIdList2).ToList();
    }

    /// <summary>
    /// 根据角色Id集合获取机构Id集合
    /// </summary>
    /// <param name="roleList"></param>
    /// <returns></returns>
    private async static Task<List<long>> GetUserOrgIdListAsync(List<DbRole> roleList)
    {
        // 按最大范围策略设定(若同时拥有ALL和SELF权限，则结果ALL)
        int strongerDataScopeType = (int)EnumDataScope.Self;

        // 角色集合拥有的数据范围
        var customDataScopeRoleIdList = new List<long>();
        if (roleList != null && roleList.Count > 0)
        {
            roleList.ForEach(u =>
            {
                if (u.DataScope == EnumDataScope.Define)
                {
                    customDataScopeRoleIdList.Add(u.Id);
                    strongerDataScopeType = (int)u.DataScope; // 自定义数据权限时也要更新最大范围
                }
                else if ((int)u.DataScope <= strongerDataScopeType)
                    strongerDataScopeType = (int)u.DataScope;
            });
        }

        // 根据角色集合获取机构集合
        var orgIdList1 = await new BaseRepository<DbRoleOrg>().Where(u => customDataScopeRoleIdList.Contains(u.RoleId))
            .Select(u => u.OrgId).ToListAsync();
        // 根据数据范围获取机构集合
        var orgIdList2 = await GetOrgIdListByDataScopeAsync(strongerDataScopeType);

        // 缓存当前用户最大角色数据范围
        _cache.Set(CacheConst.KeyRoleMaxDataScope + SessionUser.UserId, strongerDataScopeType);

        // 并集机构集合
        return orgIdList1.Union(orgIdList2).ToList();
    }


    /// <summary>
    /// 根据数据范围获取机构Id集合
    /// </summary>
    /// <param name="dataScope"></param>
    /// <returns></returns>
    static List<long> GetOrgIdListByDataScope(int dataScope)
    {
        var orgId = SessionUser.OrgId;
        var orgIdList = new List<long>();
        // 若数据范围是全部，则获取所有机构Id集合
        if (dataScope == (int)EnumDataScope.All)
        {
            orgIdList = new BaseRepository<DbOrg>().AsQueryable().Select(u => u.Id).ToList();
        }
        else if (dataScope == (int)EnumDataScope.DeptChild)
        {
            orgIdList = (new BaseRepository<DbOrg>().AsQueryable().ToChildList(u => u.Pid, orgId, true)).Select(u => u.Id).ToList();
        }
        // 若数据范围是本部门不含子节点，则直接返回本部门
        else if (dataScope == (int)EnumDataScope.Dept)
        {
            orgIdList.Add(orgId);
        }
        return orgIdList;
    }

    /// <summary>
    /// 根据数据范围获取机构Id集合
    /// </summary>
    /// <param name="dataScope"></param>
    /// <returns></returns>
    static async Task<List<long>> GetOrgIdListByDataScopeAsync(int dataScope)
    {
        var orgId = SessionUser.OrgId;
        var orgIdList = new List<long>();
        // 若数据范围是全部，则获取所有机构Id集合
        if (dataScope == (int)EnumDataScope.All)
        {
            orgIdList = await new BaseRepository<DbOrg>().AsQueryable().Select(u => u.Id).ToListAsync();
        }
        else if (dataScope == (int)EnumDataScope.DeptChild)
        {
            orgIdList = (await new BaseRepository<DbOrg>().AsQueryable().ToChildListAsync(u => u.Pid, orgId, true)).Select(u => u.Id).ToList();
        }
        // 若数据范围是本部门不含子节点，则直接返回本部门
        else if (dataScope == (int)EnumDataScope.Dept)
        {
            orgIdList.Add(orgId);
        }
        return orgIdList;
    }

    /// <summary>
    /// 配置用户仅本人数据过滤器
    /// </summary>
    /// <param name="db"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    static int SetDataScopeFilterForUser(SqlSugarScopeProvider db, long userId)
    {
        var maxDataScope = (int)EnumDataScope.All;

        if (userId < 1) return maxDataScope;

        var userOrgId = SessionUser.OrgId;
        if (userOrgId < 1) return maxDataScope;

        // 获取用户最大数据范围---仅本人数据
        maxDataScope = _cache.Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
        // 若为0则获取用户机构组织集合建立缓存
        if (maxDataScope == 0)
        {
            // 获取用户所属机构
            var orgIdList = _cache.Get<List<long>>($"{CacheConst.KeyUserOrg}{userId}"); // 取缓存
            if (orgIdList == null || orgIdList.Count < 1)
            {
                GetUserOrgIdList();
                maxDataScope = _cache.Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
            }
        }
        if (maxDataScope != (int)EnumDataScope.Self) return maxDataScope;

        // 配置用户数据范围缓存
        var cacheKey = $"{CacheConst.KeySystem}db:{db.CurrentConnectionConfig.ConfigId}:dataScope:{userId}";
        var dataScopeFilterDic = _cache.Get<ConcurrentDictionary<Type, LambdaExpression>>(cacheKey);
        if (dataScopeFilterDic == null)
        {
            // 获取业务实体数据表
            var entityTypes = WebApp.Types!.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                && u.IsSubclassOf(typeof(EntityDataScoreBase)));
            if (!entityTypes.Any()) return maxDataScope;

            dataScopeFilterDic = new ConcurrentDictionary<Type, LambdaExpression>();
            foreach (var entityType in entityTypes)
            {
                // 排除非当前数据库实体
                var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                if ((tAtt != null && db.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString()))
                    continue;

                var lambda = entityType.GetConditionExpression<OwnerUserAttribute>([userId]);
                db.QueryFilter.AddTableFilter(entityType, lambda);
                dataScopeFilterDic.TryAdd(entityType, lambda);
            }
            _cache.Set(cacheKey, dataScopeFilterDic);
        }
        else
        {
            foreach (var filter in dataScopeFilterDic)
                db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
        }
        return maxDataScope;
    }


    /// <summary>
    /// 删除用户机构缓存
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tanentId"></param>
    public static bool DeleteUserOrgCache(long userId, long tanentId)
    {
        // 删除用户机构集合缓存
        _cache.Delete($"{CacheConst.KeyUserOrg}{userId}");
        // 删除最大数据权限缓存
        _cache.Delete($"{CacheConst.KeyRoleMaxDataScope}{userId}");
        // 删除用户机构（数据范围）缓存——过滤器
        _cache.Delete($"{CacheConst.KeySystem}db:{tanentId}:orgList:{userId}");
        return true;
    }

    /// <summary>
    /// 删除用户机构缓存
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tanentId"></param>
    /// <returns></returns>
    public static bool DeleteUserOrgCache(long userId, object? tanentId)
    {
        var tid = LuBanOrmConst.DefaultTenantId;
        if (tanentId == null)
        {
            return DeleteUserOrgCache(userId, tid);
        }
        else
        {
            return DeleteUserOrgCache(userId, Convert.ToInt64(tanentId));
        }
    }

    /// <summary>
    /// 配置用户机构集合过滤器,
    /// 数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
    /// </summary>
    internal static void SetOrgDataScopeFilter(SqlSugarScopeProvider db)
    {
        //无用户的或是超级管理员，则跳过
        var userId = SessionUser.UserId;
        if (userId < 1 || userId == LuBanOrmConst.SuperAdminId) return;

        // 若仅本人数据，则直接返回
        var maxDataScope = SetDataScopeFilterForUser(db, userId);
        if (maxDataScope == (int)EnumDataScope.Undefined || maxDataScope == (int)EnumDataScope.Self) return;

        // 如果是全部数据，则跳过
        if (maxDataScope == (int)EnumDataScope.All) return;

        // 配置用户机构集合缓存
        var cacheKey = $"{CacheConst.KeySystem}db:{db.CurrentConnectionConfig.ConfigId}:orgList:{userId}";
        var orgFilter = _cache.Get<ConcurrentDictionary<Type, LambdaExpression>>(cacheKey);
        if (orgFilter == null || orgFilter.Count < 1)
        {
            // 获取用户所属机构，保证同一作用域
            var orgIds = GetUserOrgIdList();
            if (orgIds == null || orgIds.Count == 0) return;

            // 获取业务实体数据表
            var entityTypes = WebApp.Types!.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                && u.IsSubclassOf(typeof(EntityDataScoreBase)));
            if (!entityTypes.Any()) return;

            orgFilter = new ConcurrentDictionary<Type, LambdaExpression>();
            foreach (var entityType in entityTypes)
            {
                // 排除非当前数据库实体
                var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                if ((tAtt != null && db.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString()))
                    continue;
                var lambda = entityType.GetConditionExpression<OwnerOrgAttribute>(orgIds);
                db.QueryFilter.AddTableFilter(entityType, lambda);
                orgFilter.TryAdd(entityType, lambda);
            }
            _cache.Set(cacheKey, orgFilter);
        }
        else
        {
            foreach (var filter in orgFilter)
                db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
        }
    }

}
