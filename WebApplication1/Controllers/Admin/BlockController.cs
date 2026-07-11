using Services.ApiServices;

using WebApplication1.Models.Entities;
using WebApplication1.Models.Vos;

namespace Controllers.Admin;

/// <summary>
/// 内容业务控制器
/// </summary>

public class BlockController : BaseAdminController
{

    /// <summary>
    /// 后台-栏目列表  移动端加isdelete=true参数来过滤
    /// </summary>
    [DisplayName("栏目列表"), HttpPost]
    public async Task<PagedList<DbBlock>> GetPagedListAsync([FromBody, Required] BlockPagedInput input)
    {
        return await BlockService.Instance.GetPagedListAsync(input);
    }

    /// <summary>
    /// 后台-查看栏目信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("查看栏目信息"), HttpPost]
    public async Task<DbBlock> GetInfoAsync([Required] long id)
    {
        return await BlockService.Instance.GetInfoAsync(id);
    }

    /// <summary>
    /// 后台-增加栏目信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [DisplayName("增加栏目信息"), HttpPost]
    public async Task<long> AddInfoAsync([FromBody, Required] BlockInfo request)
    {
        return await BlockService.Instance.AddInfoAsync(request);
    }

    /// <summary>
    /// 后台-更新信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("更新信息"), HttpPost]
    public async Task<bool> UpdateInfoAsync([FromBody, Required] BlockInfo request)
    {
        return await BlockService.Instance.UpdateInfoAsync(request);
    }

}
