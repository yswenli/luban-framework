using LuBan.Orm.Attributes;

using Services.ApiServices;

using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers.Admin;

public class UserController : BaseAdminController
{


    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取用户分页列表"), HttpPost]
    public async Task<PagedList<PageUserOutput>?> PageAsync([FromBody] PageUserInput input)
    {
        return await UserService.Instance.PageAsync(input);
    }

    /// <summary>
    /// 增加用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("增加用户"), HttpPost]
    public async Task<long> AddUserAsync([FromBody, Required] AddUserInput input)
    {
        return await UserService.Instance.AddUserAsync(input);
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("更新用户"), HttpPost]
    public async Task<bool> UpdateUserAsync([FromBody, Required] UpdateUserInput input)
    {
        return await UserService.Instance.UpdateUserAsync(input);
    }


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
    [UnitOfWork]
    public async Task<Result> TranTest()
    {
        return await BusUserService.Instance.TranTest();
    }



    /// <summary>
    /// 测试用户
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<DbUser?> TestSessionUser()
    {
        return await BusUserService.Instance.TestSessionUserAsync();
    }

}
