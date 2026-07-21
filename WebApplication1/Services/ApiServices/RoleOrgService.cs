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
*描述：系统角色机构服务
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统角色机构服务
*
*****************************************************************************/
using WebApplication1.Models.Entities;

namespace Services.ApiServices;

/// <summary>
/// 系统角色机构服务
/// </summary>
public class RoleOrgService : BaseService<RoleOrgService>
{
    private DbRepository<DbRoleOrg> _sysRoleOrgRep => new();

    /// <summary>
    /// 授权角色机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantRoleOrgAsync(RoleOrgInput input)
    {
        var data = await _sysRoleOrgRep.DeleteAsync(u => u.RoleId == input.Id);
        if (input.DataScope == (int)EnumDataScope.Define)
        {
            var roleOrgs = input.OrgIdList.Select(u => new DbRoleOrg
            {
                RoleId = input.Id,
                OrgId = u
            }).ToList();
            return await _sysRoleOrgRep.InsertRangeAsync(roleOrgs);
        }
        return data;
    }



    /// <summary>
    /// 根据角色Id集合获取角色机构Id集合
    /// </summary>
    /// <param name="roleIdList"></param>
    /// <returns></returns>
    public async Task<List<long>> GetRoleOrgIdListAsync(List<long> roleIdList)
    {
        return await _sysRoleOrgRep
            .Where(u => roleIdList.Contains(u.RoleId))
            .Select(u => u.OrgId).ToListAsync() ?? [];
    }




    /// <summary>
    /// 根据机构Id集合删除角色机构
    /// </summary>
    /// <param name="orgIdList"></param>
    /// <returns></returns>
    public async Task<bool> DeleteRoleOrgByOrgIdListAsync(List<long> orgIdList)
    {
        return await _sysRoleOrgRep.DeleteAsync(u => orgIdList.Contains(u.OrgId));
    }



    /// <summary>
    /// 根据角色Id删除角色机构
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteRoleOrgByRoleIdAsync(long roleId)
    {
        return await _sysRoleOrgRep.DeleteAsync(u => u.RoleId == roleId);
    }


}
