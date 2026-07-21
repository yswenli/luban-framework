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
            if (user != null) throw FriendlyError.Ex(EnumErrorCode.D1003);
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
            return await _busUserDbRes.LogicDeleteAsync(q => q.Id == entity.Id);
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
