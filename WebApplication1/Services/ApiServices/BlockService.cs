/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： BlockService
*版本号： V1.0.0.0
*唯一标识：04c5ac43-8f7b-4f31-b90f-2be3cea356be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 18:51:19
*描述：内容业务
*
*=================================================
*修改标记
*修改时间：2023/12/6 18:51:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：内容业务
*
*****************************************************************************/
using WebApplication1.Models.Entities;
using WebApplication1.Models.Vos;

namespace Services.ApiServices;

/// <summary>
/// 内容业务
/// </summary>
public class BlockService : BaseService<BlockService>
{
    private DbRepository<DbBlock> _busBlockRep => new();

    /// <summary>
    /// 获取内容列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<DbBlock>> GetPagedListAsync(BlockPagedInput input)
    {
        var list = await _busBlockRep
            .WhereIF(input.Key.IsNotNullOrWhiteSpace(), u => u.BlockName != null && u.BlockName.Contains(input.Key!.Trim()))
            .WhereIF(input.Pid != null, u => u.Pid == input.Pid)
            .WhereIF(input.Level != null, u => u.Level == input.Level)
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .WhereIF(input.BlockType.IsNotNullOrEmpty(), u => u.BlockType == input.BlockType)
            .Where(u => u.IsDelete == false)
            .OrderBy(u => u.Sort, OrderByType.Desc).ToListAsync();

        return await _busBlockRep
            .WhereIF(input.Key.IsNotNullOrWhiteSpace(), u => u.BlockName != null && u.BlockName.Contains(input.Key!.Trim()))
            .WhereIF(input.Pid != null, u => u.Pid == input.Pid)
            .WhereIF(input.Level != null, u => u.Level == input.Level)
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .WhereIF(input.BlockType.IsNotNullOrEmpty(), u => u.BlockType == input.BlockType)
            .Where(u => u.IsDelete == false)
            .OrderBy(u => u.Sort, OrderByType.Desc)
            .ToPagedTreeListAsync(input, q => q.Id, q => q.SubBlocks, q => q.Pid, input.Id ?? 0);
    }

    /// <summary>
    /// 查看栏目信息
    /// </summary>
    /// <returns></returns>
    public async Task<DbBlock> GetInfoAsync(long id)
    {
        return await _busBlockRep.FirstAsync(u => u.Id == id);
    }

    /// <summary>
    /// 新增栏目信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<long> AddInfoAsync(BlockInfo request)
    {
        var busArticle = request.Adapt<DbBlock>();
        return await _busBlockRep.InsertReturnSnowflakeIdAsync(busArticle);
    }

    /// <summary>
    /// 更新信息
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<bool> UpdateInfoAsync(BlockInfo data)
    {
        var notice = data.Adapt<DbBlock>();
        var old = await _busBlockRep.FirstAsync(u => u.IsDelete == false && u.Id == data.Id) ?? throw FriendlyError.Ex(EnumErrorCode.D1001);
        old.FillFrom(data);
        return await _busBlockRep.UpdateAsync(old);
    }

}
