/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers.Admin;

/// <summary>
/// 系统字典类型服务
/// </summary>
[AllowAnonymous, AllowAccess]
public class DictTypeController : BaseAdminController
{

    /// <summary>
    /// 获取字典类型分页列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取字典类型分页列表"), HttpPost]
    public async Task<Result> PageAsync([FromBody] PageDictTypeInput input)
    {
        return await SysDictTypeService.Instance.Page(input);
    }



    /// <summary>
    /// 获取字典类型列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取字典类型列表"), HttpGet]
    public async Task<Result> ListAsync()
    {
        return await SysDictTypeService.Instance.GetList();
    }



    /// <summary>
    /// 获取字典类型-值列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取字典类型-值列表"), HttpGet]
    public async Task<Result> DataListAsync([FromQuery] GetDataDictTypeInput input)
    {
        return await SysDictTypeService.Instance.GetDataList(input);
    }



    /// <summary>
    /// 添加字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Add")]
    [DisplayName("添加字典类型")]
    public async Task<Result> AddDictTypeAsync([FromBody] AddDictTypeInput input)
    {
        return await SysDictTypeService.Instance.AddDictType(input);
    }




    /// <summary>
    /// 更新字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Update")]
    [DisplayName("更新字典类型")]
    public async Task<Result> UpdateDictTypeAsync([FromBody] UpdateDictTypeInput input)
    {
        return await SysDictTypeService.Instance.UpdateDictType(input);
    }


    /// <summary>
    /// 删除字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Delete")]
    [DisplayName("删除字典类型")]
    public async Task<Result> DeleteDictTypeAsync([FromBody] DeleteDictTypeInput input)
    {
        return await SysDictTypeService.Instance.DeleteDictType(input);
    }




    /// <summary>
    /// 获取字典类型详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取字典类型详情"), HttpGet]
    public async Task<Result> DetailAsync([FromQuery] DictTypeInput input)
    {
        return await SysDictTypeService.Instance.GetDetail(input);
    }



    /// <summary>
    /// 修改字典类型状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改字典类型状态"), HttpPost]
    public async Task<Result> SetStatusAsync([FromBody] DictTypeInput input)
    {
        return await SysDictTypeService.Instance.SetStatus(input);
    }



    /// <summary>
    /// 获取所有字典集合
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取所有字典集合"), HttpGet]
    public async Task<Result> AllDictListAsync()
    {
        return await SysDictTypeService.Instance.GetAllDictListAsync();
    }



}