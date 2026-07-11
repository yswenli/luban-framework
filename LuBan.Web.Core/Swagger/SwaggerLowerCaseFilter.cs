/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerLowerCaseAttribute
*版本号： V1.0.0.0
*唯一标识：c79d8378-8b3e-4419-89ab-15433ca698f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/6 11:46:33
*描述：swagger接口文档中url小写
*
*=================================================
*修改标记
*修改时间：2024/11/6 11:46:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：swagger接口文档中url小写
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// swagger接口文档中url小写
/// </summary>
public class SwaggerLowerCaseFilter : IDocumentFilter
{
    /// <summary>
    /// swagger接口文档中url小写
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths;
        var list = new OpenApiPaths();
        foreach (var path in paths)
        {
            list.TryAdd(path.Key.ToLower(), path.Value);
        }
        swaggerDoc.Paths.Clear();
        swaggerDoc.Paths = list;
    }
}
