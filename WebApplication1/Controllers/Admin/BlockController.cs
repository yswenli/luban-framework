/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：Controllers.Admin
*文件名： BlockController.cs
*版本号： V1.0.0.0
*唯一标识：c67681b0-ed46-4bd3-ae1e-c336547d909f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：BlockController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：BlockController 控制器
*
*****************************************************************************/

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
