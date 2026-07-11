/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerFileUploadFilter
*版本号： V1.0.0.0
*唯一标识：75e90abb-2348-40dd-9a39-d61fdabaf5d7
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/22 9:08:38
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/22 9:08:38
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// 上传文件参数
/// </summary>
public class SwaggerFileUploadFilter : IOperationFilter
{
    /// <summary>
    /// 添加上传文件参数
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.Parameters.Any(w => w.ParameterType == typeof(IFormFile)))
        {
            Dictionary<string, OpenApiSchema> schema = new Dictionary<string, OpenApiSchema>();

            schema["file"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };

            Dictionary<string, OpenApiMediaType> content = new Dictionary<string, OpenApiMediaType>();

            content["multipart/form-data"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "object", Properties = schema } };

            operation.RequestBody = new OpenApiRequestBody() { Content = content };

        }
        else if (context.ApiDescription.ActionDescriptor.Parameters.Any(w => w.ParameterType == typeof(IFormCollection) || w.ParameterType == typeof(List<IFormFile>) || w.ParameterType == typeof(IFormFileCollection)))
        {
            Dictionary<string, OpenApiSchema> schema = new Dictionary<string, OpenApiSchema>();

            schema["file1"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };
            schema["file2"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };
            schema["file3"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };

            Dictionary<string, OpenApiMediaType> content = new Dictionary<string, OpenApiMediaType>();

            content["multipart/form-data"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "object", Properties = schema } };

            operation.RequestBody = new OpenApiRequestBody() { Content = content };

        }
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(q => q is SwaggerUIUploadFileAttribute))
        {
            Dictionary<string, OpenApiSchema> schema = new Dictionary<string, OpenApiSchema>();

            schema["file"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };

            Dictionary<string, OpenApiMediaType> content = new Dictionary<string, OpenApiMediaType>();

            content["multipart/form-data"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "object", Properties = schema } };

            operation.RequestBody = new OpenApiRequestBody() { Content = content };
        }

        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(q => q is SwaggerUIUploadFilesAttribute))
        {
            Dictionary<string, OpenApiSchema> schema = new Dictionary<string, OpenApiSchema>();

            schema["file1"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };
            schema["file2"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };
            schema["file3"] = new OpenApiSchema { Description = "Select file", Type = "string", Format = "binary" };

            Dictionary<string, OpenApiMediaType> content = new Dictionary<string, OpenApiMediaType>();

            content["multipart/form-data"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "object", Properties = schema } };

            operation.RequestBody = new OpenApiRequestBody() { Content = content };
        }

    }
}

/// <summary>
/// 在swagger界面上显示上传文件
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SwaggerUIUploadFileAttribute : Attribute
{

}
/// <summary>
/// 在swagger界面上显示上传多个文件
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SwaggerUIUploadFilesAttribute : Attribute
{

}
