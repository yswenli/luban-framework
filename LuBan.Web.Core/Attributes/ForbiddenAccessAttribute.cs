/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Web.Core.Attributes
*文件名： ForbiddenAccessAtrribute
*版本号： V1.0.0.0
*唯一标识：86f234e6-c9d3-4449-b62e-16a2c5457a5e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/12 11:40:00
*描述：禁止某些角色访问
*
*=================================================
*修改标记
*修改时间：2024/11/12 11:40:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：禁止某些角色访问
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 禁止某些角色或成员访问,可以用AllowAccessAttribute例外
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ForbiddenAccessAttribute : BaseFilterAttribute, IOrderedFilter
{
    /// <summary>
    /// 禁止某些角色访问,可以用AllowAccessAttribute例外
    /// </summary>
    public long[] ForbiddenRoles { get; set; }

    /// <summary>
    /// 禁止某些角色访问,可以用AllowAccessAttribute例外，
    /// 不指定角色的默认放行，排除无角色用户
    /// </summary>
    public ForbiddenAccessAttribute()
    {
        ForbiddenRoles = [];
    }

    /// <summary>
    /// 禁止某些角色访问,可以用AllowAccessAttribute例外
    /// </summary>
    /// <param name="forbiddenRoles"></param>
    public ForbiddenAccessAttribute(params long[] forbiddenRoles)
    {
        ForbiddenRoles = forbiddenRoles;
    }

    /// <summary>
    /// 检查当前用户是否拥有访问权限，若标记AllowAccessAttribute则跳过
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //检查无需校验标签
        if (context.HasAttribute<AllowAccessAttribute>())
        {
            await next.Invoke();
            return;
        }
        //拦截未登录
        if (SessionUser.UserId <= 0)
        {
            context.Result = new ForbidResult();
            return;
        }
        //超级管理员直接放行
        if (SessionUser.UserId == LuBanOrmConst.SuperAdminId)
        {
            await next.Invoke();
            return;
        }

        //拦截无角色用户
        var roles = await new BaseRepository<DbUserRole>().ListAsync(q => q.UserId == SessionUser.UserId);
        if (roles == null || roles.Count < 1)
        {
            context.Result = new ForbidResult();
            return;
        }
        //获取禁止访问的角色
        if (ForbiddenRoles == null || ForbiddenRoles.Length == 0)
        {
            try
            {
                ForbiddenRoles = WebApp.ServiceCache.GetOrSet<long[]>(CacheConst.KeyForbiddenAccessRoles, (k) =>
                {
                    var vals = new DbRepository<DbConfig>().First(q => q.Code == CommonConst.SysForbiddenAccessRolesCode);
                    if (vals != null)
                    {
                        return (vals?.Value ?? "").Split(',').Select(q => long.Parse(q)).ToArray();
                    }
                    return [];
                }) ?? [];
            }
            catch
            {
                ForbiddenRoles = [];
            }
        }

        //默认放行
        if (ForbiddenRoles == null || ForbiddenRoles.Length == 0)
        {
            await next.Invoke();
            return;
        }
        else
        {
            //检查当前用户的角色是否在禁止列表中
            if (roles.Exists(q => ForbiddenRoles.Contains(q.RoleId)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
        await next.Invoke();
    }

    /// <summary>
    /// OnResultExecutionAsync
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}
