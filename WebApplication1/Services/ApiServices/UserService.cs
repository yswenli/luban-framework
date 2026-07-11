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
*描述：系统用户服务
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统用户服务
*
*****************************************************************************/
using WebApplication1.Models.Entities;
using WebApplication1.Models.Vos;

namespace Services.ApiServices;

/// <summary>
/// 系统用户服务
/// </summary>
public class UserService : BaseService<UserService>
{
    private readonly DbRepository<DbUser> _sysUserRep;
    private readonly DbRepository<DbUserInfo> _busUserInfoRep;
    private readonly OrgService _sysOrgService;
    private readonly UserExtOrgService _sysUserExtOrgService;


    /// <summary>
    /// 系统用户服务
    /// </summary>
    public UserService()
    {
        _sysUserRep = new DbRepository<DbUser>();
        _busUserInfoRep = new DbRepository<DbUserInfo>();
        _sysOrgService = OrgService.Instance;
        _sysUserExtOrgService = UserExtOrgService.Instance;
    }


    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<PageUserOutput>?> PageAsync(PageUserInput input)
    {
        // 获取用户拥有的机构集合
        var userOrgIdList = await _sysOrgService.GetUserOrgIdList();
        List<long>? orgList = null;
        if (input.OrgId > 0)  // 指定机构查询时
        {
            orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);
            orgList = SessionUser.IsSuperAdmin ? orgList : orgList.Where(u => userOrgIdList.Contains(u)).ToList();
        }
        else // 非超管理员只能看到自己机构下的用户列表
        {
            orgList = SessionUser.IsSuperAdmin ? null : userOrgIdList;
        }

        var result = await _sysUserRep.AsQueryable()
                .WhereIF(orgList != null && orgList.Count > 0, u => orgList!.Contains(u.OrgId))
                .WhereIF(input.Account.IsNotNullOrWhiteSpace(), u => u.Account.Contains(input.Account))
                .WhereIF(input.RealName.IsNotNullOrWhiteSpace(), u => u.RealName.Contains(input.RealName))
                .WhereIF(input.Phone.IsNotNullOrWhiteSpace(), u => u.Phone!.Contains(input.Phone))
                .LeftJoin<DbUserInfo>((u, e) => u.Id == e.UserId)
                .Select((u, e) => new PageUserOutput
                {
                    Id = u.Id,
                    Account = u.Account,
                    Address = u.Address,
                    AuditStatus = e.AuditStatus,
                    Avatar = u.Avatar,
                    Birthday = u.Birthday,
                    CardType = e.CardType,
                    City = e.City,
                    College = e.College,
                    CreateTime = u.CreateTime,
                    CultureLevel = e.CultureLevel,
                    Email = u.Email,
                    EmergencyAddress = e.EmergencyAddress,
                    EmergencyContact = e.EmergencyContact,
                    EmergencyPhone = e.EmergencyPhone,
                    Expertise = e.Expertise,
                    IdCardNum = e.IdCardNum,
                    Introduction = e.Introduction,
                    JobNum = e.JobNum,
                    JoinDate = e.JoinDate,
                    LastLoginAddress = u.LastLoginAddress,
                    LastLoginDevice = u.LastLoginDevice,
                    LastLoginIp = u.LastLoginIp,
                    LastLoginTime = u.LastLoginTime,
                    NickName = u.NickName,
                    Nation = e.Nation,
                    OfficePhone = e.OfficePhone,
                    Office = e.Office,
                    OfficeZone = e.OfficeZone,
                    OrderNo = u.OrderNo,
                    OrgId = u.OrgId,
                    Phone = u.Phone,
                    PoliticalOutlook = e.PoliticalOutlook,
                    PosId = u.PosId,
                    PosLevel = e.PosLevel,
                    Province = e.Province,
                    RealName = u.RealName,
                    Remark = u.Remark,
                    Sex = u.Sex,
                    Signature = u.Signature,
                    Status = u.Status,
                    TenantId = u.TenantId
                })
                .OrderBy(u => u.OrderNo)
                .ToPagedListAsync(input);

        if (result != null && result.Items.Any())
        {
            foreach (var item in result.Items)
            {
                item.RoleIdList = await _sysUserRep.Change<DbUserRole>().AsQueryable().Where(r => r.UserId == item.Id).Select(r => r.RoleId).ToListAsync();
            }
        }

