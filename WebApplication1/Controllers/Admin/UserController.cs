/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Admin
*文件名： UserController.cs
*版本号： V1.0.0.0
*唯一标识：461691bf-261b-4955-a52b-a82191e56ade
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：UserController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UserController 控制器
*
*****************************************************************************/

using LuBan.Orm.Attributes;

using Services.ApiServices;

using WebApplication1.Models.Vos;
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
