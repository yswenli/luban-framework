/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：CVE.Service
*文件名： SysAuthService
*版本号： V1.0.0.0
*唯一标识：04c5ac43-8f7b-4f31-b90f-2be3cea356be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 18:51:19
*描述：系统字典类型服务
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统字典类型服务
*
*****************************************************************************/

using WebApplication1.Models.Vos;

namespace WebApplication1.Services.ApiServices;

/// <summary>
/// 系统字典类型服务
/// </summary>
public class SysDictTypeService : BaseService<SysDictTypeService>
{
    private DbRepository<DbDictType> _sysDictTypeRep => new();
    private readonly SysDictDataService _sysDictDataService;

    /// <summary>
    /// 系统字典类型服务
    /// </summary>
    public SysDictTypeService()
    {
        _sysDictDataService = new SysDictDataService();
    }


    /// <summary>
    /// 获取字典类型分页列表
    /// </summary>
    /// <returns></returns>
    public async Task<Result> Page(PageDictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictTypeRep.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.Code?.Trim()), u => u.Code.Contains(input.Code ?? ""))
            .WhereIF(!string.IsNullOrEmpty(input.Name?.Trim()), u => u.Name.Contains(input.Name ?? ""))
            //.Includes(u => u.Children)
            .OrderBy(u => new { u.OrderNo, u.Code })
            .Select<dynamic>(u => new
            {
                Id = u.Id.SelectAll(),
                ChildrenCount = SqlFunc.Subqueryable<DbDictData>().Where(a => a.DictTypeId == u.Id).Count()
            })
            .ToPagedListAsync(input);
        });
    }

    /// <summary>
    /// 获取字典类型列表
    /// </summary>
    /// <returns></returns>
    public async Task<Result> GetList()
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictTypeRep.AsQueryable().OrderBy(u => new { u.OrderNo, u.Code }).ToListAsync();
        });
    }

    /// <summary>
    /// 获取字典类型-值列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> GetDataList(GetDataDictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            var dictType = await _sysDictTypeRep.FirstAsync(u => u.Code == input.Code);
            if (dictType == null)
                throw FriendlyError.Ex(EnumErrorCode.D3000);

            return await _sysDictDataService.GetDictDataListByDictTypeId(dictType.Id);

        });
    }

    /// <summary>
    /// 添加字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> AddDictType(AddDictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            var isExist = await _sysDictTypeRep.IsAnyAsync(u => u.Code == input.Code);
            if (isExist)
                throw FriendlyError.Ex(EnumErrorCode.D3001);

            return await _sysDictTypeRep.InsertAsync(input.Adapt<DbDictType>());
        });
    }



    /// <summary>
    /// 更新字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> UpdateDictType(UpdateDictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            var isExist = await _sysDictTypeRep.IsAnyAsync(u => u.Id == input.Id);
            if (!isExist)
                throw FriendlyError.Ex(EnumErrorCode.D3000);

            isExist = await _sysDictTypeRep.IsAnyAsync(u => u.Code == input.Code && u.Id != input.Id);
            if (isExist)
                throw FriendlyError.Ex(EnumErrorCode.D3001);

            return await _sysDictTypeRep.UpdateAsync(input.Adapt<DbDictType>());
        });
    }


    /// <summary>
    /// 删除字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> DeleteDictType(DeleteDictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            var dictType = await _sysDictTypeRep.FirstAsync(u => u.Id == input.Id);
            if (dictType == null)
                throw FriendlyError.Ex(EnumErrorCode.D3000);

            // 删除字典值
            var data = await _sysDictTypeRep.DeleteAsync(dictType);
            await _sysDictDataService.DeleteDictData(input.Id);
            return data;
        });
    }


    /// <summary>
    /// 获取字典类型详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> GetDetail(DictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictTypeRep.FirstAsync(u => u.Id == input.Id);
        });
    }


    /// <summary>
    /// 修改字典类型状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> SetStatus(DictTypeInput input)
    {
        return await GetResultAsync(async () =>
        {
            var dictType = await _sysDictTypeRep.FirstAsync(u => u.Id == input.Id);
            if (dictType == null)
                throw FriendlyError.Ex(EnumErrorCode.D3000);

            if (!Enum.IsDefined(typeof(EnumEnableStatus), input.Status))
                throw FriendlyError.Ex(EnumErrorCode.D3005);

            dictType.Status = input.Status;
            return await _sysDictTypeRep.UpdateAsync(dictType);
        });
    }



    /// <summary>
    /// 获取所有字典集合
    /// </summary>
    /// <returns></returns>
    public Result GetAllDictList()
    {
        return GetResult(() =>
        {
            var dictList = _sysDictTypeRep.AsQueryable()
            .Includes(u => u.Children)
            .OrderBy(u => new { u.OrderNo, u.Code })
            .ToList();
            // 字典数据项排序
            dictList.ForEach(u => u.Children = u.Children.OrderBy(c => c.OrderNo).ThenBy(c => c.Code).ToList());
            return dictList;
        });
    }

    /// <summary>
    /// 获取所有字典集合
    /// </summary>
    /// <returns></returns>
    public async Task<Result> GetAllDictListAsync()
    {
        return await GetResultAsync(async () =>
        {
            var dictList = await _sysDictTypeRep.AsQueryable()
            .Includes(u => u.Children)
            .OrderBy(u => new { u.OrderNo, u.Code })
            .ToListAsync();
            // 字典数据项排序
            dictList.ForEach(u => u.Children = u.Children.OrderBy(c => c.OrderNo).ThenBy(c => c.Code).ToList());
            return dictList;
        });
    }


}
