/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： InputArgsValidateActionFilterAttribute
*版本号： V1.0.0.0
*唯一标识：acd7203e-42a5-472a-b695-7668f2450ecc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/14 14:14:34
*描述：集成Aspnetcore的内置模型检查
*
*=================================================
*修改标记
*修改时间：2023/12/14 14:14:34
*修改人： yswenli
*版本号： V1.0.0.0
*描述：集成Aspnetcore的内置模型检查
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 集成Aspnetcore的内置模型检查
/// </summary>
public class InputArgsValidateActionFilterAttribute : BaseFilterAttribute, IOrderedFilter
{
    /// <summary>
    /// 排序
    /// </summary>
    public new int Order { private set; get; } = 0;

    /// <summary>
    /// 执行前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            //获取验证失败的模型字段
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .Select(e => e.Value?.Errors.First().ErrorMessage ?? "")
                .ToList();

            var str = string.Join("|", errors);

            //设置返回内容
            var failResult = new Fail(str);
            context.Result = new JsonResult(failResult);
            return;
        }
        await next.Invoke();
    }

    /// <summary>
    /// 执行后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}
