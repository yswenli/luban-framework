/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： SysUserRoleService
*版本号： V1.0.0.0
*唯一标识：a6ad3177-125c-49b0-8a5a-16a0fbbd939a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 11:22:19
*描述：系统用户角色服务
*
*=================================================
*修改标记
*修改时间：2023/12/7 11:22:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统用户角色服务
*
*****************************************************************************/
using WebApplication1.Models.Vos;

namespace Services.ApiServices;

/// <summary>
/// 系统用户角色服务
/// </summary>
public class UserRoleService : BaseService<UserRoleService>
{

    private readonly BaseRepository<DbUserRole> _sysUserRoleRep;

    /// <summary>
    /// 系统用户角色服务
    /// </summary>
    public UserRoleService()
    {
        _sysUserRoleRep = new BaseRepository<DbUserRole>();
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantUserRoleAsync(UserRoleInput input)
    {
        await _sysUserRoleRep.DeleteAsync(u => u.UserId == input.UserId);

        if (input.RoleIdList == null || input.RoleIdList.Count < 1) return false;
        var roles = input.RoleIdList.Select(u => new DbUserRole
        {
            UserId = input.UserId,
            RoleId = u
        }).ToList();
        await _sysUserRoleRep.InsertRangeAsync(roles);
        CacheService.Instance.Remove(CacheConst.KeyUserButton + input.UserId);
        return true;
    }

    /// <summary>
    /// 根据角色Id删除用户角色
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserRoleByRoleIdAsync(long roleId)
    {
        await _sysUserRoleRep
             .Where(u => u.RoleId == roleId)
             .Select(u => u.UserId)
             .ForEachAsync(userId =>
             {
                 CacheService.Instance.Remove(CacheConst.KeyUserButton + userId);
             });

        return await _sysUserRoleRep.DeleteAsync(u => u.RoleId == roleId);
    }

    /// <summary>
    /// 根据用户Id删除用户角色
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserRoleByUserIdAsync(long userId)
    {
        await _sysUserRoleRep.DeleteAsync(u => u.UserId == userId);
        CacheService.Instance.Remove(CacheConst.KeyUserButton + userId);
        return true;
    }

    /// <summary>
    /// 根据用户Id获取角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<DbRole>> GetUserRoleListAsync(long userId)
    {
        var sysUserRoleList = await _sysUserRoleRep
            .Includes(u => u.SysRole)
            .Where(u => u.UserId == userId).ToListAsync();
        return sysUserRoleList.Where(u => u.SysRole != null).Select(u => u.SysRole).ToList() ?? [];
    }
    /// <summary>
    /// 根据用户Id获取角色Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserRoleIdListAsync(long userId)
    {
        return await _sysUserRoleRep
            .Where(u => u.UserId == userId).Select(u => u.RoleId).ToListAsync();
    }

    /// <summary>
    /// 根据角色Id获取用户Id集合
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserIdListAsync(long roleId)
    {
        return await _sysUserRoleRep
            .Where(u => u.RoleId == roleId).Select(u => u.UserId).ToListAsync();
    }
}
