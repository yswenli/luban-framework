/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： RemoveGenericSchemaDocumentFilter
*版本号： V1.0.0.0
*唯一标识：8c01c8b4-81bf-4e9e-895b-244daf5b968d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/27 16:20:54
*描述：移除KeyValuePair`2结构的文档
*
*=================================================
*修改标记
*修改时间：2025/10/27 16:20:54
*修改人： yswenli
*版本号： V1.0.0.0
*描述：移除KeyValuePair`2结构的文档
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// 移除KeyValuePair`2结构的文档
/// </summary>
public class RemoveGenericSchemaDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// Apply
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var keysToRemove = swaggerDoc.Components.Schemas
            .Where(kv => kv.Key.StartsWith("System.Collections.Generic.KeyValuePair`2") || kv.Key.Contains("`2"))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(key);
        }
    }
}