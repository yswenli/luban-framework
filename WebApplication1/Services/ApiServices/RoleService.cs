/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServicess.SysServices
*文件名： SysRoleService
*版本号： V1.0.0.0
*唯一标识：21413b05-3bfa-4c4a-ba2e-ce73c5f18c99
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/8 11:33:14
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/8 11:33:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

using WebApplication1.Models.Entities;
using WebApplication1.Models.Vos;

namespace Services.ApiServices;

/// <summary>
/// 系统角色服务
/// </summary>
public class RoleService : BaseService<RoleService>
{
    private DbRepository<DbRole> _sysRoleRep => new();

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<DbRole>> PageAsync(PageRoleInput input)
    {
        return await _sysRoleRep
            .WhereIF(!SessionUser.IsSuperAdmin, u => u.CreateUserId == SessionUser.UserId) // 若非超管，则只能操作自己创建的角色
            .WhereIF(input.Name.IsNotNullOrWhiteSpace(), u => u.Name.Contains(input.Name))
            .WhereIF(input.Code.IsNotNullOrWhiteSpace(), u => u.Code != null && u.Code.Contains(input.Code))
            .OrderBy(u => u.OrderNo)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<RoleOutput>> GetListAsync()
    {
        // 当前用户已拥有的角色集合
        var roleIdList = SessionUser.IsSuperAdmin ? null : await UserRoleService.Instance.GetUserRoleIdListAsync(SessionUser.UserId);

        return await _sysRoleRep
            // 若非超管，则只显示自己创建和已拥有的角色
            .WhereIF(roleIdList != null, u => u.CreateUserId == SessionUser.UserId || roleIdList!.Contains(u.Id))
            .OrderBy(u => u.OrderNo)
            .Select<RoleOutput>()
            .ToListAsync();
    }

    /// <summary>
    /// 增加角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> AddRoleAsync(AddRoleInput input)
    {
        if (await _sysRoleRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
            throw FriendlyError.Ex(EnumErrorCode.D1006);

        var newRole = await _sysRoleRep.AsInsertable(input.Adapt<DbRole>()).ExecuteReturnEntityAsync();
        input.Id = newRole.Id;
        return await UpdateRoleMenu(input);
    }

    /// <summary>
    /// 更新角色菜单权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task<bool> UpdateRoleMenu(AddRoleInput input)
    {
        if (input.MenuIdList == null)
            return false;

        // 将父节点为0的菜单排除，防止前端全选异常
        var pMenuIds = await _sysRoleRep.ChangeRepository<DbMenu>().Where(u => input.MenuIdList.Contains(u.Id) && u.Pid == 0).ToListAsync(u => u.Id);
        var menuIds = input.MenuIdList.Except(pMenuIds); // 差集
        return await GrantMenu(new RoleMenuInput()
        {
            Id = input.Id,
            MenuIdList = menuIds.ToList()
        });
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateRoleAsync(UpdateRoleInput input)
    {
        if (await _sysRoleRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
            throw FriendlyError.Ex(EnumErrorCode.D1006);

        await _sysRoleRep.AsUpdateable(input.Adapt<DbRole>()).IgnoreColumns(u => new { u.DataScope }).ExecuteCommandAsync();

        return await UpdateRoleMenu(input);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteRoleAsync(DeleteRoleInput input)
    {
        var sysRole = await _sysRoleRep.FirstAsync(u => u.Id == input.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D1002);
        if (sysRole.Code == CommonConst.SysAdminRole)
            throw FriendlyError.Ex(EnumErrorCode.D1019);

        // 级联删除角色机构数据
        await RoleOrgService.Instance.DeleteRoleOrgByRoleIdAsync(sysRole.Id);

        // 级联删除用户角色数据
        await UserRoleService.Instance.DeleteUserRoleByRoleIdAsync(sysRole.Id);

        // 级联删除角色菜单数据
        await RoleMenuService.Instance.DeleteRoleMenuByRoleIdAsync(sysRole.Id);

        //删除角色
        return await _sysRoleRep.DeleteAsync(sysRole);
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantMenu(RoleMenuInput input)
    {
        return await RoleMenuService.Instance.GrantRoleMenuAsync(input);
    }

    /// <summary>
    /// 授权角色数据范围
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantDataScopeAsync(RoleOrgInput input)
    {
        // 删除与该角色相关的用户机构缓存
        var userIdList = await UserRoleService.Instance.GetUserIdListAsync(input.Id);
        foreach (var userId in userIdList)
        {
            DataScopePermissionFilter.DeleteUserOrgCache(userId, _sysRoleRep.Context.CurrentConnectionConfig.ConfigId);
        }

        var role = await _sysRoleRep.FirstAsync(u => u.Id == input.Id);
        var dataScope = input.DataScope;
        if (!SessionUser.IsSuperAdmin)
        {
            // 非超级管理员没有全部数据范围权限
            if (dataScope == (int)EnumDataScope.All)
                throw FriendlyError.Ex(EnumErrorCode.D1016);

            // 若数据范围自定义，则判断授权数据范围是否有权限
            if (dataScope == (int)EnumDataScope.Define)
            {
                var grantOrgIdList = input.OrgIdList;
                if (grantOrgIdList.Count > 0)
                {
                    var orgIdList = await OrgService.Instance.GetUserOrgIdList();
                    if (orgIdList.Count < 1)
                        throw FriendlyError.Ex(EnumErrorCode.D1016);
                    else if (!grantOrgIdList.All(u => orgIdList.Any(c => c == u)))
                        throw FriendlyError.Ex(EnumErrorCode.D1016);
                }
            }
        }
        role.DataScope = (EnumDataScope)dataScope;
        await _sysRoleRep.AsUpdateable(role).UpdateColumns(u => new { u.DataScope }).ExecuteCommandAsync();
        return await RoleOrgService.Instance.GrantRoleOrgAsync(input);
    }

    /// <summary>
    /// 根据角色Id获取菜单Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<long>> GetOwnMenuList(RoleInput input)
    {
        return await RoleMenuService.Instance.GetRoleMenuIdListAsync(new List<long> { input.Id });
    }

    /// <summary>
    /// 根据角色Id获取机构Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<long>> GetOwnOrgList(RoleInput input)
    {
        return await RoleOrgService.Instance.GetRoleOrgIdListAsync(new List<long> { input.Id });
    }

    /// <summary>
    /// 设置角色状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<int> SetStatusAsync(RoleInput input)
    {
        if (!Enum.IsDefined(typeof(EnumEnableStatus), input.Status))
            throw FriendlyError.Ex(EnumErrorCode.D3005);

        return await _sysRoleRep.AsUpdateable()
            .SetColumns(u => u.Status == input.Status)
            .Where(u => u.Id == input.Id)
            .ExecuteCommandAsync();
    }
}
