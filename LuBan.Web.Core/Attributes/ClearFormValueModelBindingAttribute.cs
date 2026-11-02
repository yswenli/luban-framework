/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
    /// <summary>
    /// 资源执行完成后不做任何处理，表单值提供器已在执行前移除。
    /// </summary>
    /// <param name="context">资源执行完成后的上下文。</param>
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
