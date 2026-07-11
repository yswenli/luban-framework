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
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

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
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Services.ApiServices;

/// <summary>
/// 系统字典值服务
/// </summary>
public class SysDictDataService : BaseService<SysDictDataService>
{
    private readonly DbRepository<DbDictData> _sysDictDataRep;
    private readonly DbRepository<DbDictType> _sysDictTypeRep;

    /// <summary>
    /// 系统字典值服务
    /// </summary>
    public SysDictDataService()
    {
        _sysDictDataRep = new DbRepository<DbDictData>();
        _sysDictTypeRep = new DbRepository<DbDictType>();
    }


    /// <summary>
    /// 获取字典值分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> Page(PageDictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictDataRep.AsQueryable()
           .Where(u => u.DictTypeId == input.DictTypeId)
           .WhereIF(!string.IsNullOrEmpty(input.Code?.Trim()), u => u.Code.Contains(input.Code!))
           .WhereIF(!string.IsNullOrEmpty(input.Value?.Trim()), u => u.Value.Contains(input.Value!))
           //.OrderBy(u => new { u.OrderNo, u.Code })
           //.ToPagedListAsync(input.Page, input.PageSize);
           .ToPagedListAsync(input);
        });
    }



    /// <summary>
    /// 增加字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> AddDictData(AddDictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            var isExist = await _sysDictDataRep.IsAnyAsync(u => u.Code == input.Code && u.DictTypeId == input.DictTypeId);
            if (isExist)
                throw FriendlyError.Ex(EnumErrorCode.D3003);


            return await _sysDictDataRep.InsertAsync(input.Adapt<DbDictData>());
        });
    }


    /// <summary>
    /// 更新字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> UpdateDictData(UpdateDictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            var isExist = await _sysDictDataRep.IsAnyAsync(u => u.Id == input.Id);
            if (!isExist) throw FriendlyError.Ex(EnumErrorCode.D3004);

            isExist = await _sysDictDataRep.IsAnyAsync(u => u.Code == input.Code && u.DictTypeId == input.DictTypeId && u.Id != input.Id);
            if (isExist) throw FriendlyError.Ex(EnumErrorCode.D3003);

            return await _sysDictDataRep.UpdateAsync(input.Adapt<DbDictData>());
        });
    }



    /// <summary>
    /// 删除字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> DeleteDictData(DeleteDictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            var dictData = await _sysDictDataRep.FirstAsync(u => u.Id == input.Id);
            if (dictData == null)
                throw FriendlyError.Ex(EnumErrorCode.D3004);

            return await _sysDictDataRep.DeleteAsync(dictData);
        });
    }



    /// <summary>
    /// 获取字典值详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> GetDetail(DictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictDataRep.FirstAsync(u => u.Id == input.Id);
        });
    }



    /// <summary>
    /// 修改字典值状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> SetStatus(DictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            var dictData = await _sysDictDataRep.FirstAsync(u => u.Id == input.Id);
            if (dictData == null)
                throw FriendlyError.Ex(EnumErrorCode.D3004);

            if (!Enum.IsDefined(typeof(EnumEnableStatus), input.Status))
                throw FriendlyError.Ex(EnumErrorCode.D3005);

            dictData.Status = input.Status;
            return await _sysDictDataRep.UpdateAsync(dictData);
        });
    }


    /// <summary>
    /// 根据字典类型Id获取字典值集合
    /// </summary>
    /// <param name="dictTypeId"></param>
    /// <returns></returns>
    public async Task<List<DbDictData>> GetDictDataListByDictTypeId(long dictTypeId)
    {
        return await _sysDictDataRep.AsQueryable()
             .Where(u => u.DictTypeId == dictTypeId)
             .OrderBy(u => new { u.OrderNo, u.Code })
             .ToListAsync();
    }

    /// <summary>
    /// 根据字典类型Id获取字典值集合
    /// </summary>
    /// <param name="dictTypeId"></param>
    /// <returns></returns>
    public async Task<Result> GetDictDataListByDictTypeIdAsync(long dictTypeId)
    {
        return await GetResultAsync(async () => await GetDictDataListByDictTypeId(dictTypeId));
    }



    /// <summary>
    /// 根据字典类型Id删除字典值
    /// </summary>
    /// <param name="dictTypeId"></param>
    /// <returns></returns>
    public async Task<Result> DeleteDictData(long dictTypeId)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictDataRep.DeleteAsync(u => u.DictTypeId == dictTypeId);
        });
    }




    /// <summary>
    /// 根据字典类型编码获取字典值集合
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<Result> GetDataList(string code)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictDataRep.Context.Queryable<DbDictType>()
            .LeftJoin<DbDictData>((u, a) => u.Id == a.DictTypeId)
            .Where((u, a) => u.Code == code && u.Status == EnumEnableStatus.Enable && a.Status == EnumEnableStatus.Enable)
            .OrderBy((u, a) => new { a.OrderNo, a.Code })
            .Select((u, a) => a).ToListAsync();
        });
    }


    /// <summary>
    /// 根据查询条件获取字典值集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Result> GetDataList(QueryDictDataInput input)
    {
        return await GetResultAsync(async () =>
        {
            return await _sysDictTypeRep
            .LeftJoin<DbDictData>((u, a) => u.Id == a.DictTypeId)
            .Where((u, a) => u.Code == input.Code)
            .WhereIF(input.Status.HasValue, (u, a) => a.Status == (EnumEnableStatus)input.Status!.Value)
            .OrderBy((u, a) => new { a.OrderNo, a.Code })
            .Select((u, a) => a).ToListAsync();
        });
    }




}

