/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： ClearFormValueModelBindingAttribute
*版本号： V1.0.0.0
*唯一标识：ffe408a9-bdf0-4b02-bc79-0adbbbe4fe2c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/10 17:38:27
*描述：清除绑定表单
*
*=================================================
*修改标记
*修改时间：2024/9/10 17:38:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：清除绑定表单
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 清除绑定表单
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ClearFormValueModelBindingAttribute : Attribute, IAsyncResourceFilter, IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {

    }

    /// <summary>
    /// 清除绑定表单
    /// </summary>
    /// <param name="context"></param>
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    /// <summary>
    /// 清除绑定表单
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
        await next();
    }
}
