/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerSafeComparisonFilter
*版本号： V1.0.0.0
*唯一标识：031bced5-c17a-4d7a-aa5f-b427ff2a7753
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/6/7 19:35:57
*描述：
*
*=================================================
*修改标记
*修改时间：2023/6/7 19:35:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Web.Core.Swagger;

/// <summary>
/// swagger中安全较验header值
/// </summary>
public class SwaggerSafeComparisonFilter : IOperationFilter
{
    /// <summary>
    /// 校验headers
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var appOptions = HostingOptions.Default.AppOptions;

        //禁用了安全参数检查
        if (appOptions.DisableSafeComparisonFilter)
        {
            return;
        }

        var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
        var isSafeComparison = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AraParameterFilterAttribute);
        var allowUnSafeComparison = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is NoAraParameterFilterAttribute);
        var allowUnSafe = false;
        var metas = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metas != null)
            foreach (var item in metas)
            {
                if (item is NoAraParameterFilterAttribute)
                {
                    allowUnSafe = true;
                }
            }


        if (isSafeComparison && !allowUnSafeComparison && !allowUnSafe)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "timestamp",
                In = ParameterLocation.Header,
                Description = "时间戳，用于校验有效期",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString(Common.DateTimeUtil.Now.ToUnixTimeStamp(false).ToString())
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "nonce",
                In = ParameterLocation.Header,
                Description = "随机码，用于防重",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString(GuidUtil.New)
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "signature",
                In = ParameterLocation.Header,
                Description = "内容md5签名,注意签名规则",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("yswenli".GetMD5Str())
                }
            });
        }
    }
}
