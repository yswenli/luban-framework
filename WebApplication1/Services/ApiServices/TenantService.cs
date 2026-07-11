/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： SysTenantService
*版本号： V1.0.0.0
*唯一标识：88cca5b6-26f4-454b-aea8-0ba1b6c4b111
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 10:56:38
*描述：系统租户管理服务
*
*=================================================
*修改标记
*修改时间：2023/12/7 10:56:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统租户管理服务
*
*****************************************************************************/

namespace Services.ApiServices;

/// <summary>
/// 系统租户管理服务
/// </summary>
public class TenantService : BaseService<TenantService>
{
    private readonly DbRepository<DbTenant> _sysTenantRep;
    private readonly DbRepository<DbOrg> _sysOrgRep;
    private readonly DbRepository<DbRole> _sysRoleRep;
    private readonly DbRepository<DbPos> _sysPosRep;
    private readonly BaseRepository<DbUser> _sysUserRep;
    private readonly DbRepository<DbUserExtOrg> _sysUserExtOrgRep;
    private readonly DbRepository<DbRoleMenu> _sysRoleMenuRep;
    private readonly BaseRepository<DbUserRole> _userRoleRep;
    private readonly ConfigService _sysConfigService;

    public TenantService()
    {
        _sysTenantRep = new DbRepository<DbTenant>();
        _sysOrgRep = new DbRepository<DbOrg>();
        _sysRoleRep = new DbRepository<DbRole>();
        _sysPosRep = new DbRepository<DbPos>();
        _sysUserRep = new DbRepository<DbUser>();
        _sysUserExtOrgRep = new DbRepository<DbUserExtOrg>();
        _sysRoleMenuRep = new DbRepository<DbRoleMenu>();
        _userRoleRep = new BaseRepository<DbUserRole>();
        _sysConfigService = ConfigService.Instance;
    }


