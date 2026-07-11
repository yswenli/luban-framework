using Models.Dto;
using Models.Entities;

using Services.ApiServices;

namespace Controllers.Admin;

/// <summary>
/// Banner业务控制器
/// </summary>

[AllowAccess]
public class BannerController : BaseAdminController
{
    /// <summary>
    /// 后台-获取列表
    /// </summary>
    [DisplayName("列表"), HttpPost]
    public async Task<PagedList<DbBanner>> GetPagedListAsync([FromBody, Required] BannerInput input)
    {
        return await BannerService.Instance.GetPagedListAsync(input);
    }


    /// <summary>
    /// 后台-查看信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("查看信息"), HttpPost]
    public async Task<DbBanner> GetInfoAsync([Required, Range(1, long.MaxValue, ErrorMessage = "请输入正确的id")] long id)
    {
        return await BannerService.Instance.GetInfoAsync(id);
    }

    /// <summary>
    /// 后台-新增信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [DisplayName("新增信息"), HttpPost]
    public async Task<long> AddInfoAsync([FromBody, Required] BannerRequest request)
    {
        return await BannerService.Instance.AddInfoAsync(request);
    }

    /// <summary>
    /// 后台-更新信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("更新信息"), HttpPost]
    public async Task<bool> UpdateInfoAsync([FromBody, Required] BannerRequest request)
    {
        return await BannerService.Instance.UpdateInfoAsync(request);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("删除信息"), HttpPost]
    public async Task<bool> DeleteInfoAsync([Required, Range(1, long.MaxValue, ErrorMessage = "请输入正确的id")] long id)
    {
        return await BannerService.Instance.DeleteInfoAsync(id);
    }

}
