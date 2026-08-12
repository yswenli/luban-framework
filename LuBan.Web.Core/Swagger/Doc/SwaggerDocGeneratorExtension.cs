/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger.Doc
*文件名： SwaggerDocGeneratorExtension
*版本号： V1.0.0.0
*唯一标识：74c2b935-2672-44bb-864f-4a407814dc63
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:20:36
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:20:36
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger.Doc;

/// <summary>
/// SwaggerDocGeneratorExtension服务
/// </summary>
public static class SwaggerDocGeneratorExtension
{
    /// <summary>
    /// 注册<see cref="ISwaggerDocGenerator"/>服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwaggerMarkdownDoc(this IServiceCollection services)
    {
        services.AddScoped<SwaggerGenerator>();
        services.AddScoped<ISwaggerDocGenerator, SwaggerDocGenerator>();
        return services;
    }
    /// <summary>
    /// 获取schema的引用ID
    /// </summary>
    /// <param name="openApiSchema"></param>
    /// <returns></returns>
    private static string? GetReferenceId(this IOpenApiSchema openApiSchema)
    {
        if (openApiSchema is OpenApiSchemaReference refSchema)
            return refSchema.Reference.Id;
        return null;
    }
    /// <summary>
    /// 判断是否为 Object 类型
    /// </summary>
    /// <param name="openApiSchema"></param>
    /// <param name="schemas"></param>
    /// <returns></returns>
    public static bool IsObject(this IOpenApiSchema openApiSchema, IDictionary<string, IOpenApiSchema> schemas)
    {
        if (openApiSchema.Type != null) return false;
        var refId = openApiSchema.GetReferenceId();
        if (refId == null) return false;
        var target = schemas.FirstOrDefault(x => x.Key == refId).Value;
        return (target?.Enum?.Count ?? 0) == 0;
    }
    /// <summary>
    /// 判断是否为枚举类型
    /// </summary>
    /// <param name="openApiSchema"></param>
    /// <param name="schemas"></param>
    /// <returns></returns>
    public static bool IsEnum(this IOpenApiSchema openApiSchema, IDictionary<string, IOpenApiSchema> schemas)
    {
        var refId = openApiSchema.GetReferenceId();
        if (refId == null) return false;
        var target = schemas.FirstOrDefault(x => x.Key == refId).Value;
        return (target?.Enum?.Count ?? 0) > 0;
    }
    /// <summary>
    /// 判断是否为数组类型
    /// </summary>
    /// <param name="openApiSchema"></param>
    /// <returns></returns>
    public static bool IsArray(this IOpenApiSchema openApiSchema)
    {
        return openApiSchema.Type == JsonSchemaType.Array && openApiSchema.Items != null;
    }
    /// <summary>
    /// 判断是否为基础数组类型
    /// </summary>
    /// <param name="openApiSchema"></param>
    /// <returns></returns>
    public static bool IsBaseTypeArray(this IOpenApiSchema openApiSchema)
    {
        return openApiSchema.Type == JsonSchemaType.Array
            && openApiSchema.Items != null
            && openApiSchema.Items.Type != null
            && openApiSchema.Items is not OpenApiSchemaReference;
    }

    /// <summary>
    /// 判断是否为基本类型
    /// </summary>
    /// <param name="openApiSchema"></param>
    public static bool IsBaseType(this IOpenApiSchema openApiSchema)
    {
        return openApiSchema.Type != null;
    }
}