    /// <summary>
    /// 获取租户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<TenantOutput>> PageAsync(PageTenantInput input)
    {
        return await _sysTenantRep
             .LeftJoin<DbUser>((u, a) => u.UserId == a.Id)
             .LeftJoin<DbOrg>((u, a, b) => u.OrgId == b.Id)
             .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), (u, a) => a.Phone != null && a.Phone.Contains(input.Phone.Trim()))
             .WhereIF(!string.IsNullOrWhiteSpace(input.Name), (u, a, b) => b.Name.Contains(input.Name.Trim()))
             .OrderBy(u => u.OrderNo)
             .Select((u, a, b) => new TenantOutput
             {
                 Id = u.Id,
                 OrgId = b.Id,
                 Name = b.Name,
                 UserId = a.Id,
                 AdminAccount = a.Account,
                 Phone = a.Phone ?? "",
                 Email = a.Email ?? "",
                 TenantType = u.TenantType,
                 DbType = u.DbType,
                 Connection = u.Connection,
                 ConfigId = u.ConfigId,
                 OrderNo = u.OrderNo,
                 Remark = u.Remark,
                 Status = u.Status,
             })
             .ToPagedListAsync(input);
    }

    /// <summary>
    /// 增加租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> AddTenantAsync(AddTenantInput input)
    {
        var isExist = await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name);
        if (isExist) throw FriendlyError.Ex(EnumErrorCode.D1300);

        isExist = await _sysUserRep.ExistAsync(u => u.Account == input.AdminAccount);
        if (isExist) throw FriendlyError.Ex(EnumErrorCode.D1301);

        // ID隔离时设置与主库一致
        if (input.TenantType == EnumTenantType.Id)
        {
            var config = _sysTenantRep.AsSugarClient().CurrentConnectionConfig;
            input.DbType = config.DbType;
            input.Connection = config.ConnectionString;
        }

        var tenant = input.Adapt<TenantOutput>();
        await _sysTenantRep.InsertAsync(tenant);
        await InitNewTenant(tenant);

        return await CacheTenant();
    }

    /// <summary>
    /// 设置租户状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<int> SetStatusAsync(TenantInput input)
    {
        var tenant = await _sysTenantRep.FirstAsync(u => u.Id == input.Id);
        if (tenant == null || tenant.ConfigId == LuBanOrmConst.MainConfigId)
            throw FriendlyError.Ex(EnumErrorCode.Z1001);

        if (!Enum.IsDefined(typeof(EnumEnableStatus), input.Status))
            throw FriendlyError.Ex(EnumErrorCode.D3005);

        tenant.Status = input.Status;
        return await _sysTenantRep.AsUpdateable(tenant).UpdateColumns(u => new { u.Status }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 新增租户初始化
    /// </summary>
    /// <param name="tenant"></param>
    private async Task InitNewTenant(TenantOutput tenant)
    {
        var tenantId = tenant.Id;
        var tenantName = tenant.Name;

        // 初始化机构
        var newOrg = new DbOrg
        {
            TenantId = tenantId,
            Pid = 0,
            Name = tenantName,
            Code = tenantName,
            Remark = tenantName,
        };
        await _sysOrgRep.InsertAsync(newOrg);

        // 初始化角色
        var newRole = new DbRole
        {
            TenantId = tenantId,
            Name = "租管-" + tenantName,
            Code = CommonConst.SysAdminRole,
            DataScope = EnumDataScope.All,
            Remark = tenantName
        };
        await _sysRoleRep.InsertAsync(newRole);

        // 初始化职位
        var newPos = new DbPos
        {
            TenantId = tenantId,
            Name = "租管-" + tenantName,
            Code = tenantName,
            Remark = tenantName,
        };
        await _sysPosRep.InsertAsync(newPos);

        // 初始化系统账号
        var password = await _sysConfigService.GetConfigValueAsync(CommonConst.SysPasswordCode);
        var salt = await _sysConfigService.GetConfigValueAsync(CommonConst.SysPasswordSaltCode);
        var newUser = new DbUser
        {
            TenantId = tenantId,
            Account = tenant.AdminAccount,
            Password = PasswordUtil.Encrypt(password, salt),
            NickName = "租管",
            Email = tenant.Email,
            Phone = tenant.Phone,
            OrgId = newOrg.Id,
            PosId = newPos.Id,
            Birthday = DateTime.Parse("1985-12-22"),
            RealName = "租管",
            Remark = "租管" + tenantName,
        };
        await _sysUserRep.InsertAsync(newUser);

        // 关联用户及角色
        var newUserRole = new DbUserRole
        {
            RoleId = newRole.Id,
            UserId = newUser.Id
        };
        await _userRoleRep.InsertAsync(newUserRole);

        // 关联租户组织机构和管理员用户
        await _sysTenantRep.UpdateAsync(u => new DbTenant() { UserId = newUser.Id, OrgId = newOrg.Id }, u => u.Id == tenantId);

        // 默认租户管理员角色菜单集合
        var menuIdList = new List<long> { 1300000000111,1300000000121, // 工作台
        1310000000111,1310000000112,1310000000113,1310000000114,1310000000115,1310000000116,1310000000117,1310000000118,1310000000119,1310000000120, // 账号
        1310000000131,1310000000132,1310000000133,1310000000134,1310000000135,1310000000136,1310000000137,1310000000138, // 角色
        1310000000141,1310000000142,1310000000143,1310000000144,1310000000145, // 机构
        1310000000151,1310000000152,1310000000153,1310000000154,1310000000155, // 职位
        1310000000161,1310000000162,1310000000163,1310000000164, // 个人中心
        1310000000171,1310000000172,1310000000173,1310000000174,1310000000175,1310000000176,1310000000177 // 通知公告
    };
        await RoleMenuService.Instance.GrantRoleMenuAsync(new RoleMenuInput() { Id = newRole.Id, MenuIdList = menuIdList });
    }

    /// <summary>
    /// 删除租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteTenantAsync(DeleteTenantInput input)
    {
        // 禁止删除默认租户
        if (input.Id.ToString() == LuBanOrmConst.MainConfigId)
            throw FriendlyError.Ex(EnumErrorCode.D1023);

        await _sysTenantRep.DeleteAsync(u => u.Id == input.Id);

        await CacheTenant(input.Id);

        // 删除与租户相关的表数据
        var users = await _sysUserRep.Where(u => u.TenantId == input.Id).ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();
        await _sysUserRep.AsDeleteable().Where(u => userIds.Contains(u.Id)).ExecuteCommandAsync();

        await _userRoleRep.AsDeleteable().Where(u => userIds.Contains(u.UserId)).ExecuteCommandAsync();

        await _sysUserExtOrgRep.AsDeleteable().Where(u => userIds.Contains(u.UserId)).ExecuteCommandAsync();

        await _sysRoleRep.AsDeleteable().Where(u => u.TenantId == input.Id).ExecuteCommandAsync();

        var roleIds = await _sysRoleRep.Where(u => u.TenantId == input.Id).Select(u => u.Id).ToListAsync();
        await _sysRoleMenuRep.DeleteAsync(u => roleIds.Contains(u.RoleId));

        await _sysOrgRep.DeleteAsync(u => u.TenantId == input.Id);

        await _sysPosRep.DeleteAsync(u => u.TenantId == input.Id);

        return true;

    }

    /// <summary>
    /// 更新租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTenantAsync(UpdateTenantInput input)
    {
        var isExist = await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name && u.Id != input.OrgId);
        if (isExist)
            throw FriendlyError.Ex(EnumErrorCode.D1300);
        isExist = await _sysUserRep.IsAnyAsync(u => u.Account == input.AdminAccount && u.Id != input.UserId);
        if (isExist)
            throw FriendlyError.Ex(EnumErrorCode.D1301);

        await _sysTenantRep.AsUpdateable(input.Adapt<TenantOutput>()).IgnoreColumns(true).ExecuteCommandAsync();

        // 更新系统机构
        await _sysOrgRep.UpdateAsync(u => new DbOrg() { Name = input.Name }, u => u.Id == input.OrgId);

        // 更新系统用户
        await _sysUserRep.UpdateAsync(u => new DbUser() { Account = input.AdminAccount, Phone = input.Phone, Email = input.Email }, u => u.Id == input.UserId);

        return await CacheTenant(input.Id);
    }

    /// <summary>
    /// 授权租户管理员角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> GrantMenuAsync(RoleMenuInput input)
    {
        var tenantAdminUser = await _sysUserRep.FirstAsync(u => u.TenantId == input.Id && u.Id == LuBanOrmConst.SuperAdminId);
        if (tenantAdminUser == null) return false;

        var roleIds = await UserRoleService.Instance.GetUserRoleIdListAsync(tenantAdminUser.Id);
        input.Id = roleIds[0]; // 重置租户管理员角色Id
        return await RoleMenuService.Instance.GrantRoleMenuAsync(input);
    }

    /// <summary>
    /// 获取租户管理员角色拥有菜单Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<long>> GetOwnMenuListAsync(TenantUserInput input)
    {
        var roleIds = await UserRoleService.Instance.GetUserRoleIdListAsync(input.UserId);
        return await RoleMenuService.Instance.GetRoleMenuIdListAsync(new List<long> { roleIds[0] });
    }

    /// <summary>
    /// 重置租户管理员密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<string> ResetPwdAsync(TenantUserInput input)
    {
        var password = await _sysConfigService.GetConfigValueAsync(CommonConst.SysPasswordCode) ?? "";
        var salt = await _sysConfigService.GetConfigValueAsync(CommonConst.SysPasswordSaltCode);
        var encryptPassword = PasswordUtil.Encrypt(password, salt);
        await _sysUserRep.UpdateAsync(u => new DbUser() { Password = encryptPassword }, u => u.Id == input.UserId);
        return password;
    }

    /// <summary>
    /// 缓存所有租户
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public async Task<bool> CacheTenant(long tenantId = 0)
    {
        if (tenantId.ToString() != LuBanOrmConst.MainConfigId)
        {
            if (tenantId > 0)
                _sysTenantRep.AsTenant().RemoveConnection(tenantId);

            var tenantList = await _sysTenantRep.GetListAsync();
            CacheService.Instance.Set(CacheConst.KeyTenant, tenantList);
            return true;
        }
        return false;
    }


    /// <summary>
    /// 获取租户下的用户列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<DbUser>> UserListAsync(TenantIdInput input)
    {
        return await _sysUserRep.Where(u => u.TenantId == input.TenantId).ToListAsync();
    }
}
