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
namespace Services.ApiServices
{

    public class UserExtOrgService : BaseService<UserExtOrgService>
    {
        private readonly DbRepository<DbUserExtOrg> _sysUserExtOrgRep;


        public UserExtOrgService()
        {
            _sysUserExtOrgRep = new DbRepository<DbUserExtOrg>();
        }


        /// <summary>
        /// 获取用户扩展机构集合
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<DbUserExtOrg>> GetUserExtOrgListAsync(long userId)
        {
            return await _sysUserExtOrgRep.GetListAsync(u => u.UserId == userId);
        }
        /// <summary>
        /// 更新用户扩展机构
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="extOrgList"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserExtOrgAsync(long userId, List<DbUserExtOrg> extOrgList)
        {
            await _sysUserExtOrgRep.DeleteAsync(u => u.UserId == userId);

            if (extOrgList == null || extOrgList.Count < 1) return false;
            extOrgList.ForEach(u =>
            {
                u.UserId = userId;
            });
            return await _sysUserExtOrgRep.InsertRangeAsync(extOrgList);
        }
        /// <summary>
        /// 根据机构Id集合删除扩展机构
        /// </summary>
        /// <param name="orgIdList"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserExtOrgByOrgIdListAsync(List<long> orgIdList)
        {
            return await _sysUserExtOrgRep.DeleteAsync(u => orgIdList.Contains(u.OrgId));
        }

        /// <summary>
        /// 根据用户Id删除扩展机构
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserExtOrgByUserIdAsync(long userId)
        {
            return await _sysUserExtOrgRep.DeleteAsync(u => u.UserId == userId);
        }


        /// <summary>
        /// 根据机构Id判断是否有用户
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<bool> HasUserOrgAsync(long orgId)
        {
            return await _sysUserExtOrgRep.IsAnyAsync(u => u.OrgId == orgId);
        }


        /// <summary>
        /// 根据职位Id判断是否有用户
        /// </summary>
        /// <param name="posId"></param>
        /// <returns></returns>
        public async Task<bool> HasUserPosAsync(long posId)
        {
            return await _sysUserExtOrgRep.IsAnyAsync(u => u.PosId == posId);
        }

    }
}
