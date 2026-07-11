/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： SysAuthService
*版本号： V1.0.0.0
*唯一标识：04c5ac43-8f7b-4f31-b90f-2be3cea356be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 18:51:19
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/


using WebApplication1.Models.Entities;

namespace Services.ApiServices;

public class OrgService : BaseService<OrgService>
{
    private readonly DbRepository<DbOrg> _sysOrgRep;
    private readonly UserExtOrgService _sysUserExtOrgService;
    private readonly RoleOrgService _sysRoleOrgService;

    public OrgService()
    {
        _sysOrgRep = new DbRepository<DbOrg>();
        _sysUserExtOrgService = UserExtOrgService.Instance;
        _sysRoleOrgService = RoleOrgService.Instance;
    }

    /// <summary>
    /// 获取机构列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbOrg>> GetListAsync(OrgInput input)
    {
        // 获取拥有的机构Id集合
        var userOrgIdList = await GetUserOrgIdList();

        var iSugarQueryable = _sysOrgRep.Where(q => q.IsDelete == false).OrderBy(u => u.OrderNo);

        // 带条件筛选时返回列表数据
        if (!string.IsNullOrWhiteSpace(input.Name) || !string.IsNullOrWhiteSpace(input.Code) || !string.IsNullOrWhiteSpace(input.Type))
        {
            return await iSugarQueryable.WhereIF(userOrgIdList.Count > 0, u => userOrgIdList.Contains(u.Id))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code == input.Code)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Type), u => u.Type == input.Type)
                .ToListAsync();
        }

        var sysOrg = await _sysOrgRep.GetSingleAsync(u => u.Id == input.Id);
        var orgTree = new List<DbOrg>();
        if (SessionUser.IsSuperAdmin)
        {
            orgTree = await iSugarQueryable.ToTreeAsync(u => u.Children, u => u.Pid, sysOrg?.Pid);
        }
        else
        {
            orgTree = await iSugarQueryable.ToTreeAsync(u => u.Children, u => u.Pid, sysOrg?.Pid ?? 0, userOrgIdList.Select(d => (object)d).ToArray());
            // 递归禁用没权限的机构（防止用户修改或创建无权的机构和用户）
            HandlerOrgTree(orgTree, userOrgIdList);
        }
        return orgTree;
    }



    /// <summary>
    /// 递归禁用没权限的机构
    /// </summary>
    /// <param name="orgTree"></param>
    /// <param name="userOrgIdList"></param>
    private void HandlerOrgTree(List<DbOrg> orgTree, List<long> userOrgIdList)
    {
        foreach (var org in orgTree)
        {
            org.Disabled = !userOrgIdList.Contains(org.Id); // 设置禁用/不可选择
            if (org.Children != null)
                HandlerOrgTree(org.Children, userOrgIdList);
        }
    }

    /// <summary>
    /// 增加机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<long> AddOrgAsync(AddOrgInput input)
    {
        if (!SessionUser.IsSuperAdmin && input.Pid == 0)
            throw FriendlyError.Ex(EnumErrorCode.D2009);

        if (await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
            throw FriendlyError.Ex(EnumErrorCode.D2002);

        if (!SessionUser.IsSuperAdmin && input.Pid != 0)
        {
            // 新增机构父Id不是0，则进行权限校验
            var orgIdList = await GetUserOrgIdList();
            // 新增机构的父机构不在自己的数据范围内
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Pid))
                throw FriendlyError.Ex(EnumErrorCode.D2003);
        }

        // 删除与此父机构有关的用户机构缓存
        var pOrg = await _sysOrgRep.FirstAsync(u => u.Id == input.Pid);
        if (pOrg != null)
            DeleteUserOrgCache(pOrg.Id, pOrg.Pid);

        var newOrg = await _sysOrgRep.AsInsertable(input.Adapt<DbOrg>()).ExecuteReturnEntityAsync();
        return newOrg.Id;
    }


    /// <summary>
    /// 更新机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateOrgAsync(UpdateOrgInput input)
    {
        if (!SessionUser.IsSuperAdmin && input.Pid == 0)
            throw FriendlyError.Ex(EnumErrorCode.D2009);

        if (input.Pid != 0)
        {
            //var pOrg = await _sysOrgRep.FirstAsync(u => u.Id == input.Pid);
            //_ = pOrg ?? throw FriendlyError.Ex(EnumErrorCode.D2000);

            // 若父机构发生变化则清空用户机构缓存
            var sysOrg = await _sysOrgRep.FirstAsync(u => u.Id == input.Id);
            if (sysOrg != null && sysOrg.Pid != input.Pid)
            {
                // 删除与此机构、新父机构有关的用户机构缓存
                DeleteUserOrgCache(sysOrg.Id, input.Pid);
            }
        }
        if (input.Id == input.Pid)
            throw FriendlyError.Ex(EnumErrorCode.D2001);

        if (await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
            throw FriendlyError.Ex(EnumErrorCode.D2002);

        // 父Id不能为自己的子节点
        var childIdList = await GetChildIdListWithSelfById(input.Id);
        if (childIdList.Contains(input.Pid))
            throw FriendlyError.Ex(EnumErrorCode.D2001);

        // 是否有权限操作此机构
        if (!SessionUser.IsSuperAdmin)
        {
            var orgIdList = await GetUserOrgIdList();
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Id))
                throw FriendlyError.Ex(EnumErrorCode.D2003);
        }
        await _sysOrgRep.AsUpdateable(input.Adapt<DbOrg>()).IgnoreColumns(true).ExecuteCommandAsync();
        return true;
    }


    /// <summary>
    /// 删除机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteOrgAsync(DeleteOrgInput input)
    {
        var sysOrg = await _sysOrgRep.FirstAsync(u => u.Id == input.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D1002);

        // 是否有权限操作此机构
        if (!SessionUser.IsSuperAdmin)
        {
            var orgIdList = await GetUserOrgIdList();
            if (orgIdList.Count < 1 || !orgIdList.Contains(sysOrg.Id))
                throw FriendlyError.Ex(EnumErrorCode.D2003);
        }

        // 若机构为租户默认机构禁止删除
        var isTenantOrg = await _sysOrgRep.ChangeRepository<DbTenant>()
            .IsAnyAsync(u => u.OrgId == input.Id);
        if (isTenantOrg)
            throw FriendlyError.Ex(EnumErrorCode.D2008);

        // 若机构有用户则禁止删除
        var orgHasEmp = await _sysOrgRep.ChangeRepository<DbUser>()
            .IsAnyAsync(u => u.OrgId == input.Id);
        if (orgHasEmp)
            throw FriendlyError.Ex(EnumErrorCode.D2004);

        // 若扩展机构有用户则禁止删除
        var hasExtOrgEmp = await _sysUserExtOrgService.HasUserOrgAsync(sysOrg.Id);
        if (hasExtOrgEmp)
            throw FriendlyError.Ex(EnumErrorCode.D2005);

        // 若子机构有用户则禁止删除
        var childOrgTreeList = await _sysOrgRep.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        var childOrgIdList = childOrgTreeList.Select(u => u.Id).ToList();

        // 若子机构有用户则禁止删除
        var cOrgHasEmp = await _sysOrgRep.ChangeRepository<DbUser>()
            .IsAnyAsync(u => childOrgIdList.Contains(u.OrgId));
        if (cOrgHasEmp)
            throw FriendlyError.Ex(EnumErrorCode.D2007);

        // 删除与此机构、父机构有关的用户机构缓存
        DeleteUserOrgCache(sysOrg.Id, sysOrg.Pid);

        // 级联删除机构子节点
        await _sysOrgRep.DeleteAsync(u => childOrgIdList.Contains(u.Id));

        // 级联删除角色机构数据
        await _sysRoleOrgService.DeleteRoleOrgByOrgIdListAsync(childOrgIdList);

        // 级联删除用户机构数据
        return await _sysUserExtOrgService.DeleteUserExtOrgByOrgIdListAsync(childOrgIdList);
    }


    /// <summary>
    /// 删除与此机构、父机构有关的用户机构缓存
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="orgPid"></param>
    private void DeleteUserOrgCache(long orgId, long orgPid)
    {
        var userOrgKeyList = CacheService.Instance.GetKeysByPrefixKey(CacheConst.KeyUserOrg);
        if (userOrgKeyList != null && userOrgKeyList.Count > 0)
        {
            foreach (var userOrgKey in userOrgKeyList)
            {
                var userOrgs = CacheService.Instance.Get<List<long>>(userOrgKey) ?? [];
                if (userOrgs.Contains(orgId) || userOrgs.Contains(orgPid))
                {
                    var userId = long.Parse(userOrgKey.Substring(CacheConst.KeyUserOrg));
                    DataScopePermissionFilter.DeleteUserOrgCache(userId, _sysOrgRep.Context.CurrentConnectionConfig.ConfigId);
                }
            }
        }
    }



    /// <summary>
    /// 根据用户Id获取机构Id集合
    /// </summary>
    /// <returns></returns>
    public async Task<List<long>> GetUserOrgIdList()
    {
        if (SessionUser.IsSuperAdmin)
            return [];

        var userId = SessionUser.UserId;
        var orgIdList = CacheService.Instance.Get<List<long>>($"{CacheConst.KeyUserOrg}{userId}") ?? []; // 取缓存
        if (orgIdList.Count < 1)
        {
            // 本人新建机构集合
            var orgList0 = await _sysOrgRep.Where(u => u.CreateUserId == userId).Select(u => u.Id).ToListAsync();
            // 扩展机构集合
            var orgList1 = await _sysUserExtOrgService.GetUserExtOrgListAsync(userId);
            // 角色机构集合
            var orgList2 = await GetUserRoleOrgIdList();
            // 机构并集
            orgIdList = orgList1.Select(u => u.OrgId).Union(orgList2).Union(orgList0).ToList() ?? [];
            // 当前所属机构
            if (!orgIdList.Contains(SessionUser.OrgId))
                orgIdList.Add(SessionUser.OrgId);
            CacheService.Instance.Set($"{CacheConst.KeyUserOrg}{userId}", orgIdList); // 存缓存
        }
        return orgIdList;
    }

    /// <summary>
    /// 获取用户角色机构Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<List<long>> GetUserRoleOrgIdList()
    {
        var roleList = await UserRoleService.Instance.GetUserRoleListAsync(SessionUser.UserId);
        if (roleList.Count < 1)
            return []; // 空机构Id集合

        return await GetUserOrgIdList(roleList);
    }


    /// <summary>
    /// 根据角色Id集合获取机构Id集合
    /// </summary>
    /// <param name="roleList"></param>
    /// <returns></returns>
    private async Task<List<long>> GetUserOrgIdList(List<DbRole> roleList)
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
        var orgIdList1 = await _sysRoleOrgService.GetRoleOrgIdListAsync(customDataScopeRoleIdList);
        // 根据数据范围获取机构集合
        var orgIdList2 = await GetOrgIdListByDataScope(strongerDataScopeType, SessionUser.OrgId);

        // 缓存当前用户最大角色数据范围
        CacheService.Instance.Set(CacheConst.KeyRoleMaxDataScope + SessionUser.UserId, strongerDataScopeType);

        // 并集机构集合
        return orgIdList1.Union(orgIdList2).ToList() ?? [];
    }


    /// <summary>
    /// 根据数据范围获取机构Id集合
    /// </summary>
    /// <param name="dataScope"></param>
    /// <returns></returns>
    private async Task<List<long>> GetOrgIdListByDataScope(int dataScope, long orgID)
    {
        var orgId = orgID;
        var orgIdList = new List<long>();
        // 若数据范围是全部，则获取所有机构Id集合
        if (dataScope == (int)EnumDataScope.All)
        {
            orgIdList = await _sysOrgRep.Where(q => q.IsDelete == false && q.Status == EnumEnableStatus.Enable).Select(u => u.Id).ToListAsync() ?? [];
        }
        // 若数据范围是本部门及以下，则获取本节点和子节点集合
        else if (dataScope == (int)EnumDataScope.DeptChild)
        {
            orgIdList = await GetChildIdListWithSelfById(orgId);
        }
        // 若数据范围是本部门不含子节点，则直接返回本部门
        else if (dataScope == (int)EnumDataScope.Dept)
        {
            orgIdList.Add(orgId);
        }
        return orgIdList;
    }


    /// <summary>
    /// 根据节点Id获取子节点Id集合(包含自己)
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public async Task<List<long>> GetChildIdListWithSelfById(long pid)
    {
        var orgTreeList = await _sysOrgRep.Where(q => q.IsDelete == false && q.Status == EnumEnableStatus.Enable).ToChildListAsync(u => u.Pid, pid, true);
        return orgTreeList?.Select(u => u.Id)?.ToList() ?? [];
    }


}
