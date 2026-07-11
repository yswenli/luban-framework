/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： BannerService
*版本号： V1.0.0.0
*唯一标识：04c5ac43-8f7b-4f31-b90f-2be3cea356be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 18:51:19
*描述：banner业务
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：banner业务
*
*****************************************************************************/
using Models.Dto;
using Models.Entities;

namespace Services.ApiServices;

/// <summary>
/// banner业务
/// </summary>
public class BannerService : BaseService<BannerService>
{
    private readonly DbRepository<DbBanner> _butBannerRep;

    /// <summary>
    /// banner业务
    /// </summary>
    public BannerService()
    {
        _butBannerRep = new DbRepository<DbBanner>();
    }



    /// <summary>
    /// 获取列表
    /// </summary>
    public async Task<PagedList<DbBanner>> GetPagedListAsync(BannerInput input)
    {
        return await _butBannerRep
        .Where(u => u.IsDelete == false)
        .WhereIF(input.Key.IsNotNullOrEmpty(), u => u.Title != null && u.Title.Contains(input.Key!.Trim()))
        .WhereIF(input.Status > 0, u => u.Status == input.Status)
        .WhereIF(input.Position > 0, u => u.Position == input.Position)
        .OrderBy(u => u.Sort, OrderByType.Desc)
        .ToPagedListAsync(input);
    }

    /// <summary>
    /// 查看信息
    /// </summary>
    /// <returns></returns>
    public async Task<DbBanner> GetInfoAsync(long id)
    {
        return await _butBannerRep.FirstAsync(u => u.Id == id);
    }



    /// <summary>
    /// 新增信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<long> AddInfoAsync(BannerRequest request)
    {
        var busEntity = request.Adapt<DbBanner>();
        return await _butBannerRep.InsertReturnSnowflakeIdAsync(busEntity);
    }




    /// <summary>
    /// 更新信息
    /// </summary>
    /// <param name="busArticle"></param>
    /// <returns></returns>
    public async Task<bool> UpdateInfoAsync(BannerRequest request)
    {
        var data = request.Adapt<DbBanner>();
        var old = await _butBannerRep.FirstAsync(u => u.IsDelete == false && u.Id == data.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D1001);
        old.FillFromEntity(data);
        return await _butBannerRep.UpdateAsync(old);
    }




    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteInfoAsync(long id)
    {
        return await _butBannerRep.LogicDeleteAsync(u => u.Id == id);
    }



    /// <summary>
    /// banner列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbBanner>> GetListAsync(BannerInput input)
    {
        return await _butBannerRep
         .Where(u => u.Status == EnumBannerStatus.Release && u.IsDelete == false)
         .WhereIF(input.Position != null, u => u.Position == input.Position)
         .WhereIF(input.Status != null, u => u.Status == input.Status)
         .OrderBy(u => u.Sort, OrderByType.Asc)
         .ToListAsync();
    }






}
