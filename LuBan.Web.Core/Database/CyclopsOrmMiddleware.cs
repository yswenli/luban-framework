/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Database
*文件名： LuBanOrmMiddleware
*版本号： V1.0.0.0
*唯一标识：b8b768c0-d09f-4fff-a2af-91142a5102d0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/12 19:22:48
*描述：LuBanOrmMiddleware,数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
*
*=================================================
*修改标记
*修改时间：2025/11/12 19:22:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanOrmMiddleware,数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
*
*****************************************************************************/
namespace LuBan.Web.Core.Database;

/// <summary>
/// LuBanOrmMiddleware,
/// 数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
/// </summary>
/// <param name="next"></param>
internal class LuBanOrmMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Invoke,
    /// 数据范围权限： 仅本人数据、本部门数据、本部门及以下数据、本机构数据、本机构及以下数据、全部数据
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            //如果当前控制器或方法标记了IgnoreDataScopePermissionAttribute的，则不进行数据范围权限过滤
            if (context.GetEndpoint()?.Metadata.GetMetadata<IgnoreDataScopePermissionAttribute>() != null)
            {
                return;
            }
            if (ServiceProviderUtil.GetRequiredService<ISqlSugarClient>() is not SqlSugarScope client)
                throw new Exception("LuBanOrm初始化失败，未配置LuBanORM");
            client.CurrentConnectionConfig.IsAutoCloseConnection = true;
            var provider = client.GetConnectionScope(client.CurrentConnectionConfig.ConfigId);
            DataScopePermissionFilter.SetOrgDataScopeFilter(provider!);
        }
        catch (Exception ex)
        {
            Logger.Error("数据范围权限过滤器异常", ex);
        }
        finally
        {
            await _next(context);
        }
    }
}
