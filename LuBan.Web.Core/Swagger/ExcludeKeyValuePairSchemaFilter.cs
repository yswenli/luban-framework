/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： ExcludeKeyValuePairSchemaFilter
*版本号： V1.0.0.0
*唯一标识：1f79d7ba-33fa-4bb0-8da8-8d00dc712f65
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/27 16:05:56
*描述：阻止生成 KeyValuePair<string, StringValues> 的Schema过滤器
*
*=================================================
*修改标记
*修改时间：2025/10/27 16:05:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：阻止生成 KeyValuePair<string, StringValues> 的Schema过滤器
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// 阻止生成 KeyValuePair<string, StringValues> 的Schema过滤器
/// </summary>
public class ExcludeKeyValuePairSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Apply
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var targetType = typeof(KeyValuePair<string, StringValues>);

        if (context.Type != targetType || context.Type.FullName?.StartsWith("System.Collections.Generic.KeyValuePair") != true)
            return;

        schema.Reference = null;
        schema.Type = null;
        schema.Properties = null;
        schema.AdditionalPropertiesAllowed = false;

        var targetSchemaId = targetType.Name;
        var keysToRemove = context.SchemaRepository.Schemas
            .Where(kv => kv.Key.Equals(targetSchemaId, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            context.SchemaRepository.Schemas.Remove(key);
        }
    }
}
