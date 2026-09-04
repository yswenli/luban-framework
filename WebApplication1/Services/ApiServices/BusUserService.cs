/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.ApiServices
*文件名： BusUserService.cs
*版本号： V1.0.0.0
*唯一标识：b79c9a30-363c-4b08-a208-dc5e0f919c40
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：BusUserService 服务类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：BusUserService 服务类
*
*****************************************************************************/

using LuBan.Common.Errors;

namespace WebApplication1.Services.ApiServices;

public class BusUserService : BaseService<BusUserService>
{
    private DbRepository<DbUser> _busUserDbRes => new();

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public async Task<Result> GetListAsync()
    {
        return await GetResultAsync(async () =>
        {
            return await _busUserDbRes.ListAsync(q => q.IsDelete == false);
        });
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    public async Task<Result> Add(DbUser busUser)
    {
        return await GetResultAsync(async () =>
        {
            var user = await _busUserDbRes.FirstAsync(q => q.RealName == busUser.RealName);
            if (user != null) throw FriendlyError.Ex(FrameworkErrors.User.AccountExists);
            return await _busUserDbRes.InsertAsync(busUser);
        });
    }
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    public async Task<Result> Update(DbUser busUser)
    {
        return await GetResultAsync(async () =>
        {
            if (busUser.RealName == "yswenli")
            {
                return await _busUserDbRes.UpdateAsync(q => new DbUser { RealName = "WALLE" }, q => q.RealName == "yswenli");
            }
            else
            {
                return await _busUserDbRes.UpdateAsync(busUser);
            }
        });
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    public async Task<Result> Delete(DbUser busUser)
    {
        return await GetResultAsync(async () =>
        {
            //return await _busUserRes.LogicDeleteAsync(busUser);
            return await new DbRepository<DbUser>().DeleteAsync(busUser);
        });
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result> DeleteById(long id)
    {
        return await GetResultAsync(async () =>
        {
            return await _busUserDbRes.LogicDeleteAsync(q => q.Id == id);
        });
    }

    /// <summary>
    /// 测试事务
    /// </summary>
    /// <returns></returns>
    public async Task<Result> TranTest()
    {
        //可使用UnitOfWorkAttribute代替
        return await GetResultAsync(async () =>
        {
            using var tran = _busUserDbRes.CreateTran();
            var entity = new DbUser()
            {
                RealName = "wenli"
            };
            await _busUserDbRes.InsertAsync(entity);
            entity = await _busUserDbRes.FirstAsync(q => q.IsDelete == false);
            entity.RealName = "yswenli";
            await _busUserDbRes.UpdateAsync(entity);
            var result = await _busUserDbRes.LogicDeleteAsync(q => q.Id == entity.Id);
            tran.Commit();
            return result;
        });
    }
    /// <summary>
    /// 测试用户
    /// </summary>
    /// <returns></returns>
    public async Task<DbUser?> TestSessionUserAsync()
    {
        return await Task.FromResult(SessionUser.CurrentUser);
    }
}
