/****************************************************************************
*Copyright (c) 2024 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： UserInfoService
*版本号： V1.0.0.0
*唯一标识：b5d69ece-fcf5-4f15-8697-f3fdada59a1d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/10/8 17:11:07
*描述：用户扩展信息业务类
*
*=================================================
*修改标记
*修改时间：2024/10/8 17:11:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：用户扩展信息业务类
*
*****************************************************************************/
using WebApplication1.Models.Entities;

namespace Services.ApiServices;

/// <summary>
/// 用户扩展信息业务类
/// </summary>
public class UserInfoService : BaseService<UserInfoService>
{
    private readonly DbRepository<DbUserInfo> _busUserRep;

    /// <summary>
    /// 用户扩展信息业务类
    /// </summary>
    public UserInfoService()
    {
        _busUserRep = new DbRepository<DbUserInfo>();
    }

    /// <summary>
    /// 根据用户id获取用户扩展信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DbUserInfo> GetUserInfoByUserId(long userId)
    {
        return await _busUserRep.FirstAsync(x => x.UserId == userId);
    }

    /// <summary>
    /// 更新用户扩展信息
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<bool> UpdateUserInfo(DbUserInfo data)
    {
        var old = await _busUserRep.FirstAsync(u => u.IsDelete == false && u.Id == data.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D1001);
        old.FillFromEntity(data);
        return await _busUserRep.UpdateAsync(data);
    }
    /// <summary>
    /// 添加用户扩展信息
    /// </summary>
    /// <param name="busUserInfo"></param>
    /// <returns></returns>
    public async Task<long> AddUserInfo(DbUserInfo busUserInfo)
    {
        return await _busUserRep.InsertReturnSnowflakeIdAsync(busUserInfo);
    }

    /// <summary>
    /// 逻辑删除用户扩展信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserInfo(long userId)
    {
        return await _busUserRep.LogicDeleteAsync(x => x.UserId == userId);
    }
}