        return result;
    }

    /// <summary>
    /// 增加用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<long> AddUserAsync(AddUserInput input)
    {
        var isExist = await _sysUserRep.ExistAsync(u => u.Account == input.Account);
        if (isExist) throw FriendlyError.Ex(EnumErrorCode.D1003);

        var password = await ConfigService.Instance.GetConfigValueAsync(CommonConst.SysPasswordCode);
        var salt = await ConfigService.Instance.GetConfigValueAsync(CommonConst.SysPasswordSaltCode);

        var user = input.Adapt<DbUser>();
        user.Password = PasswordUtil.Encrypt(password, salt);
        var newUser = await _sysUserRep.AsInsertable(user).ExecuteReturnEntityAsync();
        input.Id = newUser.Id;
        await UpdateRoleAndExtOrg(input);

        var userInfo = new DbUserInfo();
        userInfo.TenantId = newUser.TenantId;
        userInfo.UserId = newUser.Id;
        userInfo.AuditStatus = input.AuditStatus;
        userInfo.CardType = input.CardType;
        userInfo.City = input.City;
        userInfo.College = input.College;
        userInfo.CultureLevel = input.CultureLevel;
        userInfo.EmergencyAddress = input.EmergencyAddress;
        userInfo.EmergencyPhone = input.EmergencyPhone;
        userInfo.EmergencyContact = input.EmergencyContact;
        userInfo.Expertise = input.Expertise;
        userInfo.IdCardNum = input.IdCardNum;
        userInfo.Introduction = input.Introduction;
        userInfo.JobNum = input.JobNum;
        userInfo.JoinDate = input.JoinDate;
        userInfo.Office = input.Office;
        userInfo.OfficeZone = input.OfficeZone;
        userInfo.OfficePhone = input.OfficePhone;
        userInfo.Province = input.Province;
        userInfo.PoliticalOutlook = input.PoliticalOutlook;
        userInfo.PosLevel = input.PosLevel;
        userInfo.Nation = input.Nation;
        await _sysUserRep.Change<DbUserInfo>().InsertAsync(userInfo);

        return newUser.Id;
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateUserAsync(UpdateUserInput input)
    {
        if (await _sysUserRep.ExistAsync(u => u.Account == input.Account && u.Id != input.Id))
            throw FriendlyError.Ex(EnumErrorCode.D1003);

        //当前逻辑中不允许改此值
        input.IsDelete = false;

        await _sysUserRep.AsUpdateable(input.Adapt<DbUser>()).IgnoreColumns(true)
            .IgnoreColumns(u => new { u.Password, u.Status }).ExecuteCommandAsync();

        await UpdateRoleAndExtOrg(input);

        var userInfo = await _busUserInfoRep.AsQueryable().FirstAsync(q => q.UserId == input.Id);
        if (userInfo == null)
        {
            var user = await _sysUserRep.AsQueryable().FirstAsync(q => q.Id == input.Id);
            userInfo = new DbUserInfo();
            userInfo.TenantId = user.TenantId;
            userInfo.UserId = user.Id;
        }
        userInfo.AuditStatus = input.AuditStatus;
        userInfo.CardType = input.CardType;
        userInfo.City = input.City;
        userInfo.College = input.College;
        userInfo.CultureLevel = input.CultureLevel;
        userInfo.EmergencyAddress = input.EmergencyAddress;
        userInfo.EmergencyPhone = input.EmergencyPhone;
        userInfo.EmergencyContact = input.EmergencyContact;
        userInfo.Expertise = input.Expertise;
        userInfo.IdCardNum = input.IdCardNum;
        userInfo.Introduction = input.Introduction;
        userInfo.JobNum = input.JobNum;
        userInfo.JoinDate = input.JoinDate;
        userInfo.Office = input.Office;
        userInfo.OfficeZone = input.OfficeZone;
        userInfo.OfficePhone = input.OfficePhone;
        userInfo.Province = input.Province;
        userInfo.PoliticalOutlook = input.PoliticalOutlook;
        userInfo.PosLevel = input.PosLevel;
        userInfo.Nation = input.Nation;

        await _sysUserRep.Change<DbUserInfo>().InsertOrUpdateAsync(userInfo);

        // 删除用户机构缓存
        return DataScopePermissionFilter.DeleteUserOrgCache(input.Id, _sysUserRep.Context.CurrentConnectionConfig.ConfigId);
    }


    /// <summary>
    /// 更新角色和扩展机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleAndExtOrg(AddUserInput input)
    {
        await GrantRoleAsync(new UserRoleInput { UserId = input.Id, RoleIdList = input.RoleIdList });

        await _sysUserExtOrgService.UpdateUserExtOrgAsync(input.Id, input.ExtOrgIdList);
    }

    /// <summary>
    /// 删除用户,
    /// 不能删除超级管理员，
    /// 不能删除自已
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserAsync(DeleteUserInput input, long userID)
    {
        var user = await _sysUserRep.FirstAsync(u => u.Id == input.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D0009);
        if (user.Id == userID)
            throw FriendlyError.Ex(EnumErrorCode.D1001);
        if (userID == LuBanOrmConst.SuperAdminId)
        {
            throw FriendlyError.Ex(EnumErrorCode.D1014);
        }

        await _sysUserRep.DeleteAsync(user);

        // 删除用户角色
        await UserRoleService.Instance.DeleteUserRoleByUserIdAsync(input.Id);

        // 删除用户扩展机构
        await _sysUserExtOrgService.DeleteUserExtOrgByUserIdAsync(input.Id);

        // 删除用户扩展信息
        return await _sysUserRep.Change<DbUserInfo>().DeleteAsync(u => u.UserId == input.Id);
    }


    /// <summary>
    /// 查看用户基本信息
    /// </summary>
    /// <returns></returns>
    public async Task<DbUser> GetBaseInfoAsync(long userID)
    {
        return await _sysUserRep.FirstAsync(u => u.Id == userID);
    }

    /// <summary>
    /// 更新用户基本信息
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateBaseInfoAsync(DbUser user)
    {
        user.IsDelete = false;
        return await _sysUserRep.AsUpdateable(user)
                            .IgnoreColumns(u => new { u.CreateTime, u.Account, u.Password, u.OrgId, u.PosId }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置用户状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<int> SetStatusAsync(UserInput input)
    {
        var user = await _sysUserRep.FirstAsync(u => u.Id == input.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D0009);
        if (user.Id == LuBanOrmConst.SuperAdminId)
            throw FriendlyError.Ex(EnumErrorCode.D1015);

        if (!Enum.IsDefined(typeof(EnumEnableStatus), input.Status))
            throw FriendlyError.Ex(EnumErrorCode.D3005);

        user.Status = input.Status;
        return await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.Status }).ExecuteCommandAsync();
    }


    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantRoleAsync(UserRoleInput input)
    {
        var user = await _sysUserRep.FirstAsync(u => u.Id == input.UserId) ?? throw FriendlyError.Ex(EnumErrorCode.D0009);
        if (user.Id == LuBanOrmConst.SuperAdminId)
            throw FriendlyError.Ex(EnumErrorCode.D1022);

        return await UserRoleService.Instance.GrantUserRoleAsync(input);
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<int> ChangePwdAsync(ChangePwdInput input, long userID)
    {
        var user = await _sysUserRep.FirstAsync(u => u.Id == userID) ?? throw FriendlyError.Ex(EnumErrorCode.D0009);
        var salt = await ConfigService.Instance.GetConfigValueAsync(CommonConst.SysPasswordSaltCode);
        user.Password = PasswordUtil.Encrypt(input.PasswordNew, salt);
        return await _sysUserRep.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<string?> ResetPwdAsync(ResetPwdUserInput input)
    {
        var user = await _sysUserRep.FirstAsync(u => u.Id == input.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D0009);
        var password = await ConfigService.Instance.GetConfigValueAsync(CommonConst.SysPasswordCode);
        var salt = await ConfigService.Instance.GetConfigValueAsync(CommonConst.SysPasswordSaltCode);
        user.Password = PasswordUtil.Encrypt(password, salt);
        await _sysUserRep.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
        return password;
    }

    /// <summary>
    /// 获取用户扩展机构集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<DbUserExtOrg>> GetOwnExtOrgListAsync(long userId)
    {
        return await _sysUserExtOrgService.GetUserExtOrgListAsync(userId);
    }

}
