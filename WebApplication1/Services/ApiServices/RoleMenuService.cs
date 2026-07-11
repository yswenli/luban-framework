/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServicess
*文件名： SysRoleMenuService
*版本号： V1.0.0.0
*唯一标识：3b0fb171-3484-4b82-803b-c0ce3917b236
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 11:31:26
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/7 11:31:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using WebApplication1.Models.Vos;

namespace Services.ApiServices
{
    /// <summary>
    /// 系统角色菜单服务
    /// </summary>
    public class RoleMenuService : BaseService<RoleMenuService>
    {
        private readonly DbRepository<DbRoleMenu> _sysRoleMenuRep;
        /// <summary>
        /// 系统角色菜单服务
        /// </summary>
        public RoleMenuService()
        {
            _sysRoleMenuRep = new DbRepository<DbRoleMenu>();
        }

        /// <summary>
        /// 根据角色Id集合获取菜单Id集合
        /// </summary>
        /// <param name="roleIdList"></param>
        /// <returns></returns>
        public async Task<List<long>> GetRoleMenuIdListAsync(List<long> roleIdList)
        {
            return await _sysRoleMenuRep
                .Where(u => roleIdList.Contains(u.RoleId))
                .Select(u => u.MenuId).ToListAsync();
        }



        /// <summary>
        /// 授权角色菜单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> GrantRoleMenuAsync(RoleMenuInput input)
        {
            await _sysRoleMenuRep.DeleteAsync(u => u.RoleId == input.Id);
            var menus = input.MenuIdList.Select(u => new DbRoleMenu
            {
                RoleId = input.Id,
                MenuId = u
            }).ToList();
            if (menus != null && menus.Count > 0)
                await _sysRoleMenuRep.InsertRangeAsync(menus);

            // 清除缓存
            CacheService.Instance.RemoveByPrefixKey(CacheConst.KeyUserMenu);
            CacheService.Instance.RemoveByPrefixKey(CacheConst.KeyUserButton);
            return true;
        }


        /// <summary>
        /// 根据菜单Id集合删除角色菜单
        /// </summary>
        /// <param name="menuIdList"></param>
        /// <returns></returns>
        public async Task<Result> DeleteRoleMenuByMenuIdListFormController(List<long> menuIdList)
        {
            return await GetResultAsync(async () => await DeleteRoleMenuByMenuIdListAsync(menuIdList));
        }


        public async Task<bool> DeleteRoleMenuByMenuIdListAsync(List<long> menuIdList)
        {
            return await _sysRoleMenuRep.DeleteAsync(u => menuIdList.Contains(u.MenuId));
        }


        /// <summary>
        /// 根据角色Id删除角色菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<Result> DeleteRoleMenuByRoleIdFormController(long roleId)
        {
            return await GetResultAsync(async () => await DeleteRoleMenuByRoleIdAsync(roleId));
        }


        public async Task<bool> DeleteRoleMenuByRoleIdAsync(long roleId)
        {
            return await _sysRoleMenuRep.DeleteAsync(u => u.RoleId == roleId);
        }


    }
}
