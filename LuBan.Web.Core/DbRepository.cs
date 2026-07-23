/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Database
*文件名： DBRepository
*版本号： V1.0.0.0
*唯一标识：672a530f-91ac-481d-92f1-ec7b4a1b58ca
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:15:03
*描述：多租户db 实体仓储
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:15:03
*修改人： yswenli
*版本号： V1.0.0.0
*描述：多租户db 实体仓储
*
*****************************************************************************/

namespace LuBan.Web.Core;

/// <summary>
/// LuBan.Web.Core 多租户db 实体仓储
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class DbRepository<TEntity> : BaseRepository<TEntity>
    where TEntity : EntityBase, IDeletedFilter, new()
{
    /// <summary>
    ///  LuBan.Web.Core 多租户db 实体仓储
    /// </summary>
    public DbRepository() : base(GetTenantId())
    {

    }

    static string GetTenantId()
    {
        try
        {
            if (WebApp.HttpContext == null)
                return LuBanOrmConst.MainConfigId;
            var tenantId = WebApp.User.FindFirst(ClaimConst.TenantId)?.Value;
            var result = tenantId.IsNullOrEmpty() || tenantId == "0" ? LuBanOrmConst.MainConfigId : tenantId;
            return result;
        }
        catch
        {
            return LuBanOrmConst.MainConfigId;
        }
    }

    /// <summary>
    /// 切换到另一个仓储
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public new DbRepository<E> ChangeRepository<E>() where E : EntityBase, IDeletedFilter, new()
    {
        return base.ChangeRepository<DbRepository<E>>();
    }

    /// <summary>
    /// 切换到另一个仓储
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public new DbRepository<E> Change<E>() where E : EntityBase, IDeletedFilter, new()
    {
        return ChangeRepository<E>();
    }
}
