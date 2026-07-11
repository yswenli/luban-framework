/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerApiFilter
*版本号： V1.0.0.0
*唯一标识：3504a3e2-9176-4ff8-b46a-19ec242acf4a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/6 13:25:28
*描述：swagger接口过滤
*
*=================================================
*修改标记
*修改时间：2024/11/6 13:25:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：swagger接口过滤
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// swagger接口过滤
/// </summary>
public class SwaggerApiFilter : IDocumentFilter
{
    /// <summary>
    /// swagger接口过滤
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (ApiDescription apiDescription in context.ApiDescriptions)
        {
            if (apiDescription.TryGetMethodInfo(out MethodInfo method))
            {
                if (method == null || method.ReflectedType == null) continue;
                if (method.ReflectedType.CustomAttributes.Any(t => t.AttributeType == typeof(SwaggerShowAttribute))
                        || method.CustomAttributes.Any(t => t.AttributeType == typeof(SwaggerShowAttribute)))
                {
                    continue;
                }

                if (method.ReflectedType.CustomAttributes.Any(t => t.AttributeType == typeof(SwaggerHiddenAttribute))
                    || method.CustomAttributes.Any(t => t.AttributeType == typeof(SwaggerHiddenAttribute)))
                {
                    string key = "/" + apiDescription.RelativePath;
                    if (key.Contains("?"))
                    {
                        int idx = key.IndexOf("?", StringComparison.Ordinal);
                        key = key.Substring(0, idx);
                    }
                    swaggerDoc.Paths.Remove(key);
                }
            }
        }
    }
}
