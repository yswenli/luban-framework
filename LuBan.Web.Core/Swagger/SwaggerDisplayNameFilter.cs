/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerDisplayNameFilter
*版本号： V1.0.0.0
*唯一标识：a1e7f8c2-3b4d-5e6f-7a8b-9c0d1e2f3a4b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/25
*描述：Swagger 显示 DisplayNameAttribute 特性值
*
*=================================================
*修改标记
*修改时间：2026/8/25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Swagger 显示 DisplayNameAttribute 特性值
*
*****************************************************************************/

using System.ComponentModel;

namespace LuBan.Web.Core.Swagger;

/// <summary>
/// Swagger 显示 DisplayNameAttribute 特性值
/// </summary>
public class SwaggerDisplayNameFilter : IOperationFilter
{
    /// <summary>
    /// 将 DisplayNameAttribute 的值应用到 Swagger 操作的 Summary
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo == null) return;

        var displayName = context.MethodInfo
            .GetCustomAttributes(typeof(DisplayNameAttribute), false)
            .FirstOrDefault() as DisplayNameAttribute;

        if (displayName != null && !string.IsNullOrWhiteSpace(displayName.DisplayName))
        {
            operation.Summary = displayName.DisplayName;
        }
    }
}
