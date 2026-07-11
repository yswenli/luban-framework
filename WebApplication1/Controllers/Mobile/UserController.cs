using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers.Mobile;

public class UserController : BaseMobileController
{
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    [HttpPost, Authorize]
    public async Task<Result> GetList()
    {
        return await BusUserService.Instance.GetListAsync();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result> Add([FromBody] DbUser busUser)
    {
        return await BusUserService.Instance.Add(busUser);
    }
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result> Update([FromBody] DbUser busUser)
    {
        return await BusUserService.Instance.Update(busUser);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="busUser"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result> Delete([FromBody] DbUser busUser)
    {
        return await BusUserService.Instance.Delete(busUser);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result> DeleteById([Required] long id)
    {
        return await BusUserService.Instance.DeleteById(id);
    }


    /// <summary>
    /// 测试事务
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result> TranTest()
    {
        return await BusUserService.Instance.TranTest();
    }


}
