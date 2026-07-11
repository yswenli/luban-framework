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
/// 系统字典值服务
/// </summary>
[AllowAnonymous, AllowAccess]
public class DictDataController : BaseAdminController
{

    /// <summary>
    /// 获取字典值分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取字典值分页列表"), HttpPost]
    public async Task<Result> Page([FromBody] PageDictDataInput input)
    {
        return await SysDictDataService.Instance.Page(input);
    }



    /// <summary>
    /// 获取字典值列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取字典值列表"), HttpGet]
    public async Task<Result> List([FromQuery] GetDataDictDataInput input)
    {
        return await SysDictDataService.Instance.GetDictDataListByDictTypeIdAsync(input.DictTypeId);
    }


    /// <summary>
    /// 增加字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Add")]
    [DisplayName("增加字典值")]
    public async Task<Result> AddDictData([FromBody] AddDictDataInput input)
    {
        return await SysDictDataService.Instance.AddDictData(input);
    }


    /// <summary>
    /// 更新字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Update")]
    [DisplayName("更新字典值")]
    public async Task<Result> UpdateDictData([FromBody] UpdateDictDataInput input)
    {
        return await SysDictDataService.Instance.UpdateDictData(input);
    }



    /// <summary>
    /// 删除字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/api/[controller]/Delete")]
    [DisplayName("删除字典值")]
    public async Task<Result> DeleteDictData([FromBody] DeleteDictDataInput input)
    {
        return await SysDictDataService.Instance.DeleteDictData(input);
    }


    /// <summary>
    /// 获取字典值详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取字典值详情"), HttpGet]
    public async Task<Result> Detail([FromQuery] DictDataInput input)
    {
        return await SysDictDataService.Instance.GetDetail(input);
    }



    /// <summary>
    /// 修改字典值状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改字典值状态"), HttpPost]
    public async Task<Result> SetStatus([FromBody] DictDataInput input)
    {
        return await SysDictDataService.Instance.SetStatus(input);
    }


    /// <summary>
    /// 根据字典类型编码获取字典值集合
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [DisplayName("根据字典类型编码获取字典值集合"), HttpGet("/api/[controller]/DataList/{code}")]
    public async Task<Result> DataList([Required] string code)
    {
        return await SysDictDataService.Instance.GetDataList(code);
    }


    /// <summary>
    /// 根据查询条件获取字典值集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据查询条件获取字典值集合"), HttpGet]
    public async Task<Result> DataList([FromQuery] QueryDictDataInput input)
    {
        return await SysDictDataService.Instance.GetDataList(input);
    }







}